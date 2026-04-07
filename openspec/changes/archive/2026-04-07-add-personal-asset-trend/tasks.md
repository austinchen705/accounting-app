## 1. Data Model and Persistence

- [x] 1.1 Add `AssetSnapshot` model with fields: Date, Stock, Cash, FirstTrade, Property, and computed total helper
- [x] 1.2 Add database migration/table creation for `AssetSnapshots`
- [x] 1.3 Implement `AssetSnapshotService` CRUD methods (create/list/update/delete)
- [x] 1.4 Add service-level validation for required fields and non-negative numeric values

## 2. Snapshot Management UI

- [x] 2.1 Create personal asset trend page and register navigation route/tab entry
- [x] 2.2 Implement viewmodel state and commands for loading, adding, editing, and deleting snapshots
- [x] 2.3 Build snapshot input form for Date, Stock, Cash, FirstTrade, and Property
- [x] 2.4 Build snapshot history list ordered by date with edit/delete actions

## 3. Trend Chart Visualization

- [x] 3.1 Implement chart data transformation from snapshots to stacked series by date
- [x] 3.2 Render stacked chart for Stock, Cash, FirstTrade, and Property
- [x] 3.3 Add total-asset visibility indicator on the same trend chart
- [x] 3.4 Add empty-state UI when there are no snapshots

## 4. CSV Import Workflow

- [x] 4.1 Define CSV schema mapping for Date, Stock, Cash, FirstTrade, and Property columns
- [x] 4.2 Implement CSV import parser with row-level validation and skip-invalid behavior
- [x] 4.3 Implement import summary feedback (imported count, skipped count, error details)
- [x] 4.4 Wire CSV import action into the personal asset trend page

## 5. Verification and Quality

- [x] 5.1 Add unit tests for service validation and CRUD behavior
- [x] 5.2 Add tests for chart transformation output and total calculations
- [x] 5.3 Add tests for CSV import valid/invalid row handling and summary output
- [x] 5.4 Run full test suite and manual QA for create/edit/delete/import/chart flows
