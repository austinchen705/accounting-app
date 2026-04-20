using AccountingApp.Core.Services;

namespace AccountingApp.Tests;

public class AppProvisionInfoServiceTests
{
    [Fact]
    public void FormatExpirationDateText_converts_to_gmt_plus_8_with_full_time()
    {
        var expirationDate = new DateTimeOffset(2026, 5, 1, 15, 59, 59, TimeSpan.Zero);

        var formatted = AppProvisionInfoService.FormatExpirationDateText(expirationDate);

        Assert.Equal("2026/05/01 23:59:59 GMT+08:00", formatted);
    }

    [Fact]
    public void TryGetExpirationDate_reads_provision_expiration_and_preserves_offset()
    {
        const string provisionContent = """
            ignored-prefix
            <plist version="1.0">
              <dict>
                <key>Name</key>
                <string>iOS Team Provisioning Profile: dindin.accounting</string>
                <key>ExpirationDate</key>
                <date>2026-05-01T15:59:59Z</date>
              </dict>
            </plist>
            ignored-suffix
            """;

        var expirationDate = AppProvisionInfoService.TryGetExpirationDate(provisionContent);

        Assert.Equal(new DateTimeOffset(2026, 5, 1, 15, 59, 59, TimeSpan.Zero), expirationDate);
    }

    [Fact]
    public void TryGetExpirationDate_returns_null_when_expiration_is_missing()
    {
        const string provisionContent = """
            <plist version="1.0">
              <dict>
                <key>Name</key>
                <string>missing-expiration</string>
              </dict>
            </plist>
            """;

        var expirationDate = AppProvisionInfoService.TryGetExpirationDate(provisionContent);

        Assert.Null(expirationDate);
    }
}
