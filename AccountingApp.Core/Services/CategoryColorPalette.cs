namespace AccountingApp.Core.Services;

public static class CategoryColorPalette
{
    private const double GoldenAngle = 137.508;
    private const int MaxCandidateAttempts = 32;
    private const double MinChannelDistance = 42;

    public static string GetHexColor(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var hue = (index * GoldenAngle) % 360d;
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

    public static IReadOnlyDictionary<string, string> BuildDistinctHexColors(IEnumerable<string> keys)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var usedColors = new List<(byte Red, byte Green, byte Blue)>();

        foreach (var key in keys)
        {
            if (result.ContainsKey(key))
            {
                continue;
            }

            var normalized = NormalizeKey(key);
            var hash = ComputeStableHash(normalized);
            var baseHue = hash % 360d;
            var baseSaturation = 0.62 + ((hash >> 9) % 18) / 100d;
            var baseLightness = 0.46 + ((hash >> 17) % 16) / 100d;

            (byte Red, byte Green, byte Blue) bestCandidate = default;
            var bestDistance = double.MinValue;
            var chosen = false;

            for (var attempt = 0; attempt < MaxCandidateAttempts; attempt++)
            {
                var hue = (baseHue + attempt * GoldenAngle) % 360d;
                var saturationOffset = ((attempt % 5) - 2) * 0.04;
                var lightnessOffset = (((attempt / 5) % 5) - 2) * 0.04;
                var saturation = Math.Clamp(baseSaturation + saturationOffset, 0.55, 0.82);
                var lightness = Math.Clamp(baseLightness + lightnessOffset, 0.40, 0.68);
                var candidate = HslToRgb(hue, saturation, lightness);

                var nearestDistance = GetNearestDistance(candidate, usedColors);
                if (nearestDistance > bestDistance)
                {
                    bestDistance = nearestDistance;
                    bestCandidate = candidate;
                }

                if (nearestDistance >= MinChannelDistance)
                {
                    bestCandidate = candidate;
                    chosen = true;
                    break;
                }
            }

            if (!chosen && usedColors.Count == 0)
            {
                bestCandidate = HslToRgb(baseHue, baseSaturation, baseLightness);
            }

            usedColors.Add(bestCandidate);
            result[key] = $"#{bestCandidate.Red:X2}{bestCandidate.Green:X2}{bestCandidate.Blue:X2}";
        }

        return result;
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

    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return string.Empty;
        }

        return key.Trim().ToUpperInvariant();
    }

    private static double GetNearestDistance(
        (byte Red, byte Green, byte Blue) candidate,
        IReadOnlyList<(byte Red, byte Green, byte Blue)> usedColors)
    {
        if (usedColors.Count == 0)
        {
            return double.MaxValue;
        }

        var minDistance = double.MaxValue;
        foreach (var used in usedColors)
        {
            var distance = Math.Sqrt(
                Math.Pow(candidate.Red - used.Red, 2) +
                Math.Pow(candidate.Green - used.Green, 2) +
                Math.Pow(candidate.Blue - used.Blue, 2));
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
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
