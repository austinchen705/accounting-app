using AccountingApp.Core.Abstractions;

namespace AccountingApp.Models;

public class Category : IFrequentCategorySourceCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "category_other.png";
    public string Type { get; set; } = "expense";
}

public class Transaction : IFrequentCategorySourceTransaction
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TWD";
    public int CategoryId { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Note { get; set; } = string.Empty;
    public string? ImageRelativePath { get; set; }
    public string Type { get; set; } = "expense";
}
