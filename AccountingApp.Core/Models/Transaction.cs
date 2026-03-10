using SQLite;

namespace AccountingApp.Core.Models;

[Table("Transactions")]
public class Transaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public decimal Amount { get; set; }

    [NotNull, MaxLength(10)]
    public string Currency { get; set; } = "TWD";

    public int CategoryId { get; set; }

    [NotNull]
    public DateTime Date { get; set; } = DateTime.Today;

    public string Note { get; set; } = string.Empty;

    [NotNull, MaxLength(10)]
    public string Type { get; set; } = "expense"; // "income" or "expense"
}
