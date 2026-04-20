using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class AssetTrendChartAxisFormattingTests
{
    [Fact]
    public void Detail_step_is_denser_than_summary_step_for_the_same_range()
    {
        var values = new[] { 0d, 4_200_000d, 8_600_000d, 19_600_000d };

        var summaryStep = AssetTrendChartAxisHelper.CalculateSummaryStep(values);
        var detailStep = AssetTrendChartAxisHelper.CalculateDetailStep(values);

        Assert.True(detailStep < summaryStep);
    }

    [Fact]
    public void Summary_labels_use_compact_format_but_detail_labels_keep_full_values()
    {
        Assert.Equal("2.5M", AssetTrendChartAxisHelper.FormatSummaryValue(2_500_000d));
        Assert.Equal("2,500,000", AssetTrendChartAxisHelper.FormatDetailValue(2_500_000d));
    }
}
