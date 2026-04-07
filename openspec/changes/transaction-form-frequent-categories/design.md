## Context

目前 `TransactionFormPage` 的分類選擇只有單一 `Picker`，建立與編輯交易時都必須開下拉清單尋找分類。這在手機上對高頻操作很慢，尤其是支出記錄常集中在少數幾個分類。使用者希望同時改善兩件事：讓常用分類更快被選到，並保留完整分類清單作為備援。

## Goals / Non-Goals

**Goals:**
- 在交易新增與編輯頁顯示目前交易類型對應的前 6 個常用分類。
- 常用分類依歷史使用次數由高到低排序，同次數時順序穩定。
- 讓使用者可直接點選常用分類 chip，不必每次打開 picker。
- 保留完整分類 picker，確保冷門分類仍可選取。
- 編輯既有交易時保留原本分類，即使該分類不在前 6。

**Non-Goals:**
- 不為分類建立 icon 資料表或視覺語意系統。
- 不新增手動排序或固定排序管理功能。
- 不改變分類管理頁的資料模型或編輯方式。
- 不在這次變更中調整其他表單欄位順序。

## Decisions

1. **以文字 chips 呈現常用分類，而非全面改成 icon grid**
   - 決定在分類 picker 上方加入一排可換行的常用分類 chips。
   - Why: 這能直接解決手機操作慢的問題，又不需要先建立完整的 icon 對照與維護規則。
   - Alternative: 全改成 icon grid。缺點是分類一多會造成畫面過長，且目前專案沒有穩定的 icon 對應資料。

2. **使用現有交易資料即時計算分類使用次數**
   - 決定以 `TransactionService.GetAllAsync()` 的交易資料，依目前 `Type` 統計每個分類被使用的次數，再與 `CategoryService.GetByTypeAsync(Type)` 的結果合併排序。
   - Why: 這不需要新的資料表或 migration，能利用現有資料快速提供穩定排序。
   - Alternative: 在分類資料表上新增 usage count 欄位。缺點是需要額外同步機制，且容易和實際交易資料失真。

3. **常用分類與完整 picker 共用同一個 SelectedCategory**
   - 決定 chips 點選只更新 `SelectedCategory`，picker 也綁同一個屬性。
   - Why: 可避免 UI 有兩套選取狀態，並讓新增與編輯流程共用既有儲存邏輯。
   - Alternative: 為 chips 建一個獨立的 selected state。缺點是容易與 picker 狀態不同步。

4. **編輯模式不強迫原分類出現在常用區**
   - 決定若編輯中的分類不在前 6，仍只在完整 picker 顯示為已選中，不插入常用 chips。
   - Why: 常用區應維持「依頻率排序」的規則，不應因單筆編輯而改變排序準則。
   - Alternative: 將目前選中分類硬插進常用區。缺點是會造成常用區排序規則不一致。

## Risks / Trade-offs

- [Risk] 每次載入表單都讀取全部交易後再統計，可能有額外成本
  - Mitigation: 交易量目前為個人記帳規模，先採用簡單實作；若未來資料量成長，再抽成專門查詢或快取。

- [Risk] 常用分類排序可能因交易歷史更新而改變，造成使用者感知到位置移動
  - Mitigation: 限制只顯示前 6，並在同次數時用名稱排序，降低順序抖動。

- [Trade-off] 保留 picker 代表分類區塊高度會增加
  - Mitigation: 常用區優先解決高頻操作；冷門分類仍由 picker 補足，整體效率優於單一下拉選單。
