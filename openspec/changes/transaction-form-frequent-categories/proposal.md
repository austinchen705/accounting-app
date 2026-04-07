## Why

The transaction form currently relies on a single category picker, which is slow on mobile when users repeatedly choose the same few categories. We need a faster category selection flow that prioritizes the most frequently used categories without removing access to the full category list.

## What Changes

- Add a frequent-category section to the transaction form for both create and edit flows.
- Rank frequent categories by transaction usage count within the current transaction type (`expense` or `income`), highest to lowest.
- Show the top 6 frequent categories as directly tappable chips above the full category picker.
- Keep the full category picker as a fallback for less common categories.
- Preserve correct category selection when editing an existing transaction, regardless of whether that category appears in the top 6.

## Capabilities

### New Capabilities
- `transaction-form-frequent-categories`: Surface the top 6 most-used categories in the transaction form and keep the full picker available as a fallback.

### Modified Capabilities

## Impact

- Affected code: `AccountingApp/ViewModels/TransactionFormViewModel.cs`, `AccountingApp/Views/TransactionFormPage.xaml`, category / transaction data access used to compute usage counts, and related layout / view model tests.
- APIs: no external API changes.
- Dependencies: no new package dependencies expected.
