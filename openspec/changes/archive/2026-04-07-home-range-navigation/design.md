## Context

首頁現在使用 `CurrentMonth` 載入月摘要與月內最近記錄。這個設計可支援月份切換，但無法擴充到 `day/week/year/all` 而不讓 ViewModel 變得零散。需求是把首頁改成統一的時間範圍模型，並保留 `All` 下的收入、支出、結餘摘要。

## Goals / Non-Goals

**Goals:**
- 在首頁提供 `Day / Week / Month / Year / All` 範圍切換。
- 所有範圍都顯示收入、支出、結餘摘要。
- 最近記錄依目前選取範圍更新，最多顯示 10 筆。
- `Week` 固定週一為一週起點。
- `All` 不允許前後切換。

**Non-Goals:**
- 不修改統計頁與分類報告頁的期間控件。
- 不新增資料庫 schema。
- 不做分組式最近記錄（例如依天/月分段顯示）。

## Decisions

1. **首頁改用 `range + anchorDate` 模型，而不是只存 `CurrentMonth`**
   - Why: `day/week/month/year` 都需要同一套前後切換與標題計算。
   - Alternative: 每種範圍各自維護不同欄位。缺點是 ViewModel 邏輯分散，維護成本高。

2. **新增通用日期區間查詢與摘要方法**
   - Why: 首頁若要支援多範圍，`TransactionService` 不能只靠 `GetByMonthAsync` / `GetMonthSummaryAsync`。
   - Alternative: 在 `HomeViewModel` 端自己篩全部交易。缺點是責任錯置，也不利後續重用。

3. **週區間固定以週一為起點**
   - Why: 已與分類報告的週邏輯一致，避免首頁和報表週區間不同。
   - Alternative: 使用系統地區設定。缺點是行為可能因裝置地區而變動，難以驗證。

4. **`All` 模式仍顯示收入、支出、結餘，但停用前後切換**
   - Why: 使用者仍需要全期間總覽，但 `All` 沒有前後期間可切。
   - Alternative: `All` 隱藏摘要。缺點是失去全期間概覽價值。

5. **最近記錄仍維持最多 10 筆，不做額外分組**
   - Why: 保持首頁精簡，先解決可切換範圍的核心需求。
   - Alternative: 在 `Year` / `All` 下做月份分組。缺點是首頁複雜度提升過快。

## Risks / Trade-offs

- [Risk] `All` 範圍可能查出大量資料，若直接取完整列表再裁前 10 筆，性能會變差
  - Mitigation: service 層補區間查詢與排序，盡量在資料來源就限制結果。

- [Risk] 範圍切換邏輯與標題格式容易出現邊界錯誤
  - Mitigation: 補 focused tests，覆蓋 day/week/month/year/all 的期間標題與前後切換。

- [Trade-off] 首頁控制會比目前多一層範圍按鈕，但這是支援多尺度總覽的必要成本。

## Proposed UX Rules

- `Day`: 標題顯示 `yyyy/MM/dd`
- `Week`: 標題顯示 `yyyy/MM/dd - MM/dd`
- `Month`: 標題顯示 `yyyy/MM`
- `Year`: 標題顯示 `yyyy`
- `All`: 標題顯示 `全部期間`

- `All`：隱藏或停用上一期 / 下一期按鈕
- 空狀態文案：統一改為 `此期間尚無收支資料`
- 最近記錄標題：由 `最近記錄` 改為 `期間內記錄`
