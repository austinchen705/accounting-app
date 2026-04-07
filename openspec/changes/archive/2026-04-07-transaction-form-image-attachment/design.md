## Context

Transactions are currently stored in a single SQLite table and edited through `TransactionFormViewModel` plus `TransactionFormPage`. The form already handles creation, editing, category focus flow, and inline calendar guidance, but it has no concept of attachments. The app is local-first, and the user explicitly wants one image per transaction, with receipt use as the primary case, while avoiding database bloat and not adding export requirements.

The current repository already uses MAUI platform services, SQLite-net models, and view-model-driven form logic. That makes this change cross-cutting: it needs a schema extension, a file storage abstraction, form state management, and platform picker/capture integration.

## Goals / Non-Goals

**Goals:**
- Allow a user to attach one image to a transaction from either camera capture or photo library selection.
- Store only file reference metadata in the database while keeping the actual image in app-local sandbox storage.
- Compress the saved image to a reasonable single-file size without introducing a separate thumbnail pipeline.
- Support add, replace, remove, and full-image viewing from the transaction form in both create and edit modes.
- Keep attachment file lifecycle consistent with transaction lifecycle so orphaned files are minimized.

**Non-Goals:**
- Multiple images per transaction.
- OCR, receipt parsing, or automatic amount/date/category extraction.
- Exporting or backing up attachment files outside the app sandbox.
- Shared photo-library references as the primary storage model.

## Decisions

### 1. Store one relative image path directly on `Transactions`

The transaction table will gain a nullable relative-path field for the attachment image, plus a small timestamp/metadata field if implementation needs change tracking. We will not introduce a separate attachment table yet because the confirmed product scope is one image per transaction, and a direct column keeps query and form loading simple.

Alternatives considered:
- Separate `transaction_attachments` table: more extensible, but unnecessary schema complexity for a one-image requirement.
- Binary/blob in SQLite: simpler writes, but unacceptable database growth and heavier query/backup costs.

### 2. Save the managed image file inside app sandbox storage

Selected images will be copied into an app-managed `receipts/...` folder under `FileSystem.AppDataDirectory`, and the database will keep only the relative path. The app will not rely on system photo-library file paths because those are less stable and depend on external asset lifecycle and permissions.

Alternatives considered:
- Keep the original Photos reference only: smaller app data footprint, but attachments can break if the user deletes the source image or changes access permissions.
- Save to external/shared storage: more visible to users, but less reliable and more platform-specific.

### 3. Compress to one managed display file, but do not create thumbnails

When importing from camera or library, the app will generate a single compressed image file sized for receipt readability and app storage efficiency. This avoids full-resolution photo bloat while keeping the implementation simpler than a two-file original-plus-thumbnail pipeline.

Alternatives considered:
- Preserve full original image: simplest implementation, but too much growth in app data.
- Save thumbnail + original: best scrolling performance, but extra storage and more lifecycle complexity than the user currently needs.

### 4. Encapsulate file operations in a dedicated service

A new transaction image service will own path generation, import/compression, replacement, deletion, and cleanup behavior. `TransactionFormViewModel` and `TransactionService` should not manipulate raw file paths directly beyond consuming service results. This keeps storage logic testable and prevents page code-behind from becoming a file-management layer.

Alternatives considered:
- Put file logic inside `TransactionFormViewModel`: faster to start, but tightly couples media operations, persistence, and UI flow.
- Put all file logic inside `TransactionService`: keeps fewer services, but mixes record CRUD with storage concerns.

### 5. Defer destructive file cleanup until transaction persistence succeeds

Replacing or removing an image should stage the new state in form state first. Old files are deleted only after the transaction save/update succeeds. Deleting a transaction should also delete its managed attachment file after the database delete succeeds. This preserves consistency between the database record and the sandbox file tree.

Alternatives considered:
- Delete old file immediately when user taps replace/remove: simpler state model, but risky if save later fails.

## Risks / Trade-offs

- **Large photos still increase app data size** → Compress on import and keep only one managed file per transaction.
- **Sandbox files can drift from DB rows if a save crashes mid-flow** → Centralize staged replace/remove logic in the image service and add cleanup tests for failure paths.
- **Platform picker/camera behavior differs across iOS and Android** → Keep media acquisition behind service abstractions and cover page/viewmodel contract behavior with focused tests.
- **Schema migration on existing installs can be brittle** → Add explicit database migration logic for new columns instead of assuming `CreateTableAsync` alone will cover all upgrade cases.

## Migration Plan

1. Extend the transaction schema with nullable image path metadata fields.
2. Add database initialization/migration logic that safely adds missing columns on existing installs.
3. Introduce the image storage service and wire it into DI.
4. Update transaction form state, UI, and save/delete flows.
5. Validate with focused tests, then full transaction-related test runs and an iOS simulator build.
