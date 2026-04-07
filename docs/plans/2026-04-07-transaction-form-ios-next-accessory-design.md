# Transaction Form iOS Next Accessory Design

**Problem:** `AmountEntry` uses `Keyboard="Numeric"`, and on iOS that keyboard often does not expose a usable return or next key. The existing `Completed` handler is therefore not a reliable way to advance from amount input to category selection.

**Goal:** Add an iOS-only keyboard accessory toolbar for the transaction amount field so users can tap `Next` above the numeric keypad and trigger the existing amount-to-category input flow.

**Scope:**
- Only affect the transaction form amount entry.
- Only change iOS behavior.
- Reuse the existing `OnAmountEntryCompleted` flow instead of inventing a second navigation path.

**Approach:**
- Add an opt-in attached property for `Entry` that marks fields which should receive an iOS next accessory.
- Apply that property only to `AmountEntry` in `TransactionFormPage.xaml`.
- Register an iOS `EntryHandler` mapping that detects the opt-in property and installs a `UIToolbar` as `InputAccessoryView`.
- Put a `Next` button on that toolbar; tapping it calls the existing completion path so focus moves to `CategoryPicker`.

**Why this approach:**
- It avoids global behavior changes to every numeric `Entry`.
- It keeps platform-specific code in the iOS path instead of leaking UIKit concerns into page code.
- It leaves Android and other targets unchanged.

**Testing:**
- Add source-contract tests to verify the attached property is applied in XAML.
- Add source-contract tests to verify the iOS mapper and accessory toolbar code exist.
- Run focused transaction form tests, then the full test suite, and compile the iOS app target to catch platform build issues.
