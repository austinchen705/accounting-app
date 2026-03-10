## ADDED Requirements

### Requirement: 記錄原始幣別
系統 SHALL 儲存每筆記錄的原始幣別代碼（如 USD、JPY、TWD）與原始金額。

#### Scenario: 新增外幣記錄
- **WHEN** 使用者選擇 JPY 並輸入 1000 元
- **THEN** 系統儲存幣別 JPY、金額 1000，不自動換算

### Requirement: 主幣別設定
系統 SHALL 允許使用者在設定頁面指定主幣別，所有統計與預算以主幣別計算。

#### Scenario: 變更主幣別
- **WHEN** 使用者將主幣別從 TWD 改為 USD
- **THEN** 系統重新換算所有統計數據為 USD

### Requirement: 每日匯率快取
系統 SHALL 每日從 ExchangeRate-API 取得匯率並快取至本地，統計換算時使用快取匯率。

#### Scenario: 成功取得匯率
- **WHEN** 快取過期（超過 24 小時）且網路可用
- **THEN** 系統從 API 取得最新匯率並更新本地快取

#### Scenario: 網路不可用
- **WHEN** 快取過期但無網路連線
- **THEN** 系統使用最後一次快取的匯率，並顯示「匯率資料可能非最新」提示