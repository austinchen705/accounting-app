## Purpose

Define the transaction form experience for surfacing and selecting frequent categories.

## Requirements

### Requirement: Transaction form shows frequent categories for the current type
The system SHALL show up to 6 frequent categories above the full category picker in the transaction form for the currently selected transaction type.

#### Scenario: Show top frequent expense categories
- **WHEN** the user opens the transaction form with `expense` selected
- **THEN** the system SHALL display up to 6 expense categories ranked by usage count from highest to lowest
- **AND** the system SHALL exclude income categories from the frequent category section

#### Scenario: Show top frequent income categories
- **WHEN** the user switches the transaction form to `income`
- **THEN** the system SHALL refresh the frequent category section using only income transaction history
- **AND** the system SHALL exclude expense categories from the frequent category section

### Requirement: Frequent categories are directly selectable
The system SHALL allow users to select a frequent category directly from the transaction form without opening the full category picker.

#### Scenario: Select category from frequent chips
- **WHEN** the user taps a frequent category chip
- **THEN** the system SHALL set that category as the selected category for the transaction
- **AND** the full category picker SHALL reflect the same selected category

#### Scenario: Highlight selected frequent category
- **WHEN** the selected category is included in the frequent category section
- **THEN** the matching frequent category chip SHALL display a selected visual state

### Requirement: Full category picker remains available as fallback
The system SHALL preserve the full category picker after adding frequent categories.

#### Scenario: Select a non-frequent category
- **WHEN** the user chooses a category that is not in the top 6 from the full picker
- **THEN** the system SHALL keep that category selected for save
- **AND** the transaction form SHALL not require the category to appear in the frequent category section

### Requirement: Edit mode preserves the original category selection
The system SHALL preserve the existing category when opening an existing transaction in edit mode.

#### Scenario: Edit transaction with frequent category
- **WHEN** the user opens an existing transaction whose category is in the frequent category section
- **THEN** the system SHALL show that category as selected in both the frequent category section and the full picker

#### Scenario: Edit transaction with non-frequent category
- **WHEN** the user opens an existing transaction whose category is not in the frequent category section
- **THEN** the system SHALL show that category as selected in the full picker
- **AND** the transaction SHALL remain editable without changing the category first
