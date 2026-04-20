using System.Globalization;
using System.Xml.Linq;

namespace AccountingApp.Core.Services;

public static class AppProvisionInfoService
{
    private static readonly TimeSpan GmtPlusEightOffset = TimeSpan.FromHours(8);

    public static DateTimeOffset? TryGetExpirationDate(string provisionContent)
    {
        var plistStart = provisionContent.IndexOf("<plist", StringComparison.Ordinal);
        var plistEnd = provisionContent.IndexOf("</plist>", StringComparison.Ordinal);
        if (plistStart < 0 || plistEnd < plistStart)
        {
            return null;
        }

        var plistContent = provisionContent[plistStart..(plistEnd + "</plist>".Length)];
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
                DateTimeOffset.TryParse(
                    elements[i + 1].Value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var expirationDate))
            {
                return expirationDate;
            }
        }

        return null;
    }

    public static string FormatExpirationDateText(DateTimeOffset expirationDate)
    {
        return expirationDate
            .ToOffset(GmtPlusEightOffset)
            .ToString("yyyy/MM/dd HH:mm:ss 'GMT'zzz", CultureInfo.InvariantCulture);
    }
}
