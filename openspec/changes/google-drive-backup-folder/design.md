## Context

現行 App 以 iCloud Drive 作為備份來源，啟動流程會嘗試同步，使用者無法指定備份資料夾。新需求是改為 Google Drive，且採固定檔名策略，但資料夾需由使用者首次自行選擇並持久化設定。現有資料庫檔案為本機 `accounting.db`，設定頁已有手動備份/還原入口。

## Goals / Non-Goals

**Goals:**
- 提供 Google Drive OAuth 登入與授權。
- 首次讓使用者選擇 Google Drive 資料夾並保存 `folderId`。
- 以固定檔名 `accounting_backup.db` 在指定資料夾執行上傳（覆蓋）與還原（下載）。
- 在設定頁以 Google Drive 取代 iCloud 備份/還原入口。

**Non-Goals:**
- 不實作多備份版本管理。
- 不支援任意檔名搜尋或多檔案挑選。
- 不在啟動時自動從雲端覆蓋本機資料。

## Decisions

1. **固定檔名 + 使用者選資料夾**
- 決策：檔名固定為 `accounting_backup.db`，資料夾由使用者首次選擇。
- 替代：完全固定根目錄或每次都手選檔案。
- 理由：兼顧穩定自動化（固定檔名）與使用者控制（資料夾可選）。

2. **以 Google Drive API 清單/查詢操作檔案，不使用本地檔案 picker 模擬**
- 決策：使用 Drive API 在指定 `folderId` 內查詢同名檔案，存在就 `update`，不存在就 `create`。
- 替代：每次交給使用者選檔/另存。
- 理由：可做可靠備份流程與可測試行為。

3. **Token 與 folder 設定持久化**
- 決策：OAuth token 與 `folderId`/`folderName` 儲存在 Preferences（或安全儲存機制）。
- 替代：只存在記憶體。
- 理由：避免每次重登與重選資料夾。

4. **保留手動操作，不做啟動自動覆蓋**
- 決策：設定頁保留「備份到 Google Drive」「從 Google Drive 還原」手動按鈕。
- 替代：啟動時自動同步。
- 理由：降低誤覆蓋本機資料風險。

## Risks / Trade-offs

- **OAuth 失效/撤銷授權** → 需要重新授權與明確錯誤提示。  
- **folderId 無效（資料夾被刪）** → 導引使用者重新選資料夾。  
- **網路不穩導致上傳失敗** → 顯示失敗訊息，保留手動重試。  
- **Drive API 配額限制** → 維持手動操作頻率，必要時加入退避重試。

## Migration Plan

1. 新增 `GoogleDriveService` 與 OAuth/Drive API client。
2. 設定頁按鈕與文案從 iCloud 改為 Google Drive。
3. `App.OnStart` 移除 iCloud 啟動同步依賴（保留 DB 初始化）。
4. 保留舊 `ICloudService` 但不再由 UI 入口觸發（可在後續版本移除）。
5. 回滾策略：保留舊服務實作，若 Google Drive 異常可快速切回 iCloud 入口。

## Open Questions

- OAuth 授權流程採系統瀏覽器還是內嵌 WebView？
- Token 儲存是否要改用 `SecureStorage`（相較 Preferences 更安全）？
- 是否需要「切換備份資料夾」專用按鈕，或與「重新授權」合併？
