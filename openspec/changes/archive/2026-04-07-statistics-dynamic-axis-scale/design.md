## Context

目前 `StatisticsViewModel` 會對兩張趨勢圖的 Y 軸都寫死：

- `MinLimit = 0`
- `MinStep = 50_000`
- `ForceStepToMin = true`

這在中大額資料下還算可讀，但當資料範圍只有幾千或幾萬時，折線容易被壓扁在底部，Y 軸刻度也失去辨識度。使用者已明確指出希望小範圍資料時使用更細的刻度，例如 1k、2k、3k、4k、5k，而大範圍資料維持現有粗刻度。

## Goals / Non-Goals

**Goals:**
- 讓統計頁上方的收入/支出趨勢圖與下方的分類趨勢圖都依資料範圍自動選擇 Y 軸步進。
- 保持 Y 軸從 0 開始。
- 讓小額資料時仍有 4 到 6 條可讀的 Y 軸刻度。
- 兩張圖共用同一套刻度邏輯，避免規則不一致。

**Non-Goals:**
- 不修改圖表 series 顏色、線條樣式或 X 軸規則。
- 不改動 Asset Trend 頁的 Y 軸邏輯。
- 不追求完全自由的 chart library auto-scale；仍保留可預測的「好看刻度」集合。

## Decisions

1. **以共享 helper 計算「nice step」**
   - 決定抽出一個小型 helper，輸入圖表資料值集合，輸出合適的 `MinStep`。
   - Why: 目前兩張圖都要動態縮放，抽 helper 能保證一致性並避免重複邏輯。
   - Alternative: 在兩個建軸區塊各自寫 if/else。缺點是容易漂移，也較難測試。

2. **採用「目標 5 格」的 nice number 規則**
   - 決定以資料最大值除以目標刻度數，然後向上取最近的 nice step，例如 `1k / 2k / 5k / 10k / 20k / 50k / 100k`。
   - Why: 這比單純用 `小於 50k => 1k` 更穩，能自然涵蓋 6k、18k、42k、260k 這些中間區間。
   - Alternative: 用單一門檻切換。缺點是只有兩段式規則，面對中間值時可讀性不穩。

3. **保留 `MinLimit = 0`，視需要推導 `MaxLimit`**
   - 初版至少動態調整 `MinStep`；若 chart library 在低值區間仍不理想，可同步設定向上取整的 `MaxLimit`。
   - Why: 部分 library 即使有適合的 `MinStep`，若上限沒有往上取整，視覺上仍可能不夠均勻。
   - Alternative: 只改 `Labeler`。缺點是只改顯示單位，不會改善刻度密度。

## Risks / Trade-offs

- [Risk] 若刻度切得太細，較大資料集會出現過多 grid lines
  - Mitigation: 使用固定的 nice-step 集合與目標 5 格策略，不讓步進無限制變小。

- [Risk] 目前 `StatisticsAxisStepTests` 綁死 `50_000`，改動後測試會全面失真
  - Mitigation: 將測試改為驗證 helper 規則與兩張圖都呼叫動態刻度邏輯，而不是硬編碼某個步進值。

- [Trade-off] 動態刻度會讓不同月份間的 Y 軸密度改變
  - Mitigation: 這是為了提升單月可讀性；若未來要做跨月份精準比較，可再討論固定 scale 模式。
