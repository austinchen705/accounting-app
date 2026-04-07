## Why

Users need to keep receipt-oriented evidence together with a transaction record, but storing image binary directly in the transaction table would make the local database grow quickly and degrade query, backup, and sync behavior. The app needs a transaction attachment flow that keeps one image per record while storing the file efficiently in app-local storage.

## What Changes

- Add support for attaching one `附件照片` to a transaction from either the camera or photo library.
- Store the image as a compressed file in the app sandbox and persist only relative path metadata in the transaction record.
- Extend the transaction form so users can add, view, replace, and remove the attachment while creating or editing a transaction.
- Ensure transaction and file lifecycle stay in sync when a transaction is saved, updated, or deleted.

## Capabilities

### New Capabilities
- `transaction-image-attachment`: Lets a transaction own one app-managed attachment image with add/view/replace/remove flows and file-backed persistence.

### Modified Capabilities

None.

## Impact

- Affected code: `AccountingApp/Models/Transaction.cs`, `AccountingApp/Services/DatabaseService.cs`, `AccountingApp/Services/TransactionService.cs`, `AccountingApp/ViewModels/TransactionFormViewModel.cs`, `AccountingApp/Views/TransactionFormPage.xaml`, `AccountingApp/Views/TransactionFormPage.xaml.cs`, related resources and tests.
- New code: a dedicated transaction image storage/service layer and a lightweight image viewing surface.
- Platform/system touchpoints: MAUI media picking/capture APIs, app sandbox file storage, iOS/Android photo and camera permissions.
