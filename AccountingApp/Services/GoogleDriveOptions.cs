namespace AccountingApp.Services;

public static class GoogleDriveOptions
{
    // App-level OAuth client id. End users do not need to input this.
    public const string ClientId = "350597496746-s34tikf6j2aefsc59kpjro3q3ile0f0b.apps.googleusercontent.com";
    public const string RedirectScheme = "com.googleusercontent.apps.350597496746-s34tikf6j2aefsc59kpjro3q3ile0f0b";
    public const string RedirectUri = "com.googleusercontent.apps.350597496746-s34tikf6j2aefsc59kpjro3q3ile0f0b:/oauth2redirect";
    public const string Scopes = "https://www.googleapis.com/auth/drive";
    public const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    public const string TokenEndpoint = "https://oauth2.googleapis.com/token";
}
