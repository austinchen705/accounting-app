using System.Globalization;
using System.Xml.Linq;

namespace AccountingApp.Services;

public interface IAppInstallInfoService
{
    string GetVersionText();
    string GetExpirationDateText();
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
        var expirationDate = TryGetProvisionExpirationDate();
        return expirationDate?.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) ?? UnknownExpiration;
    }

    private static DateTime? TryGetProvisionExpirationDate()
    {
        try
        {
            using var stream = FileSystem.Current.OpenAppPackageFileAsync("embedded.mobileprovision").GetAwaiter().GetResult();
            using var reader = new StreamReader(stream);
            var content = reader.ReadToEnd();

            var plistStart = content.IndexOf("<plist", StringComparison.Ordinal);
            var plistEnd = content.IndexOf("</plist>", StringComparison.Ordinal);
            if (plistStart < 0 || plistEnd < plistStart)
            {
                return null;
            }

            var plistContent = content[plistStart..(plistEnd + "</plist>".Length)];
            var document = XDocument.Parse(plistContent);
            var dict = document.Root?.Element("dict");
            if (dict is null)
            {
                return null;
            }

            var elements = dict.Elements().ToList();
            for (var i = 0; i < elements.Count - 1; i++)
            {
                if (elements[i].Name.LocalName == "key" &&
                    string.Equals(elements[i].Value, "ExpirationDate", StringComparison.Ordinal) &&
                    elements[i + 1].Name.LocalName == "date" &&
                    DateTime.TryParse(elements[i + 1].Value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expirationDate))
                {
                    return expirationDate;
                }
            }
        }
        catch
        {
        }

        return null;
    }
}
