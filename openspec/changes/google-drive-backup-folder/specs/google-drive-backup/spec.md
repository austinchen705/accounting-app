## ADDED Requirements

### Requirement: User-selected Google Drive backup folder
The system SHALL allow the user to select a Google Drive folder and persist the selected folder identifier for subsequent backup and restore operations.

#### Scenario: First-time folder selection
- **WHEN** user taps Google Drive backup/restore and no folder has been configured
- **THEN** the system prompts the user to authorize Google Drive and select a destination folder
- **AND** the selected `folderId` is persisted for future operations

#### Scenario: Reusing existing folder setting
- **WHEN** user has already configured a Google Drive folder
- **THEN** backup/restore operations use the stored `folderId` without prompting again

### Requirement: Fixed backup filename behavior
The system SHALL use a fixed backup filename `accounting_backup.db` within the configured Google Drive folder.

#### Scenario: Backup when file exists
- **WHEN** a file named `accounting_backup.db` already exists in the configured folder
- **THEN** the system updates/replaces that file with the current local database content

#### Scenario: Backup when file does not exist
- **WHEN** no file named `accounting_backup.db` exists in the configured folder
- **THEN** the system creates a new file with that name and uploads the local database content

### Requirement: Restore from fixed backup file
The system SHALL restore local database content only from `accounting_backup.db` in the configured Google Drive folder.

#### Scenario: Restore success
- **WHEN** user triggers restore and `accounting_backup.db` exists in configured folder
- **THEN** system downloads the file and replaces local database content
- **AND** user sees a success message

#### Scenario: Backup file missing
- **WHEN** user triggers restore and `accounting_backup.db` does not exist in configured folder
- **THEN** system shows a clear error message indicating backup file was not found

### Requirement: Manual cloud operation model
The system SHALL provide manual backup and restore actions in settings and SHALL NOT auto-restore data from cloud during app startup.

#### Scenario: Startup behavior
- **WHEN** app starts
- **THEN** app initializes local database only
- **AND** app does not automatically overwrite local database from Google Drive

#### Scenario: Manual actions in settings
- **WHEN** user opens settings
- **THEN** user can trigger manual backup to Google Drive and manual restore from Google Drive
