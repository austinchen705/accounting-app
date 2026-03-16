using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class CategoryColorPaletteTests
{
    [Fact]
    public void GetHexColor_returns_unique_colors_for_first_24_indexes()
    {
        var colors = Enumerable.Range(0, 24)
            .Select(CategoryColorPalette.GetHexColor)
            .ToList();

        Assert.Equal(24, colors.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void GetHexColor_returns_hex_rgb_value()
    {
        var color = CategoryColorPalette.GetHexColor(0);

        Assert.Matches("^#[0-9A-F]{6}$", color);
    }

    [Fact]
    public void GetHexColorForKey_returns_same_color_for_same_key()
    {
        var first = CategoryColorPalette.GetHexColorForKey("居家");
        var second = CategoryColorPalette.GetHexColorForKey("居家");

        Assert.Equal(first, second);
    }

    [Fact]
    public void GetHexColorForKey_returns_hex_rgb_value()
    {
        var color = CategoryColorPalette.GetHexColorForKey("娛樂");

        Assert.Matches("^#[0-9A-F]{6}$", color);
    }

    [Fact]
    public void BuildDistinctHexColors_returns_unique_colors_for_visible_categories()
    {
        var categories = new[] { "餐飲", "交通", "娛樂", "購物", "醫療", "保險", "旅遊", "居家" };

        var map = CategoryColorPalette.BuildDistinctHexColors(categories);

        Assert.Equal(categories.Length, map.Count);
        Assert.Equal(categories.Length, map.Values.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void BuildDistinctHexColors_avoids_near_match_for_known_close_pair()
    {
        var map = CategoryColorPalette.BuildDistinctHexColors(new[] { "交通", "保險" });
        var traffic = ParseHexRgb(map["交通"]);
        var insurance = ParseHexRgb(map["保險"]);

        Assert.True(GetDistance(traffic, insurance) >= 42);
    }

    private static (int Red, int Green, int Blue) ParseHexRgb(string color)
    {
        return (
            Convert.ToInt32(color.Substring(1, 2), 16),
            Convert.ToInt32(color.Substring(3, 2), 16),
            Convert.ToInt32(color.Substring(5, 2), 16));
    }

    private static double GetDistance(
        (int Red, int Green, int Blue) left,
        (int Red, int Green, int Blue) right)
    {
        return Math.Sqrt(
            Math.Pow(left.Red - right.Red, 2) +
            Math.Pow(left.Green - right.Green, 2) +
            Math.Pow(left.Blue - right.Blue, 2));
    }
}
