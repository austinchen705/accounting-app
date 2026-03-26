namespace AccountingApp.Core.Services;

public static class StatisticsAxisScaleHelper
{
    public const double DefaultStep = 50_000;

    public static double CalculateStep(IEnumerable<double> values)
    {
        var maxValue = values
            .Select(Math.Abs)
            .DefaultIfEmpty(0)
            .Max();

        if (maxValue <= 0)
        {
            return 1_000;
        }

        var rawStep = maxValue / 6d;
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));

        foreach (var multiplier in new[] { 1d, 2d, 5d, 10d })
        {
            var candidate = multiplier * magnitude;
            if (rawStep <= candidate)
            {
                return candidate;
            }
        }

        return 10d * magnitude;
    }
}
