## Context

目前 `CategoryReportPage` 只呈現分類彙總資料：甜甜圈圖與分類排行列表。每個分類列包含分類名稱、筆數、金額與占比，但沒有任何 drill-down 互動，導致使用者在看到異常高的分類金額時，無法直接追查是哪幾筆交易構成該分類總額。

需求已確認採用「新頁 drill-down」方案，而不是同頁展開。原因是分類報告頁本身已包含篩選器、圖表與列表；若再加進日期分組的交易明細，手機畫面會過度擁擠。新頁能讓分類報告維持總覽角色，同時把交易明細聚焦為獨立的分析步驟。

## Goals / Non-Goals

**Goals:**
- 讓使用者點擊分類報告列表中的任一分類後，導向單一分類的交易明細頁。
- 明細頁只顯示目前報告範圍與期間下、屬於該分類的 `expense` 交易。
- 明細資料依日期分組，日期由近到舊排序，組內交易也由近到舊排序。
- 返回分類報告頁時保留原本的 range 與 period 狀態。

**Non-Goals:**
- 不在這次變更中提供同頁展開模式。
- 不在明細頁加入交易編輯、刪除或跨頁重新篩選。
- 不讓 drill-down 改寫分類報告頁本身的排行規則或圖表呈現。
- 不新增新的資料表或永久儲存 drill-down 狀態。

## Decisions

1. **使用新頁 drill-down，而不是同頁展開**
   - Why: 能保持分類報告總覽簡潔，並給交易明細足夠空間做日期分組清單。
   - Alternative: 在同頁直接展開交易列表。缺點是手機畫面太長，圖表與列表的可讀性會明顯下降。

2. **以 Shell route 或導航參數傳遞 drill-down 上下文**
   - 決定將 category identifier、range、anchor date 傳給新頁，而不是依賴全域暫存狀態。
   - Why: 這能確保新頁可根據當前報告條件獨立重建資料，也更容易測試。
   - Alternative: 共用 ViewModel 狀態。缺點是頁面耦合高，返回與重入行為較難推理。

3. **在 `StatisticsService` 新增分類交易明細查詢**
   - 決定新增專用 API，回傳該分類在指定報告範圍與期間內的交易明細，再由 detail view model 做日期分組。
   - Why: 篩選規則應與分類報告本身保持一致，放在 service 層最容易重用並維持單一邏輯來源。
   - Alternative: 在 detail view model 直接呼叫 transaction service 並自行拼條件。缺點是與現有分類報告統計條件容易漂移。

4. **日期分組與排序由 detail view model 組裝**
   - 決定 service 回傳明細交易平面資料，view model 再依日期做 group。
   - Why: UI 呈現所需的分組形狀屬於 view model 責任，也較利於 MAUI `CollectionView` 或 `BindableLayout` 綁定。
   - Alternative: service 直接回傳 grouped view model shape。缺點是服務層綁到 UI 呈現模型，降低重用性。

## Risks / Trade-offs

- [Risk] drill-down 的篩選邏輯若與分類報告主頁不一致，使用者會看到總額對不起來
  - Mitigation: 讓 detail 查詢使用與 category report 相同的 range / anchor date 規則，並維持只納入 `expense` 交易與 base currency 轉換。

- [Risk] MAUI 日期分組清單若直接使用複雜巢狀模板，維護成本會升高
  - Mitigation: 採用簡單明確的 section model，例如 `DateGroup` + `TransactionItem`，避免過度動態化模板。

- [Trade-off] 新頁 drill-down 多一次導航
  - Mitigation: 返回後保留報告頁狀態，讓這次導航成為可預期的 drill-down，而不是跳來跳去。
