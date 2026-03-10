using System.Globalization;
using CsvHelper;
using ClosedXML.Excel;
using AccountingApp.Models;

namespace AccountingApp.Services;

public class ExportService
{
    private readonly TransactionService _transactionService;
    private readonly CategoryService _categoryService;
    private readonly CurrencyService _currencyService;

    public ExportService(TransactionService transactionService, CategoryService categoryService, CurrencyService currencyService)
    {
        _transactionService = transactionService;
        _categoryService = categoryService;
        _currencyService = currencyService;
    }

    private async Task<List<ExportRow>> BuildRowsAsync()
    {
        var txns = await _transactionService.GetAllAsync();
        var categories = await _categoryService.GetAllAsync();
        var baseCurrency = Preferences.Get("base_currency", "TWD");
        var rows = new List<ExportRow>();

        foreach (var t in txns)
        {
            var rate = await _currencyService.GetRateAsync(t.Currency, baseCurrency);
            rows.Add(new ExportRow
            {
                Date = t.Date.ToString("yyyy-MM-dd"),
                Type = t.Type == "income" ? "收入" : "支出",
                Category = categories.FirstOrDefault(c => c.Id == t.CategoryId)?.Name ?? "",
                OriginalCurrency = t.Currency,
                OriginalAmount = t.Amount,
                BaseCurrencyAmount = Math.Round(t.Amount * (decimal)rate, 2),
                Note = t.Note
            });
        }

        return rows;
    }

    public async Task<string?> ExportCsvAsync()
    {
        var rows = await BuildRowsAsync();
        if (rows.Count == 0) return null;

        var path = Path.Combine(FileSystem.CacheDirectory, $"accounting_{DateTime.Today:yyyyMMdd}.csv");
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(rows);
        return path;
    }

    public async Task<string?> ExportExcelAsync()
    {
        var rows = await BuildRowsAsync();
        if (rows.Count == 0) return null;

        var path = Path.Combine(FileSystem.CacheDirectory, $"accounting_{DateTime.Today:yyyyMMdd}.xlsx");
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("記帳記錄");

        // Header row
        var headers = new[] { "日期", "類型", "分類", "原始幣別", "原始金額", "主幣別金額", "備註" };
        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
        }

        for (int r = 0; r < rows.Count; r++)
        {
            var row = rows[r];
            ws.Cell(r + 2, 1).Value = row.Date;
            ws.Cell(r + 2, 2).Value = row.Type;
            ws.Cell(r + 2, 3).Value = row.Category;
            ws.Cell(r + 2, 4).Value = row.OriginalCurrency;
            ws.Cell(r + 2, 5).Value = (double)row.OriginalAmount;
            ws.Cell(r + 2, 6).Value = (double)row.BaseCurrencyAmount;
            ws.Cell(r + 2, 7).Value = row.Note;
        }

        ws.Columns().AdjustToContents();
        wb.SaveAs(path);
        return path;
    }

    public static async Task ShareFileAsync(string filePath)
    {
        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "匯出記帳資料",
            File = new ShareFile(filePath)
        });
    }

    private class ExportRow
    {
        public string Date { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string OriginalCurrency { get; set; } = string.Empty;
        public decimal OriginalAmount { get; set; }
        public decimal BaseCurrencyAmount { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}
