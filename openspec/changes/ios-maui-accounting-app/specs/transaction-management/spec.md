## ADDED Requirements

### Requirement: 新增收支記錄
系統 SHALL 允許使用者新增收入或支出記錄，包含金額、幣別、分類、日期、備註欄位。

#### Scenario: 成功新增支出
- **WHEN** 使用者填入金額、選擇分類與日期後送出
- **THEN** 系統將記錄存入 SQLite，並在首頁與記錄列表顯示

#### Scenario: 金額為空時送出
- **WHEN** 使用者未填金額即點擊儲存
- **THEN** 系統顯示驗證錯誤提示，不儲存記錄

### Requirement: 編輯收支記錄
系統 SHALL 允許使用者編輯已存在的記錄，修改任意欄位後儲存。

#### Scenario: 成功編輯記錄
- **WHEN** 使用者點擊記錄並修改金額後儲存
- **THEN** 系統更新 SQLite 中的記錄，列表即時反映變更

### Requirement: 刪除收支記錄
系統 SHALL 允許使用者刪除記錄，刪除前需二次確認。

#### Scenario: 確認刪除
- **WHEN** 使用者點擊刪除並確認
- **THEN** 系統從 SQLite 移除該記錄，列表移除該項目

#### Scenario: 取消刪除
- **WHEN** 使用者點擊刪除後選擇取消
- **THEN** 系統不刪除記錄

### Requirement: 篩選記錄列表
系統 SHALL 支援依月份、分類、幣別篩選記錄列表。

#### Scenario: 依月份篩選
- **WHEN** 使用者選擇特定月份
- **THEN** 系統僅顯示該月份的記錄