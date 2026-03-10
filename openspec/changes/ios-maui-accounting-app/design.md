## Context

全新個人記帳 iOS App，從零建立，無現有程式碼庫。使用者為單一個人，需要離線優先、能備份至 iCloud 的輕量記帳工具。開發者具備 C# / .NET 經驗，無 Swift 經驗。

## Goals / Non-Goals

**Goals:**
- 以 .NET MAUI 建立可部署至 iOS 的個人記帳 App
- 本地 SQLite 資料庫，離線優先
- iCloud Drive 備份還原（非即時同步）
- 多幣別記錄與每日匯率換算
- 月度統計圖表（圓餅圖、長條圖）
- 預算設定與本地推播通知
- CSV / Excel 匯出

**Non-Goals:**
- 多用戶 / 家庭共享
- Android 支援（雖然 MAUI 跨平台，但僅部署 iOS）
- 即時 iCloud 同步（僅 App 啟動 / 資料變更時上傳）
- 雲端後端 API
- 複式記帳 / 商業帳務

## Decisions

### 1. .NET MAUI 而非 Swift/SwiftUI
**選擇**：.NET MAUI（C#）
**理由**：開發者已熟悉 C# / .NET，可大幅縮短學習曲線，快速交付。
**替代方案**：Swift + SwiftUI — 原生 iOS 體驗更佳，但需從頭學習語言和框架。
**取捨**：接受略低於原生的 UI 流暢度，換取快速上手。

### 2. SQLite（sqlite-net-pcl）而非 CloudKit
**選擇**：本地 SQLite + iCloud Drive 備份
**理由**：sqlite-net-pcl 是 MAUI 生態最成熟的 ORM；CloudKit 在 MAUI 中需要原生 binding，整合複雜度高。
**替代方案**：CloudKit — 即時同步，但 MAUI 整合困難。
**取捨**：換機前需手動觸發備份，非即時同步。

### 3. MVVM 架構
**選擇**：Model-View-ViewModel（MVVM）
**理由**：MAUI 原生支援 MVVM 綁定（INotifyPropertyChanged、Command），社群範例豐富。
**替代方案**：Code-behind — 較簡單但難以測試和維護。

### 4. ExchangeRate-API（免費方案）
**選擇**：每日快取匯率至本地
**理由**：個人用途不需要即時匯率，免費方案每月 1500 次請求足夠。
**替代方案**：Open Exchange Rates、Fixer.io — 功能更豐富但需付費。

### 5. 圖表套件：Microcharts
**選擇**：Microcharts
**理由**：輕量、MAUI 相容、社群有維護中的 fork，支援圓餅圖與長條圖。
**替代方案**：LiveCharts2 — 功能更豐富但設定更複雜。

## Risks / Trade-offs

- **MAUI iOS 部署限制** → 需要 macOS 機器（或 Mac Build Host）才能打包上架 App Store；若無 Mac，可用 Mac-in-Cloud 服務
- **iCloud Drive 同步衝突** → 若多裝置同時寫入（未來擴充），可能覆蓋資料；目前單裝置使用，風險極低
- **ExchangeRate-API 不可用** → App 降級為顯示原始幣別金額，不換算；快取最後一次匯率繼續使用
- **Microcharts 社群維護** → 若套件停止維護，圖表模組需替換；可隔離為獨立 Service 以降低影響
- **sqlite-net-pcl 無 migration 工具** → Schema 變更需手動撰寫 migration 邏輯；透過版本欄位控管