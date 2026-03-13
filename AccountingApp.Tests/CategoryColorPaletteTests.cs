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
}
