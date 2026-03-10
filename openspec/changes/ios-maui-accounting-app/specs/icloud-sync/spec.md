## ADDED Requirements

### Requirement: App 啟動時從 iCloud Drive 下載資料庫
系統 SHALL 在 App 啟動時檢查 iCloud Drive 是否有較新的 SQLite 備份，若有則下載覆蓋本地資料庫。

#### Scenario: iCloud 有較新備份
- **WHEN** App 啟動且 iCloud Drive 的備份時間戳較本地新
- **THEN** 系統下載 iCloud 備份並覆蓋本地 SQLite，重新載入資料

#### Scenario: iCloud 無備份或較舊
- **WHEN** App 啟動但 iCloud 無備份或備份較本地舊
- **THEN** 系統使用本地資料庫，不進行下載

#### Scenario: iCloud 不可用
- **WHEN** App 啟動時 iCloud 帳號未登入或網路不可用
- **THEN** 系統使用本地資料庫，顯示「iCloud 同步不可用」提示

### Requirement: 資料變更後自動上傳備份
系統 SHALL 在每次新增、編輯或刪除記錄後，自動將 SQLite 資料庫上傳至 iCloud Drive。

#### Scenario: 成功上傳
- **WHEN** 使用者儲存一筆新記錄且網路可用
- **THEN** 系統在背景將 SQLite 檔案上傳至 iCloud Drive，不阻塞 UI

#### Scenario: 上傳失敗（無網路）
- **WHEN** 資料變更但無網路連線
- **THEN** 系統標記待同步狀態，App 下次有網路時自動補傳

### Requirement: 手動觸發備份與還原
系統 SHALL 在設定頁面提供手動備份與還原按鈕。

#### Scenario: 手動備份
- **WHEN** 使用者點擊「立即備份至 iCloud」
- **THEN** 系統立即上傳最新 SQLite 至 iCloud Drive，並顯示成功訊息

#### Scenario: 手動還原
- **WHEN** 使用者點擊「從 iCloud 還原」並確認
- **THEN** 系統下載 iCloud 最新備份覆蓋本地資料，重新載入 App