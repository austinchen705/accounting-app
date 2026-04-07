## 1. Transaction Form Focus Flow

- [ ] 1.1 Add or update layout tests that describe the amount-to-category focus path and date-picker interaction hooks.
- [ ] 1.2 Update `TransactionFormPage.xaml` to name the category picker and preserve the existing amount/date/note structure needed for focus transitions.
- [ ] 1.3 Update `TransactionFormPage.xaml.cs` so the amount completed handler advances to category selection and the page can scroll to the calendar when it opens.

## 2. Calendar Interaction Hooks

- [ ] 2.1 Extend `CalendarDatePicker.xaml.cs` with lightweight notifications for opened and completed/closed states.
- [ ] 2.2 Wire the transaction form page to calendar open/close notifications so it scrolls to the date picker and then advances focus to the note field.
- [ ] 2.3 Verify that selecting a date and pressing the calendar completion action both produce the expected note-focus behavior.

## 3. Verification

- [ ] 3.1 Run focused transaction form and calendar-related tests.
- [ ] 3.2 Run an iOS simulator build to verify the updated page and control compile cleanly.
- [ ] 3.3 Manually verify the flow on simulator/device: amount → category, date opens in view, date completion → note.
