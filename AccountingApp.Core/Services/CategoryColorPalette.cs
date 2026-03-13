namespace AccountingApp.Core.Services;

public static class CategoryColorPalette
{
    public static string GetHexColor(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        const double goldenAngle = 137.508;
        var hue = (index * goldenAngle) % 360d;
        var saturation = 0.72 - ((index / 24) % 3) * 0.1;
        var lightness = 0.5 + ((index / 8) % 2) * 0.08;
        var (r, g, b) = HslToRgb(hue, saturation, lightness);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public static string GetHexColorForKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return GetHexColor(0);
        }

        var hash = ComputeStableHash(key.Trim().ToUpperInvariant());
        var hue = hash % 360d;
        var saturation = 0.62 + ((hash >> 9) % 18) / 100d;
        var lightness = 0.46 + ((hash >> 17) % 16) / 100d;
        var (r, g, b) = HslToRgb(hue, saturation, lightness);

        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static (byte Red, byte Green, byte Blue) HslToRgb(double hue, double saturation, double lightness)
    {
        var c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        var hPrime = hue / 60d;
        var x = c * (1 - Math.Abs(hPrime % 2 - 1));

        var (r1, g1, b1) = hPrime switch
        {
            >= 0 and < 1 => (c, x, 0d),
            >= 1 and < 2 => (x, c, 0d),
            >= 2 and < 3 => (0d, c, x),
            >= 3 and < 4 => (0d, x, c),
            >= 4 and < 5 => (x, 0d, c),
            _ => (c, 0d, x)
        };

        var m = lightness - c / 2;
        return (
            Red: ToByte(r1 + m),
            Green: ToByte(g1 + m),
            Blue: ToByte(b1 + m));
    }

    private static byte ToByte(double value)
    {
        var scaled = (int)Math.Round(value * 255);
        return (byte)Math.Clamp(scaled, 0, 255);
    }

    private static uint ComputeStableHash(string value)
    {
        const uint offset = 2166136261;
        const uint prime = 16777619;
        var hash = offset;

        foreach (var c in value)
        {
            hash ^= c;
            hash *= prime;
        }

        return hash;
    }
}
