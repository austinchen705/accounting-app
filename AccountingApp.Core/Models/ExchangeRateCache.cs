using SQLite;

namespace AccountingApp.Core.Models;

[Table("ExchangeRateCache")]
public class ExchangeRateCache
{
    [PrimaryKey, MaxLength(10)]
    public string BaseCurrency { get; set; } = "TWD";

    [NotNull]
    public string RatesJson { get; set; } = "{}";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
