using SQLite;

namespace AccountingApp.Models;

[Table("Budgets")]
public class Budget
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int CategoryId { get; set; }

    [NotNull]
    public decimal Amount { get; set; }

    /// <summary>Format: yyyy-MM (e.g. "2026-03")</summary>
    [NotNull, MaxLength(7)]
    public string Month { get; set; } = DateTime.Today.ToString("yyyy-MM");
}
