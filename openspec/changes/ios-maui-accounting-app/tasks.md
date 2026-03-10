## 1. 專案初始化

- [x] 1.1 建立 .NET MAUI iOS 專案（AccountingApp）
- [x] 1.2 安裝 NuGet 套件：sqlite-net-pcl、Microcharts.Maui、CsvHelper、ClosedXML
- [x] 1.3 設定 MVVM 資料夾結構（Models / Views / ViewModels / Services / Resources）
- [x] 1.4 建立 AppShell 與底部 Tab Bar（首頁、記錄、統計、預算、設定）
- [x] 1.5 設定 iOS 權限：iCloud entitlement、本地推播通知

## 2. 資料模型與資料庫

- [x] 2.1 建立 SQLite Model：Transaction（id, amount, currency, category_id, date, note, type）
- [x] 2.2 建立 SQLite Model：Category（id, name, icon, type）
- [x] 2.3 建立 SQLite Model：Budget（id, category_id, amount, month）
- [x] 2.4 建立 SQLite Model：ExchangeRateCache（base_currency, rates_json, updated_at）
- [x] 2.5 建立 DatabaseService：初始化 SQLite、建立資料表、版本控管
- [x] 2.6 DatabaseService 首次啟動時插入預設分類

## 3. 分類管理（category-management）

- [x] 3.1 建立 CategoryService：CRUD 操作
- [x] 3.2 建立分類列表頁面（CategoryListPage + ViewModel）
- [x] 3.3 建立新增 / 編輯分類頁面，含名稱驗證（不重複）
- [x] 3.4 實作刪除分類邏輯（有關聯記錄時禁止刪除並顯示錯誤）

## 4. 記錄管理（transaction-management）

- [x] 4.1 建立 TransactionService：CRUD 操作、依條件查詢（月份、分類、幣別）
- [x] 4.2 建立首頁（HomePage + ViewModel）：本月總覽（收入 / 支出 / 結餘）、最近記錄列表
- [x] 4.3 建立快速新增按鈕（+），開啟記錄新增頁面
- [x] 4.4 建立新增 / 編輯記錄頁面（TransactionFormPage + ViewModel）
- [x] 4.5 實作表單驗證（金額必填、金額 > 0）
- [x] 4.6 建立記錄列表頁面（TransactionListPage + ViewModel），含月份 / 分類 / 幣別篩選
- [x] 4.7 實作刪除記錄（二次確認彈窗）

## 5. 多幣別（multi-currency）

- [x] 5.1 建立 CurrencyService：取得 ExchangeRate-API 匯率、讀寫本地快取
- [x] 5.2 實作快取邏輯（超過 24 小時或無快取時重新取得）
- [x] 5.3 實作離線降級（無網路時使用舊快取，顯示提示訊息）
- [x] 5.4 在設定頁新增「主幣別」選擇器，儲存至 Preferences
- [x] 5.5 TransactionService 統計時使用 CurrencyService 換算為主幣別

## 6. 預算追蹤（budget-tracking）

- [x] 6.1 建立 BudgetService：CRUD、計算當月各分類使用率
- [x] 6.2 建立預算頁面（BudgetPage + ViewModel）：進度條列表、超支紅色警示
- [x] 6.3 新增記錄後呼叫 BudgetService 檢查使用率
- [x] 6.4 實作本地推播通知（>80% 觸發，同分類同月份不重複通知）
- [x] 6.5 實作每月通知狀態重置邏輯

## 7. 統計圖表（statistics-charts）

- [x] 7.1 建立 StatisticsService：查詢月度分類加總、最近 6 個月收支加總
- [x] 7.2 建立統計頁面（StatisticsPage + ViewModel）
- [x] 7.3 整合 Microcharts 圓餅圖（當月分類支出佔比）
- [x] 7.4 整合 Microcharts 長條圖（最近 6 個月收支趨勢）
- [x] 7.5 實作月份切換（< / >），圖表即時更新
- [x] 7.6 無資料時顯示空狀態提示

## 8. 資料匯出（data-export）

- [x] 8.1 建立 ExportService：產生 CSV（CsvHelper）、產生 Excel（ClosedXML）
- [x] 8.2 在設定頁新增「匯出 CSV」按鈕，呼叫 ExportService 並開啟 iOS Share Sheet
- [x] 8.3 在設定頁新增「匯出 Excel」按鈕，同上
- [x] 8.4 無記錄時顯示提示「尚無資料可匯出」

## 9. iCloud 同步（icloud-sync）

- [x] 9.1 建立 iCloudService：上傳 / 下載 SQLite 至 iCloud Drive
- [x] 9.2 App 啟動時比對本地與 iCloud 時間戳，若 iCloud 較新則下載覆蓋
- [x] 9.3 每次資料變更（新增 / 編輯 / 刪除）後背景非同步上傳
- [x] 9.4 實作離線待同步標記，網路恢復時自動補傳
- [x] 9.5 在設定頁新增「立即備份」與「從 iCloud 還原」手動按鈕
- [x] 9.6 iCloud 不可用時顯示提示，不中斷 App 正常使用

## 10. 收尾

- [x] 10.1 UI 樣式統一（顏色、字體、間距）
- [x] 10.2 在 iOS 模擬器進行完整功能測試
- [x] 10.3 修正測試期間發現的問題