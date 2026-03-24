using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AccountingApp.Core.Abstractions;
using AccountingApp.Core.Services;
using Microsoft.Maui.Authentication;

namespace AccountingApp.Services;

public class GoogleDriveService : IGoogleDriveFileApi
{
    private const string DefaultBackupFolderName = "personaccount_backup";
    private const string FolderIdKey = "google_drive_folder_id";
    private const string FolderNameKey = "google_drive_folder_name";
    private const string AccessTokenKey = "google_drive_access_token";
    private const string RefreshTokenKey = "google_drive_refresh_token";
    private const string AccessTokenExpiryKey = "google_drive_access_token_expiry_utc";
    private const string GrantedScopeKey = "google_drive_granted_scope";

    private readonly DatabaseService _databaseService;
    private readonly HttpClient _httpClient;

    public GoogleDriveService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public string? FolderId => Preferences.Get(FolderIdKey, null);
    public string? FolderName => Preferences.Get(FolderNameKey, null);

    public sealed record GoogleDriveFileItem(string Id, string Name);

    public async Task AuthorizeAsync()
    {
        EnsureScopeConsistency();

        if (await HasValidAccessTokenAsync())
            return;

        await RunOAuthLoginAsync();
    }

    public async Task PickFolderAsync()
    {
        await AuthorizeAsync();

        await EnsureDefaultBackupFolderSelectedAsync();
    }

    public async Task UploadAsync()
    {
        var folderId = await EnsureConfiguredAsync();
        var coordinator = new GoogleDriveBackupCoordinator(this);

        try
        {
            await coordinator.BackupAsync(folderId, () => File.OpenRead(_databaseService.DatabasePath));
        }
        catch (HttpRequestException ex)
        {
            throw MapHttpException(ex);
        }
    }

    public async Task RestoreAsync()
    {
        var folderId = await EnsureConfiguredAsync();
        var coordinator = new GoogleDriveBackupCoordinator(this);

        try
        {
            await coordinator.RestoreAsync(folderId, async downloaded =>
            {
                var localPath = _databaseService.DatabasePath;
                await using var output = File.Create(localPath);
                await downloaded.CopyToAsync(output);
                await output.FlushAsync();
                await _databaseService.InitializeAsync();
            });
        }
        catch (HttpRequestException ex)
        {
            throw MapHttpException(ex);
        }
    }

    public void ClearAuth()
    {
        Preferences.Remove(AccessTokenKey);
        Preferences.Remove(RefreshTokenKey);
        Preferences.Remove(AccessTokenExpiryKey);
        Preferences.Remove(GrantedScopeKey);
        Preferences.Remove(FolderIdKey);
        Preferences.Remove(FolderNameKey);
    }

    public async Task<string?> FindFileIdAsync(string folderId, string fileName, CancellationToken cancellationToken = default)
    {
        await EnsureAuthorizedHeaderAsync();

        var escapedName = fileName.Replace("'", "\\'");
        var escapedFolder = folderId.Replace("'", "\\'");
        var q = $"name='{escapedName}' and '{escapedFolder}' in parents and trashed=false";
        var url = $"https://www.googleapis.com/drive/v3/files?q={Uri.EscapeDataString(q)}&fields=files(id,name)&pageSize=1&includeItemsFromAllDrives=true&supportsAllDrives=true";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(response);

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
        var files = doc.RootElement.TryGetProperty("files", out var value) ? value : default;

        if (files.ValueKind != JsonValueKind.Array || files.GetArrayLength() == 0)
            return null;

        var file = files[0];
        return file.TryGetProperty("id", out var idElement) ? idElement.GetString() : null;
    }

    public async Task CreateFileAsync(string folderId, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        await EnsureAuthorizedHeaderAsync();

        var metadataJson = JsonSerializer.Serialize(new
        {
            name = fileName,
            parents = new[] { folderId }
        });

        using var multipart = new MultipartContent("related", $"batch_{Guid.NewGuid():N}");
        multipart.Add(new StringContent(metadataJson, Encoding.UTF8, "application/json"));

        var data = new StreamContent(content);
        data.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        multipart.Add(data);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart&supportsAllDrives=true")
        {
            Content = multipart
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task UpdateFileAsync(string fileId, Stream content, CancellationToken cancellationToken = default)
    {
        await EnsureAuthorizedHeaderAsync();
        content.Position = 0;

        using var request = new HttpRequestMessage(
            HttpMethod.Patch,
            $"https://www.googleapis.com/upload/drive/v3/files/{Uri.EscapeDataString(fileId)}?uploadType=media&supportsAllDrives=true")
        {
            Content = new StreamContent(content)
        };

        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    public async Task<Stream> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default)
    {
        await EnsureAuthorizedHeaderAsync();

        using var response = await _httpClient.GetAsync(
            $"https://www.googleapis.com/drive/v3/files/{Uri.EscapeDataString(fileId)}?alt=media&supportsAllDrives=true",
            cancellationToken);
        await EnsureSuccessAsync(response);

        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return new MemoryStream(bytes);
    }

    public async Task<IReadOnlyList<GoogleDriveFileItem>> ListBackupFolderFilesAsync(CancellationToken cancellationToken = default)
    {
        var folderId = await EnsureConfiguredAsync();
        await EnsureAuthorizedHeaderAsync();

        var escapedFolder = folderId.Replace("'", "\\'");
        var q = $"'{escapedFolder}' in parents and trashed=false";
        var url = $"https://www.googleapis.com/drive/v3/files?q={Uri.EscapeDataString(q)}&fields=files(id,name)&orderBy=name&pageSize=100&includeItemsFromAllDrives=true&supportsAllDrives=true";

        using var response = await _httpClient.GetAsync(url, cancellationToken);
        await EnsureSuccessAsync(response);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!doc.RootElement.TryGetProperty("files", out var files) || files.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return files.EnumerateArray()
            .Select(file => new GoogleDriveFileItem(
                file.TryGetProperty("id", out var idValue) ? idValue.GetString() ?? string.Empty : string.Empty,
                file.TryGetProperty("name", out var nameValue) ? nameValue.GetString() ?? string.Empty : string.Empty))
            .Where(file => !string.IsNullOrWhiteSpace(file.Id) && !string.IsNullOrWhiteSpace(file.Name))
            .ToList();
    }

    public static IReadOnlyList<GoogleDriveFileItem> FilterCsvFiles(IEnumerable<GoogleDriveFileItem> files) =>
        files.Where(file => file.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)).ToList();

    private async Task<string> EnsureConfiguredAsync()
    {
        await AuthorizeAsync();

        var folderId = Preferences.Get(FolderIdKey, null);
        if (!string.IsNullOrWhiteSpace(folderId))
            return folderId;

        throw new InvalidOperationException("尚未設定 Google Drive 備份資料夾，請先到設定頁選擇或建立資料夾。");
    }

    private async Task<List<DriveFolder>> ListFoldersAsync()
    {
        await EnsureAuthorizedHeaderAsync();

        const string q = "mimeType='application/vnd.google-apps.folder' and trashed=false and 'root' in parents";
        var url = $"https://www.googleapis.com/drive/v3/files?q={Uri.EscapeDataString(q)}&fields=files(id,name)&orderBy=name&pageSize=20&includeItemsFromAllDrives=true&supportsAllDrives=true";
        using var response = await _httpClient.GetAsync(url);
        await EnsureSuccessAsync(response);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        if (!doc.RootElement.TryGetProperty("files", out var files) || files.ValueKind != JsonValueKind.Array)
            return [];

        var result = new List<DriveFolder>();
        foreach (var file in files.EnumerateArray())
        {
            var id = file.TryGetProperty("id", out var idValue) ? idValue.GetString() : null;
            var name = file.TryGetProperty("name", out var nameValue) ? nameValue.GetString() : null;
            if (!string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(name))
                result.Add(new DriveFolder(id!, name!));
        }

        return result;
    }

    private async Task<string> CreateFolderAsync(string folderName)
    {
        await EnsureAuthorizedHeaderAsync();

        var metadataJson = JsonSerializer.Serialize(new
        {
            name = folderName,
            mimeType = "application/vnd.google-apps.folder"
        });

        using var content = new StringContent(metadataJson, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(
            "https://www.googleapis.com/drive/v3/files?fields=id,name&supportsAllDrives=true",
            content);
        await EnsureSuccessAsync(response);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var id = doc.RootElement.TryGetProperty("id", out var idValue) ? idValue.GetString() : null;
        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidOperationException("建立 Google Drive 資料夾失敗。");

        return id;
    }

    private async Task EnsureAuthorizedHeaderAsync()
    {
        if (!await HasValidAccessTokenAsync())
            await AuthorizeAsync();

        var token = Preferences.Get(AccessTokenKey, null);

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Google Drive 尚未授權。");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException(body, null, response.StatusCode);
    }

    private static Exception MapHttpException(HttpRequestException ex)
    {
        return ex.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new InvalidOperationException("Google Drive 授權已失效，請重新授權。"),
            HttpStatusCode.Forbidden => new InvalidOperationException("Google Drive 權限不足，請確認 scope 包含 drive.file。"),
            HttpStatusCode.NotFound => new FileNotFoundException("Google Drive 備份檔不存在，請先執行備份。"),
            _ => new InvalidOperationException($"Google Drive 操作失敗：{ex.Message}")
        };
    }

    private static Page GetPage() => Application.Current!.Windows[0].Page!;

    private async Task RunOAuthLoginAsync()
    {
        EnsureOAuthConfigured();

        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = Guid.NewGuid().ToString("N");

        var authUri = new Uri(
            $"{GoogleDriveOptions.AuthorizationEndpoint}" +
            $"?client_id={Uri.EscapeDataString(GoogleDriveOptions.ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(GoogleDriveOptions.RedirectUri)}" +
            $"&response_type=code" +
            $"&scope={Uri.EscapeDataString(GoogleDriveOptions.Scopes)}" +
            $"&code_challenge={Uri.EscapeDataString(codeChallenge)}" +
            $"&code_challenge_method=S256" +
            $"&access_type=offline" +
            $"&prompt=consent" +
            $"&state={Uri.EscapeDataString(state)}");

        var callbackUri = new Uri(GoogleDriveOptions.RedirectUri);
        WebAuthenticatorResult authResult;
        try
        {
            authResult = await WebAuthenticator.Default.AuthenticateAsync(authUri, callbackUri);
        }
        catch (TaskCanceledException)
        {
            throw new InvalidOperationException("已取消 Google Drive 授權。");
        }

        if (!authResult.Properties.TryGetValue("state", out var returnedState) || returnedState != state)
            throw new InvalidOperationException("Google Drive 授權狀態驗證失敗。");

        if (!authResult.Properties.TryGetValue("code", out var code) || string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("Google Drive 授權失敗，未取得授權碼。");

        await ExchangeCodeForTokenAsync(code, codeVerifier);
    }

    private async Task ExchangeCodeForTokenAsync(string code, string codeVerifier)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = GoogleDriveOptions.ClientId,
            ["redirect_uri"] = GoogleDriveOptions.RedirectUri,
            ["grant_type"] = "authorization_code",
            ["code_verifier"] = codeVerifier
        });

        using var response = await _httpClient.PostAsync(GoogleDriveOptions.TokenEndpoint, content);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Google OAuth Token 交換失敗：{body}");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        var accessToken = root.TryGetProperty("access_token", out var at) ? at.GetString() : null;
        var refreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null;
        var expiresIn = root.TryGetProperty("expires_in", out var ei) ? ei.GetInt32() : 3600;

        if (string.IsNullOrWhiteSpace(accessToken))
            throw new InvalidOperationException("Google OAuth 未回傳 access token。");

        Preferences.Set(AccessTokenKey, accessToken);
        if (!string.IsNullOrWhiteSpace(refreshToken))
            Preferences.Set(RefreshTokenKey, refreshToken);
        Preferences.Set(AccessTokenExpiryKey, DateTimeOffset.UtcNow.AddSeconds(expiresIn).ToString("O"));
        Preferences.Set(GrantedScopeKey, GoogleDriveOptions.Scopes);
    }

    private async Task<bool> HasValidAccessTokenAsync()
    {
        var accessToken = Preferences.Get(AccessTokenKey, null);
        if (string.IsNullOrWhiteSpace(accessToken))
            return false;

        var expiryRaw = Preferences.Get(AccessTokenExpiryKey, null);
        if (DateTimeOffset.TryParse(expiryRaw, out var expiry))
        {
            if (DateTimeOffset.UtcNow < expiry.AddMinutes(-2))
                return true;
        }
        else
        {
            return true;
        }

        return await TryRefreshTokenAsync();
    }

    private async Task<bool> TryRefreshTokenAsync()
    {
        var refreshToken = Preferences.Get(RefreshTokenKey, null);
        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        EnsureOAuthConfigured();

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = GoogleDriveOptions.ClientId,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        });

        using var response = await _httpClient.PostAsync(GoogleDriveOptions.TokenEndpoint, content);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            return false;

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        var accessToken = root.TryGetProperty("access_token", out var at) ? at.GetString() : null;
        var expiresIn = root.TryGetProperty("expires_in", out var ei) ? ei.GetInt32() : 3600;

        if (string.IsNullOrWhiteSpace(accessToken))
            return false;

        Preferences.Set(AccessTokenKey, accessToken);
        Preferences.Set(AccessTokenExpiryKey, DateTimeOffset.UtcNow.AddSeconds(expiresIn).ToString("O"));
        Preferences.Set(GrantedScopeKey, GoogleDriveOptions.Scopes);
        return true;
    }

    private static void EnsureScopeConsistency()
    {
        var grantedScope = Preferences.Get(GrantedScopeKey, string.Empty);
        if (!string.IsNullOrWhiteSpace(grantedScope) &&
            !string.Equals(grantedScope, GoogleDriveOptions.Scopes, StringComparison.Ordinal))
        {
            Preferences.Remove(AccessTokenKey);
            Preferences.Remove(RefreshTokenKey);
            Preferences.Remove(AccessTokenExpiryKey);
            Preferences.Remove(GrantedScopeKey);
        }
    }

    private static void EnsureOAuthConfigured()
    {
        if (string.IsNullOrWhiteSpace(GoogleDriveOptions.ClientId) ||
            GoogleDriveOptions.ClientId.Contains("YOUR_GOOGLE_CLIENT_ID", StringComparison.Ordinal))
            throw new InvalidOperationException("請先在 GoogleDriveOptions 設定 OAuth Client ID。");
    }

    private static string GenerateCodeVerifier()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private sealed record DriveFolder(string Id, string Name);

    public sealed record FolderOption(string Id, string Name);

    public async Task<IReadOnlyList<FolderOption>> GetFolderOptionsAsync()
    {
        await AuthorizeAsync();
        var folders = await ListFoldersAsync();
        return folders.Select(f => new FolderOption(f.Id, f.Name)).ToList();
    }

    public Task SelectFolderAsync(string folderId, string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderId))
            throw new InvalidOperationException("資料夾 Id 不能為空。");

        Preferences.Set(FolderIdKey, folderId.Trim());
        Preferences.Set(FolderNameKey, string.IsNullOrWhiteSpace(folderName) ? folderId.Trim() : folderName.Trim());
        return Task.CompletedTask;
    }

    public async Task<string> CreateFolderAndSelectAsync(string folderName)
    {
        if (string.IsNullOrWhiteSpace(folderName))
            throw new InvalidOperationException("資料夾名稱不能為空。");

        await AuthorizeAsync();
        var newId = await CreateFolderAsync(folderName.Trim());
        await SelectFolderAsync(newId, folderName.Trim());
        return newId;
    }

    public async Task EnsureDefaultBackupFolderSelectedAsync()
    {
        await AuthorizeAsync();
        var folders = await ListFoldersAsync();
        var existing = folders.FirstOrDefault(f =>
            string.Equals(f.Name, DefaultBackupFolderName, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            await SelectFolderAsync(existing.Id, existing.Name);
            return;
        }

        await CreateFolderAndSelectAsync(DefaultBackupFolderName);
    }
}
