## Purpose

Define the guided input flow for the transaction form between amount, category, date, and note entry.

## Requirements

### Requirement: Transaction form SHALL guide users from amount to category selection
The transaction form SHALL advance the user's input flow from the amount field into category selection when the amount field completes.

#### Scenario: Amount field advances to category selection
- **WHEN** the user finishes entering an amount and triggers the keyboard next action
- **THEN** the form SHALL move focus to the category selection control instead of the note field

### Requirement: Transaction form SHALL bring the inline calendar into view when opened
The transaction form SHALL automatically scroll to the inline date picker when the user opens the date selection step.

#### Scenario: Opening date scrolls the calendar into view
- **WHEN** the user taps the date field in the transaction form
- **THEN** the inline calendar SHALL become visible
- **AND** the form SHALL scroll so the expanded calendar is visible without requiring manual scrolling

### Requirement: Transaction form SHALL continue to note entry after the date step completes
The transaction form SHALL advance the user's flow to the note field after the inline date picker is completed.

#### Scenario: Selecting a date moves focus to note
- **WHEN** the user selects a date from the inline calendar
- **THEN** the calendar SHALL close
- **AND** the form SHALL move focus to the note field

#### Scenario: Closing the calendar without changing the date moves focus to note
- **WHEN** the user closes the inline calendar with the completion action
- **THEN** the form SHALL move focus to the note field
