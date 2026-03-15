using System.Globalization;
using System.Text;

namespace AccountingApp.Core.Services;

public static class TransactionAmountInputSanitizer
{
    public static string Sanitize(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(raw.Length);
        var hasDot = false;
        foreach (var ch in raw)
        {
            if (char.IsDigit(ch))
            {
                builder.Append(ch);
                continue;
            }

            if (ch == '.' && !hasDot)
            {
                builder.Append(ch);
                hasDot = true;
            }
        }

        return builder.ToString();
    }

    public static bool TryParsePositiveDecimal(string? input, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalized = Sanitize(input);
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            return false;
        }

        if (parsed <= 0m)
        {
            return false;
        }

        value = parsed;
        return true;
    }
}
