## Why

分類報告頁目前只能看到分類層級的彙總資訊，當使用者想追查某個分類金額為何偏高時，必須再回到其他頁面重新篩選交易。這使分類報告停留在總覽，無法自然延伸到「看這個分類底下有哪些記帳記錄」的分析流程。

## What Changes

- 讓分類報告頁中的分類排行列支援點擊 drill-down。
- 新增一個分類交易明細頁，用來顯示所選分類在目前報告範圍下的記帳記錄。
- 明細頁的記錄依日期分組，日期由近到舊排序，組內交易也依日期由近到舊排序。
- drill-down 會保留原本分類報告頁的 range 與 period 上下文，返回時回到原本的報告狀態。

## Capabilities

### New Capabilities

### Modified Capabilities
- `expense-category-report`: 分類報告從純彙總檢視，擴充為可查看單一分類的交易明細 drill-down。

## Impact

- Affected code:
  - `AccountingApp/Views/CategoryReportPage.xaml`
  - `AccountingApp/ViewModels/CategoryReportViewModel.cs`
  - `AccountingApp/Views/` new category detail page
  - `AccountingApp/ViewModels/` new category detail view model
  - `AccountingApp/Services/StatisticsService.cs`
  - `AccountingApp/AppShell.xaml` or route registration if needed
  - `AccountingApp.Tests/*CategoryReport*`
- API/DB:
  - 無 schema 變更。
  - 僅新增分類報告明細查詢與頁面導航。
- UX:
  - 使用者可從分類報告直接點進單一分類的交易明細。
  - 明細會延續目前報告的篩選範圍，而不是跳到全域交易列表。
