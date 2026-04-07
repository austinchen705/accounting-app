## Why

首頁目前只能顯示當月份收支總覽，無法快速比較前後月份，使用者必須切換到其他頁面或自行推算，降低首頁作為總覽入口的效率。現在已有月彙總資料來源，補上月份切換可直接解決這個缺口。

## What Changes

- 在首頁總覽區塊新增月份切換控制（上一月 / 目前月份 / 下一月）。
- 首頁上方收支總覽改為依目前選取月份計算與顯示。
- 首頁「最近記錄」改為顯示選取月份的最近交易（最多 10 筆）。
- 首頁空狀態文案改為對應「選取月份」而非固定本月。

## Capabilities

### New Capabilities
- `home-month-navigation`: 首頁可切換月份並同步更新月彙總與該月最近記錄。

### Modified Capabilities
- None.

## Impact

- Affected code:
  - `AccountingApp/ViewModels/HomeViewModel.cs`
  - `AccountingApp/Views/HomePage.xaml`
  - `AccountingApp.Tests/*Home*Tests.cs`（新增或擴充）
- API/DB:
  - 無 schema 變更，沿用既有 `GetMonthSummaryAsync`、`GetByMonthAsync`。
- UX:
  - 首頁由固定本月改為可切換月份的月總覽模式。
