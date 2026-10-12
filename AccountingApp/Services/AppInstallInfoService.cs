using AccountingApp.Core.Services;

namespace AccountingApp.Services;

public interface IAppInstallInfoService
{
    string GetVersionText();
    string GetExpirationDateText();
    DateTimeOffset? GetExpirationDate();
}

public class AppInstallInfoService : IAppInstallInfoService
{
    private const string UnknownExpiration = "無法判定";

    public string GetVersionText()
    {
        var version = AppInfo.Current.VersionString;
        var build = AppInfo.Current.BuildString;
        return string.IsNullOrWhiteSpace(build) ? version : $"{version} ({build})";
    }

    public string GetExpirationDateText()
    {
        var expirationDate = GetExpirationDate();
        return expirationDate is null
            ? UnknownExpiration
            : AppProvisionInfoService.FormatExpirationDateText(expirationDate.Value);
    }

    public DateTimeOffset? GetExpirationDate() => TryGetProvisionExpirationDate();

    private static DateTimeOffset? TryGetProvisionExpirationDate()
    {
        try
        {
            using var stream = FileSystem.Current.OpenAppPackageFileAsync("embedded.mobileprovision").GetAwaiter().GetResult();
            using var reader = new StreamReader(stream);
            return AppProvisionInfoService.TryGetExpirationDate(reader.ReadToEnd());
        }
        catch
        {
        }

        return null;
    }
}
