## 1. Schema and storage foundation

- [x] 1.1 Add transaction attachment path metadata to the transaction model and database migration flow.
- [x] 1.2 Introduce a transaction image storage service for managed file import, compression, replacement, and deletion.
- [x] 1.3 Register the new image service in app dependency injection and add focused storage/service tests.

## 2. Transaction form state and save flow

- [x] 2.1 Extend `TransactionFormViewModel` to load, stage, replace, and clear a single attachment image.
- [x] 2.2 Update transaction save and delete paths so attachment file lifecycle stays consistent with record persistence.
- [x] 2.3 Add or update view-model tests covering create, edit, replace, remove, and failed-save attachment behavior.

## 3. Transaction form UI and image viewing

- [x] 3.1 Add `附件照片` controls to the transaction form for camera pick, library pick, view, replace, and remove actions.
- [x] 3.2 Add an image-viewing surface for full-size attachment preview.
- [x] 3.3 Add or update layout/page contract tests for attachment controls and viewing hooks.

## 4. Verification

- [x] 4.1 Run focused transaction-form, transaction-service, and storage tests.
- [x] 4.2 Run the full `AccountingApp.Tests` suite.
- [x] 4.3 Run an iOS simulator build to verify the attachment flow compiles on device-targeted app code.
