## Why

首頁目前只有月份切換，無法快速檢視單日、單週、整月、全年或全部期間的收入/支出/結餘。使用者若想從首頁直接看不同時間尺度的總覽，必須切換到其他頁面或自行推算，首頁作為總覽入口的價值不足。

## What Changes

- 首頁時間範圍從固定月份擴充為 `Day / Week / Month / Year / All`。
- 首頁摘要卡片依目前時間範圍顯示收入、支出、結餘；`All` 仍顯示完整摘要。
- 首頁期間標題依範圍切換，例如單日、週區間、月份、年份、全部期間。
- 首頁最近記錄改為顯示目前時間範圍內的最近交易（最多 10 筆）。
- `All` 模式停用前後切換，其他模式保留前後期間導航。

## Capabilities

### New Capabilities
- `home-range-navigation`: 首頁可切換日、週、月、年、全部期間，並同步更新摘要與最近記錄。

### Modified Capabilities
- `home-month-navigation`: 首頁原本僅支援月份切換，擴充為多種時間範圍。

## Impact

- Affected code:
  - `AccountingApp/ViewModels/HomeViewModel.cs`
  - `AccountingApp/Views/HomePage.xaml`
  - `AccountingApp/Services/TransactionService.cs`
  - `AccountingApp.Tests/*Home*Tests.cs`
- API/DB:
  - 無 schema 變更，但需補通用日期區間查詢/摘要方法。
- UX:
  - 首頁由月總覽升級為可切換日、週、月、年、全部期間的統一總覽。
