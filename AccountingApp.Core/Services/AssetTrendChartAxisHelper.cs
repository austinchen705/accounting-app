namespace AccountingApp.Core.Services;

public static class AssetTrendChartAxisHelper
{
    private const double SummaryMinimumStep = 2_500_000d;
    private const double DetailMinimumStep = 500_000d;

    public static double CalculateSummaryStep(IEnumerable<double> values) =>
        CalculateStep(values, targetStepCount: 6, minimumStep: SummaryMinimumStep);

    public static double CalculateDetailStep(IEnumerable<double> values) =>
        CalculateStep(values, targetStepCount: 10, minimumStep: DetailMinimumStep);

    public static string FormatSummaryValue(double value)
    {
        if (value >= 1_000_000d)
        {
            return $"{value / 1_000_000d:0.#}M";
        }

        if (value >= 1_000d)
        {
            return $"{value / 1_000d:0.#}K";
        }

        return value.ToString("0");
    }

    public static string FormatDetailValue(double value) => value.ToString("#,0");

    private static double CalculateStep(IEnumerable<double> values, int targetStepCount, double minimumStep)
    {
        var maxValue = values
            .Select(Math.Abs)
            .DefaultIfEmpty(0)
            .Max();

        if (maxValue <= 0)
        {
            return minimumStep;
        }

        var rawStep = maxValue / targetStepCount;
        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));

        foreach (var multiplier in new[] { 1d, 2d, 5d, 10d })
        {
            var candidate = multiplier * magnitude;
            if (rawStep <= candidate)
            {
                return Math.Max(candidate, minimumStep);
            }
        }

        return Math.Max(10d * magnitude, minimumStep);
    }
}
