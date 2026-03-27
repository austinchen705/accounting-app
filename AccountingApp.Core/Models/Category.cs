using SQLite;
using AccountingApp.Core.Abstractions;

namespace AccountingApp.Core.Models;

[Table("Categories")]
public class Category : IFrequentCategorySourceCategory
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Icon { get; set; } = "category_other.png";

    [NotNull, MaxLength(10)]
    public string Type { get; set; } = "expense"; // "income" or "expense"
}
