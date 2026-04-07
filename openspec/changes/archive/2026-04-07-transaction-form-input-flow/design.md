## Context

`TransactionFormPage` already has a partial keyboard flow: the amount `Entry` uses `ReturnType="Next"` and its completed handler currently moves focus straight to the note field. The page also embeds `CalendarDatePicker` inline below the primary form grid, which means opening the date control does not automatically scroll the calendar into view. The interaction problem is therefore page-level, not domain-level.

## Goals / Non-Goals

**Goals:**
- Let the amount field advance to category selection instead of skipping directly to note.
- Make date selection feel like part of the same linear input flow.
- Auto-scroll the form when the inline calendar is opened so the expanded control is immediately visible.
- Move focus to note after the date step completes.
- Keep the change localized to form UI code and tests.

**Non-Goals:**
- Do not redesign the transaction form layout.
- Do not replace the inline calendar with a modal or bottom sheet.
- Do not change `TransactionFormViewModel` save logic or data model.
- Do not introduce a full-field focus manager abstraction for the whole app.

## Decisions

1. **Keep the change in page + control code-behind**
   - Decision: implement the flow in `TransactionFormPage.xaml.cs` and expose lightweight events from `CalendarDatePicker`.
   - Why: this is a UI sequencing problem, and moving it into the ViewModel would add complexity without adding business value.
   - Alternative: model focus state in the ViewModel. Rejected because it adds state and coupling for a platform-specific interaction issue.

2. **Treat category as the next focus target after amount**
   - Decision: name the category `Picker` in XAML and focus it from the amount completed handler.
   - Why: this matches the desired user flow and preserves the current visual layout.
   - Alternative: jump straight to date. Rejected because the user explicitly wants category first.

3. **Expose date-picker lifecycle notifications**
   - Decision: add lightweight notifications from `CalendarDatePicker` for opened and completed/closed states so the host page can scroll and advance focus.
   - Why: the page needs reliable hooks after the inline calendar changes visibility.
   - Alternative: infer calendar state by polling bindable properties. Rejected because it is brittle and harder to test.

4. **Scroll after the calendar expands**
   - Decision: once the date picker opens, schedule a scroll-to-calendar action on the page after layout updates.
   - Why: scrolling before the calendar is visible can land on the wrong Y position.
   - Alternative: scroll immediately on tap. Rejected because it may run before the control expands.

## Risks / Trade-offs

- [Risk] `Picker.Focus()` behavior can vary across platforms → Mitigation: keep the flow simple and verify on iOS simulator/device; if needed, fall back to opening the picker through tap handling later.
- [Risk] scroll timing could still race with layout on slower devices → Mitigation: perform the scroll from the page after the calendar reports that it opened, using main-thread deferred execution.
- [Trade-off] adding control events slightly increases coupling between `TransactionFormPage` and `CalendarDatePicker` → Mitigation: keep the event surface minimal and specific to form navigation.
