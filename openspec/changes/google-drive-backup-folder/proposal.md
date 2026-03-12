## Why

目前備份依賴 iCloud，對使用者帳號與裝置生態綁定較高。改用 Google Drive 可跨裝置管理備份，並讓使用者自行指定備份資料夾，提升可控性與可攜性。

## What Changes

- 新增 Google Drive 備份/還原能力（固定檔名策略）。
- 新增「首次選擇 Google Drive 資料夾」流程，儲存 `folderId` 供後續使用。
- 備份時僅在指定資料夾內查找固定檔名（例如 `accounting_backup.db`），有則更新、無則建立。
- 還原時僅從指定資料夾讀取固定檔名，找不到則提示使用者。
- 設定頁調整雲端備份入口：將現有 iCloud 備份/還原改為 Google Drive 備份/還原。
- 啟動時取消 iCloud 自動同步，改為 Google Drive 可選的手動備份/還原流程。

## Capabilities

### New Capabilities
- `google-drive-backup`: 使用 Google Drive API 做固定檔名備份/還原，並支援使用者自選資料夾。

### Modified Capabilities
（無）

## Impact

- 主要影響檔案：`Services/ICloudService.cs`（替換或重命名）、`ViewModels/SettingsViewModel.cs`、`Views/SettingsPage.xaml`、`App.xaml.cs`。
- 新增 Google OAuth 與 Drive API 相依套件與設定（client id、scope、token 儲存）。
- 新增本機設定儲存欄位（Google folder id / folder name）。
- 測試需涵蓋：資料夾尚未設定、固定檔案不存在、token 過期、上傳更新成功、下載還原成功。
