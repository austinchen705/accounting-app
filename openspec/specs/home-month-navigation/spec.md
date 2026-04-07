## Purpose

Define month-based navigation and month-scoped summary behavior on the home page.

## Requirements

### Requirement: Home page month navigation controls
The system MUST provide month navigation controls on the home summary card so users can switch the active month backward and forward.

#### Scenario: Switch to previous month
- **WHEN** the user taps the previous-month control on the home page
- **THEN** the active month SHALL change to the immediately previous calendar month

#### Scenario: Switch to next month
- **WHEN** the user taps the next-month control on the home page
- **THEN** the active month SHALL change to the immediately next calendar month

### Requirement: Home monthly summary follows selected month
The system MUST compute and display home-page income, expense, and balance using the currently selected month.

#### Scenario: Summary updates after month change
- **WHEN** the active month changes on the home page
- **THEN** the displayed income, expense, and balance SHALL refresh using data from that selected month

### Requirement: Home recent transactions are scoped to selected month
The system MUST display recent transactions on the home page from the selected month only.

#### Scenario: Monthly-scoped recent list
- **WHEN** the home page loads data for a selected month
- **THEN** the recent transactions list SHALL include only transactions whose date falls within that month
- **AND** the list SHALL show at most 10 records sorted newest first

#### Scenario: Empty month state
- **WHEN** the selected month has no transactions
- **THEN** the home page SHALL show an empty-state message indicating there is no data for the selected month
