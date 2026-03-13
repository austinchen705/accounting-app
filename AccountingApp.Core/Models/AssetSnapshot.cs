using SQLite;

namespace AccountingApp.Core.Models;

public class AssetSnapshot
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public decimal Stock { get; set; }

    public decimal Cash { get; set; }

    public decimal FirstTrade { get; set; }

    public decimal Property { get; set; }

    public decimal Total => Stock + Cash + FirstTrade + Property;
}
