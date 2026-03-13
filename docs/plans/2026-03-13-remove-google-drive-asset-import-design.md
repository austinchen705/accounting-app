# Remove Google Drive Asset Import Design

## Goal

Remove the Google Drive based asset snapshot import path from the asset trend feature and keep local file based CSV import as the only supported import workflow.

## Scope

In scope:

- remove the Google Drive import button from the asset trend page
- remove the Google Drive asset import command and flow from the asset trend view model
- remove asset-trend-specific Google Drive contract expectations from tests
- preserve local CSV import

Out of scope:

- changing existing Google Drive database backup and restore behavior
- changing Google Drive settings UI
- changing local CSV parsing or import summary behavior

## Architecture

This is a feature rollback, not a broad Google Drive removal. The app should continue to use `GoogleDriveService` for backup and restore in settings, but the asset trend page and view model should no longer reference or inject it.

The rollback should be limited to the asset trend UI surface and its related tests. Any temporary debug logic added specifically for the Google Drive asset import investigation should be removed as part of the same cleanup.

## User Experience

After the change:

- the asset trend page shows only the local `匯入 CSV` action
- selecting import uses the system file picker only
- no Google Drive asset import prompts or debug alerts appear in the asset trend flow

## Testing

Verify:

- asset trend page no longer binds `ImportFromGoogleDriveCommand`
- asset trend view model no longer declares the Google Drive import command or Google Drive debug strings
- local CSV import command still exists
- full automated test suite passes
