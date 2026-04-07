## Purpose

Define the standalone expense category report experience that is available from bottom navigation.

## Requirements

### Requirement: Category report tab is available from bottom navigation
The system SHALL provide a sixth bottom navigation tab labeled `分類報告` that opens a dedicated expense category report page without replacing the existing `統計` tab.

#### Scenario: Open category report tab
- **WHEN** the user selects the `分類報告` tab from the bottom navigation
- **THEN** the app SHALL display the dedicated category report page
- **AND** the existing `統計` tab SHALL remain available as a separate destination

### Requirement: Category report supports fixed expense time ranges
The system SHALL let the user switch the expense category report between `week`, `month`, `year`, and `all` ranges.

#### Scenario: Switch report range
- **WHEN** the user changes the selected range filter
- **THEN** the report SHALL recalculate the displayed expense category summary using the newly selected range
- **AND** the page SHALL update the visible period label to match that range

#### Scenario: Navigate bounded periods
- **WHEN** the selected range is `week`, `month`, or `year` and the user taps the previous or next period control
- **THEN** the report SHALL load the corresponding previous or next period for that same range

#### Scenario: View all-time report
- **WHEN** the selected range is `all`
- **THEN** the report SHALL display totals aggregated across all available expense transactions
- **AND** the page SHALL not require period navigation to view the report

### Requirement: Category report summarizes expense categories only
The system SHALL include only `expense` transactions in the category report and SHALL convert amounts into the configured base currency before aggregation.

#### Scenario: Exclude income transactions
- **WHEN** a selected range contains both income and expense transactions
- **THEN** the category report SHALL calculate category totals, counts, and percentages from expense transactions only

#### Scenario: Aggregate category values
- **WHEN** the selected range contains expense transactions across multiple categories
- **THEN** the report SHALL show each category's total amount, transaction count, and percentage of total expense
- **AND** the category list SHALL be ordered by amount descending, then category name ascending for ties

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
