## 1. Google Drive Foundation

- [x] 1.1 Add Google Drive/OAuth dependencies and configuration placeholders (client id, scopes)
- [x] 1.2 Implement `GoogleDriveService` skeleton with authorize, pick-folder, upload, download methods
- [x] 1.3 Persist OAuth token and selected `folderId`/`folderName` in local settings

## 2. Fixed Filename Backup/Restore Logic

- [x] 2.1 Implement backup query in configured folder for `accounting_backup.db`
- [x] 2.2 Implement backup create-or-update flow for fixed filename
- [x] 2.3 Implement restore flow to download `accounting_backup.db` and replace local DB
- [x] 2.4 Add clear error handling for missing folder, missing file, expired token, and network failure

## 3. App Integration

- [x] 3.1 Wire Settings actions to Google Drive service (replace iCloud backup/restore entry points)
- [x] 3.2 Add first-time folder selection flow when no `folderId` exists
- [x] 3.3 Update settings copy/UI labels from iCloud to Google Drive
- [x] 3.4 Remove startup auto-restore dependency from cloud sync path

## 4. Validation

- [x] 4.1 Add unit tests for fixed filename behavior (exists => update, missing => create)
- [x] 4.2 Add tests for restore missing-file and invalid-folder scenarios
- [x] 4.3 Run `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal`
- [ ] 4.4 Run iOS build smoke test for simulator and one real-device manual flow
