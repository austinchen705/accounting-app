## ADDED Requirements

### Requirement: 匯出 CSV
系統 SHALL 將所有記錄匯出為 CSV 檔案，欄位包含日期、類型、分類、原始幣別、原始金額、主幣別金額、備註。

#### Scenario: 成功匯出 CSV
- **WHEN** 使用者在設定頁點擊「匯出 CSV」
- **THEN** 系統產生 CSV 檔並開啟 iOS Share Sheet，讓使用者選擇儲存或分享方式

#### Scenario: 無記錄時匯出
- **WHEN** 資料庫中無任何記錄
- **THEN** 系統顯示提示「尚無資料可匯出」，不產生檔案

### Requirement: 匯出 Excel
系統 SHALL 將所有記錄匯出為 .xlsx 檔案，格式與 CSV 相同，並包含標題列樣式。

#### Scenario: 成功匯出 Excel
- **WHEN** 使用者在設定頁點擊「匯出 Excel」
- **THEN** 系統使用 ClosedXML 產生 .xlsx 並開啟 iOS Share Sheet