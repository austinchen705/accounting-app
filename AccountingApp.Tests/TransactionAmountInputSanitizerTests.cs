using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class TransactionAmountInputSanitizerTests
{
    [Theory]
    [InlineData("123", "123")]
    [InlineData("12.34", "12.34")]
    [InlineData("..12a3.4", ".1234")]
    [InlineData("1.2.3", "1.23")]
    [InlineData("abc", "")]
    public void Sanitize_keeps_digits_and_single_dot(string raw, string expected)
    {
        var actual = TransactionAmountInputSanitizer.Sanitize(raw);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("100", true, 100)]
    [InlineData("0.5", true, 0.5)]
    [InlineData(".5", true, 0.5)]
    [InlineData("0", false, 0)]
    [InlineData("", false, 0)]
    [InlineData("..", false, 0)]
    public void TryParsePositiveDecimal_parses_only_positive_values(string input, bool ok, decimal expected)
    {
        var result = TransactionAmountInputSanitizer.TryParsePositiveDecimal(input, out var value);
        Assert.Equal(ok, result);
        if (ok)
        {
            Assert.Equal(expected, value);
        }
    }
}
