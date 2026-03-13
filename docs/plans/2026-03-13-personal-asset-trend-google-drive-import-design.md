# Personal Asset Trend Google Drive Import Design

## Goal

Allow the personal asset trend page to import `AssetSnapshot` data directly from the same configured Google Drive backup folder used by database backup/restore, without relying on the system file picker.

## User Flow

From the asset trend page, the user taps a new "從 Google Drive 匯入" action. The app reads the currently configured Google Drive backup folder, lists the available `.csv` files in that folder, lets the user choose one, warns that the import will replace all current asset snapshots, and then downloads and imports the selected CSV.

The import result should refresh the page state immediately and report:

- imported row count
- skipped row count
- row-level error details for invalid rows

## Scope

In scope:

- list `.csv` files from the configured Google Drive backup folder
- choose one file from an action sheet
- replace existing `AssetSnapshot` data before import
- surface import summary and row-level errors in the UI
- preserve the existing backup/restore flow

Out of scope:

- automatic background sync
- importing files outside the configured backup folder
- mixing append-vs-replace modes for this Google Drive flow
- changing the database backup file workflow

## Architecture

The Google Drive folder selection, authorization, and file download logic already exists in `GoogleDriveService`. The cleanest extension is to add folder file listing and targeted file download helpers there, then consume them from the asset trend view model via the existing service layer.

`AssetSnapshotService` should own the replace-import behavior so that the UI does not manually orchestrate delete-all-plus-import operations. The view model should remain responsible for user prompts, file choice, state refresh, and presenting import results.

## Components

### GoogleDriveService

Add a small file metadata model and methods to:

- list files in the configured backup folder
- filter candidate files to `.csv`
- download a selected file's content

This should reuse the existing stored folder id and authorization path rather than introducing a second Drive client flow.

### AssetSnapshotService

Add a replace-import method that:

1. initializes the database
2. deletes all existing asset snapshots
3. parses the downloaded CSV
4. inserts valid snapshots
5. returns the same import summary object with error details

### AssetTrendViewModel

Add a `ImportFromGoogleDriveCommand` that:

1. asks `GoogleDriveService` for available `.csv` files in the configured backup folder
2. prompts the user to choose one
3. shows a destructive confirmation because current asset snapshots will be cleared
4. downloads and imports the selected CSV through `AssetSnapshotService`
5. reloads snapshots and chart
6. updates imported/skipped/error-detail UI state

The existing file-picker import can remain for now unless we explicitly decide to remove it later.

### AssetTrendPage

Add a dedicated Google Drive import button and reuse the existing import summary/error presentation area.

## Error Handling

- If Google Drive is not configured, reuse the existing Google Drive configuration/authorization behavior.
- If no `.csv` files exist in the folder, show a clear empty-state alert.
- If the user cancels file selection or confirmation, do nothing.
- If the download or parse fails, show the error in the existing asset trend error message area.
- Invalid CSV rows should not fail the full import; they should be counted and reported in error details.

## Testing

Add coverage for:

- CSV import summary includes row-level error details
- replace import clears old snapshots before adding new ones
- ViewModel contract includes Google Drive import command and import detail surface
- page layout binds the new Google Drive import action
- full test suite still passes

## Recommendation

Extend `GoogleDriveService` and keep the import orchestration split between the app service and the view model. This reuses the existing backup-folder concept, keeps Google Drive logic out of XAML/UI code, and minimizes behavioral drift from the current backup implementation.
