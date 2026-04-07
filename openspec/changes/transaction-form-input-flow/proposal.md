## Why

The transaction form currently has a broken input flow on mobile: after entering an amount, users cannot move directly into category selection, and opening the date picker does not bring the calendar into view. These gaps slow down high-frequency record entry and make the form feel disconnected.

## What Changes

- Improve the transaction form focus flow so the amount field advances into category selection instead of jumping to the note field.
- Make the category selection step participate in the form flow for both create and edit modes.
- When the user opens the date picker, automatically scroll the form so the calendar is visible without manual swiping.
- After the user selects or closes the date picker, continue the form flow by moving focus to the note field.
- Keep the existing transaction form layout and data model intact; this change only improves UI interaction flow.

## Capabilities

### New Capabilities
- `transaction-form-input-flow`: Provide guided input navigation across amount, category, date, and note fields in the transaction form.

### Modified Capabilities

## Impact

- Affected code: `AccountingApp/Views/TransactionFormPage.xaml`, `AccountingApp/Views/TransactionFormPage.xaml.cs`, `AccountingApp/Views/Controls/CalendarDatePicker.xaml.cs`, and related layout/page tests.
- APIs: no external API changes.
- Dependencies: no new package dependencies expected.
