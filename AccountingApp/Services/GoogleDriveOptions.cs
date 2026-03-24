namespace AccountingApp.Services;

public static class GoogleDriveOptions
{
    // App-level OAuth client id. End users do not need to input this.
    public const string ClientId = "147395096355-ds9167aem2bbduvndcp3n7nene7thqpl.apps.googleusercontent.com";
    public const string RedirectScheme = "com.googleusercontent.apps.147395096355-ds9167aem2bbduvndcp3n7nene7thqpl";
    public const string RedirectUri = "com.googleusercontent.apps.147395096355-ds9167aem2bbduvndcp3n7nene7thqpl:/oauth2redirect";
    public const string Scopes = "https://www.googleapis.com/auth/drive";
    public const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    public const string TokenEndpoint = "https://oauth2.googleapis.com/token";
}
