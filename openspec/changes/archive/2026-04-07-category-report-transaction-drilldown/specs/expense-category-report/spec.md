## MODIFIED Requirements

### Requirement: Category report presents a donut chart and ranked list
The system SHALL render the expense category summary as a donut chart paired with a ranked category list that use the same filtered dataset, and SHALL let the user drill into a category's matching expense transactions from that ranked list.

#### Scenario: Show report summary
- **WHEN** the selected range contains expense transactions
- **THEN** the donut chart SHALL show all categories with non-zero expense in the filtered range
- **AND** the chart center SHALL display the report label and total expense amount
- **AND** the category list SHALL show category name, transaction count, amount, and percentage for each displayed category

#### Scenario: Show empty state
- **WHEN** the selected range contains no expense transactions
- **THEN** the page SHALL show a clear empty-state message for the category report
- **AND** the report SHALL not display misleading chart slices or list rows

#### Scenario: Open category transaction drill-down
- **WHEN** the user selects a category row from the ranked category list
- **THEN** the app SHALL navigate to a category transaction detail page for that category
- **AND** the detail page SHALL keep using the currently selected report range and period as its filter context

## ADDED Requirements

### Requirement: Category report drill-down shows grouped expense transactions
The system SHALL display the selected category's matching expense transactions grouped by date on the category transaction detail page.

#### Scenario: Show date-grouped category transactions
- **WHEN** the selected category has matching expense transactions in the current report range
- **THEN** the detail page SHALL group transactions by date
- **AND** the date groups SHALL be ordered from newest to oldest
- **AND** transactions within each date group SHALL be ordered from newest to oldest

#### Scenario: Restrict drill-down to current category and filter range
- **WHEN** the user opens the detail page from the category report
- **THEN** the detail page SHALL include only transactions from the selected category
- **AND** the detail page SHALL include only transactions that match the current range and anchor period used by the category report

#### Scenario: Return to category report context
- **WHEN** the user navigates back from the category transaction detail page
- **THEN** the app SHALL return to the category report page
- **AND** the previously selected range and period SHALL remain unchanged
