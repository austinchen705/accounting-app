## Context

首頁目前固定顯示當月收支總覽，`HomeViewModel` 內已有 `CurrentMonth` 作為查詢鍵，但 UI 沒有切換入口，且最近記錄固定讀取全域最近 10 筆。需求是讓首頁可切換月份，且月總覽與最近記錄都跟著該月份更新。

## Goals / Non-Goals

**Goals:**
- 提供首頁月份前後切換控制。
- 以選取月份重新載入收入、支出、結餘。
- 最近記錄改為該月份資料（最多 10 筆）。
- 保持既有新增交易流程不變。

**Non-Goals:**
- 不新增跨年份快速跳轉器（例如月份下拉清單）。
- 不變更資料庫 schema 或 service method signature。
- 不修改其他 tab 的月份篩選行為。

## Decisions

1. **在 HomeViewModel 用命令控制月份，字串維持 `yyyy-MM`**  
   - Why: 既有 `GetMonthSummaryAsync` 與 `GetByMonthAsync` 已使用此格式，避免 API 變更。  
   - Alternative: 改為 `DateTime` 儲存月份。缺點是仍需轉字串查詢，改動面更大。

2. **最近記錄改為由 `GetByMonthAsync(CurrentMonth)` 取得後取前 10 筆**  
   - Why: 符合「整個首頁切到同月份」需求，邏輯一致。  
   - Alternative: 保持全域最近 10 筆。缺點是和月總覽不一致。

3. **首頁卡片加月份切換列（上一月 / 標題 / 下一月）**  
   - Why: 最少互動成本，與現有卡片版面相容。  
   - Alternative: 使用獨立 DatePicker。缺點是交互較重、視覺佔位更大。

## Risks / Trade-offs

- [Risk] 使用者切到沒有資料的月份會看到空畫面 → Mitigation: 空狀態文案改為「該月份尚無收支資料」。
- [Risk] 連續切月造成重複載入 → Mitigation: 先採用簡單即時載入；若有性能問題再加防抖。
- [Trade-off] 只提供前後月，不支援一次跳至任意月份，實作簡單但操作次數較多。
