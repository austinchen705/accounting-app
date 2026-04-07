## Purpose

Define the personal asset trend experience for recording dated asset snapshots, visualizing trend changes over time, and importing snapshot history from CSV.

## Requirements

### Requirement: User can record personal asset snapshots
The system SHALL allow users to create a dated asset snapshot with numeric values for Stock, Cash, FirstTrade, and Property.

#### Scenario: Create a valid snapshot
- **WHEN** the user submits a snapshot form with a date and valid numeric values for all four asset buckets
- **THEN** the system stores the snapshot as a new record
- **AND** the new snapshot appears in the snapshot history and trend chart

#### Scenario: Reject invalid snapshot input
- **WHEN** the user submits a snapshot with a missing date or non-numeric or negative value in any required bucket
- **THEN** the system MUST reject the submission
- **AND** the system MUST show a validation error describing the invalid field

### Requirement: User can manage snapshot history
The system SHALL allow users to view snapshots ordered by date and edit or delete existing snapshots.

#### Scenario: Edit an existing snapshot
- **WHEN** the user updates values or date for an existing snapshot and saves
- **THEN** the system persists the updated snapshot
- **AND** the history list and trend chart MUST reflect the updated values

#### Scenario: Delete an existing snapshot
- **WHEN** the user confirms deletion for a snapshot
- **THEN** the system removes the snapshot
- **AND** the history list and trend chart MUST no longer include that snapshot

### Requirement: System visualizes asset trend over time
The system SHALL render a personal asset trend chart using snapshot history, showing per-date stacked bucket values and total asset visibility.

#### Scenario: Render trend chart with historical data
- **WHEN** there are two or more stored snapshots
- **THEN** the system displays a trend chart with dates on the X-axis and value on the Y-axis
- **AND** each date includes stacked bars for Stock, Cash, FirstTrade, and Property
- **AND** the chart includes a total asset indicator

#### Scenario: Empty-state trend chart behavior
- **WHEN** there are no stored snapshots
- **THEN** the system MUST show an empty-state message instead of an empty chart

### Requirement: User can import snapshot data from CSV
The system SHALL allow users to replace existing snapshot records from a local CSV file.

#### Scenario: Import valid CSV rows
- **WHEN** the user selects a CSV file with valid rows containing date and four asset bucket values
- **THEN** the system confirms the replace-import action
- **AND** the system clears existing snapshot records before importing the new rows
- **AND** imported records appear in history and trend chart

#### Scenario: Handle invalid CSV rows
- **WHEN** the CSV contains rows with invalid date or malformed numeric values
- **THEN** the system MUST skip invalid rows
- **AND** the system MUST report import results including imported row count, skipped row count, and error details
