## Why

個人記帳缺乏輕量、離線優先且能備份至 iCloud 的 iOS 工具。此 App 讓使用者快速記錄收支、設定預算、查看統計圖表，並支援多幣別與資料匯出，滿足個人日常財務管理需求。

## What Changes

- 全新 .NET MAUI iOS App，從零開始建立
- 本地 SQLite 資料庫儲存所有財務記錄
- iCloud Drive 備份與還原機制
- 多幣別支援（透過匯率 API 換算主幣別）
- 本地推播通知（預算超過 80% 時觸發）
- CSV / Excel 匯出功能

## Capabilities

### New Capabilities

- `transaction-management`: 收支記錄的新增、編輯、刪除，包含金額、幣別、分類、日期、備註
- `category-management`: 分類管理（餐飲、交通、娛樂等），支援圖示與收入／支出類型
- `budget-tracking`: 各分類月度預算設定，進度條顯示，超過 80% 觸發本地推播通知
- `statistics-charts`: 月度圓餅圖（分類佔比）與長條圖（收支趨勢），支援月份切換
- `multi-currency`: 多幣別記錄與換算，每日快取匯率（ExchangeRate-API）
- `data-export`: 匯出 CSV / Excel，透過 iOS Share Sheet 分享
- `icloud-sync`: App 啟動時從 iCloud Drive 同步 SQLite 資料庫，資料變更後自動上傳

### Modified Capabilities

（無現有 spec，全為新功能）

## Impact

- 新增 .NET MAUI iOS 專案（C#）
- 套件依賴：`sqlite-net-pcl`、`Microcharts` 或 `LiveCharts2`、`CsvHelper`、`ClosedXML`
- 外部 API：ExchangeRate-API（免費方案，每日快取）
- iOS 權限：iCloud Drive 存取、本地推播通知