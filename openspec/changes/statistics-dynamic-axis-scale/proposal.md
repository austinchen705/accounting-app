## Why

統計頁目前將兩張趨勢圖的 Y 軸刻度固定為 `50,000`。當資料都落在較小區間，例如幾千到幾萬時，圖表容易只剩接近底部的折線，甚至讓使用者感覺 Y 軸單位沒有正常顯示，降低趨勢比較的可讀性。

## What Changes

- 讓統計頁的收入/支出趨勢圖與分類趨勢圖改用動態 Y 軸刻度。
- Y 軸刻度會根據當前資料最大值自動選擇合適的 step，而不是固定 `50,000`。
- 小範圍資料時採用較細的「好看刻度」，例如 `1k`、`2k`、`5k`、`10k`。
- 較大範圍資料則維持目前類似的粗刻度，例如 `50k` 或更高。
- 兩張統計趨勢圖共用同一套刻度規則，避免頁面內部不一致。

## Capabilities

### New Capabilities

### Modified Capabilities
- `statistics-category-trend-filtering`: 統計頁兩張趨勢圖的 Y 軸刻度從固定步進改為依資料範圍動態調整。

## Impact

- Affected code:
  - `AccountingApp/ViewModels/StatisticsViewModel.cs`
  - `AccountingApp.Tests/StatisticsAxisStepTests.cs`
  - `AccountingApp.Tests/*Statistics*`
  - potential shared chart-axis helper under `AccountingApp/Services/` or `AccountingApp/Core/`
- API/DB:
  - 無 schema 變更。
  - 僅調整圖表刻度計算邏輯。
- UX:
  - 小額資料的趨勢圖更容易閱讀。
  - 大額資料仍維持合理的 Y 軸間距，不會過度擁擠。
