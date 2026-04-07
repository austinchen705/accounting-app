# Transaction Form Input Flow Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Improve the transaction form so amount entry advances into category selection, opening the date field scrolls the inline calendar into view, and finishing the date step advances to the note field.

**Architecture:** Keep this as a UI-only change centered on `TransactionFormPage` and `CalendarDatePicker`. The page will own the form-step sequencing, while the calendar control exposes lightweight notifications so the page can scroll and move focus at the correct time.

**Tech Stack:** .NET MAUI, XAML, code-behind event handling, xUnit layout/page tests.

---

### Task 1: Lock down the desired form flow with tests

**Files:**
- Modify: `AccountingApp.Tests/TransactionFormLayoutTests.cs`
- Modify: `AccountingApp.Tests/CalendarDatePickerLayoutTests.cs`

**Step 1: Write the failing tests**

Add assertions that describe the new interaction contract:

```csharp
Assert.Contains("x:Name=\"CategoryPicker\"", xaml);
Assert.Contains("Completed=\"OnAmountEntryCompleted\"", xaml);
Assert.Contains("x:Name=\"FormCalendarDatePicker\"", xaml);
Assert.Contains("NoteEntry", xaml);
```

For the calendar control/layout tests, assert that the control exposes the hook surface needed by the page:

```csharp
Assert.Contains("OpenCalendarCommand", code);
Assert.Contains("CloseCalendarCommand", code);
Assert.Contains("SelectCalendarDateCommand", code);
```

**Step 2: Run tests to verify they fail**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~TransactionFormLayoutTests|FullyQualifiedName~CalendarDatePickerLayoutTests"
```

Expected: FAIL because the category picker is unnamed and the current hook surface does not guarantee the new flow.

**Step 3: Write minimal implementation support in XAML**

Update `TransactionFormPage.xaml` to name the category picker:

```xml
<Picker x:Name="CategoryPicker"
        ItemsSource="{Binding Categories}"
        SelectedItem="{Binding SelectedCategory}"
        ItemDisplayBinding="{Binding Name}" />
```

Preserve the existing `AmountEntry`, `NoteEntry`, and `FormCalendarDatePicker` names so the page code can target them.

**Step 4: Run tests to verify partial green**

Run the same command as Step 2.

Expected: some tests pass, remaining tests still fail until page/control behavior is added.

**Step 5: Commit**

```bash
git add AccountingApp/Views/TransactionFormPage.xaml AccountingApp.Tests/TransactionFormLayoutTests.cs AccountingApp.Tests/CalendarDatePickerLayoutTests.cs
git commit -m "test: define transaction form input flow hooks"
```

### Task 2: Implement amount-to-category navigation in the page

**Files:**
- Modify: `AccountingApp/Views/TransactionFormPage.xaml`
- Modify: `AccountingApp/Views/TransactionFormPage.xaml.cs`

**Step 1: Write the failing behavior test or page contract check**

Extend the transaction form tests to assert the page code no longer routes `OnAmountEntryCompleted` to `NoteEntry.Focus()` directly.

Example source check:

```csharp
Assert.Contains("CategoryPicker.Focus()", code);
Assert.DoesNotContain("NoteEntry.Focus();", amountCompletedHandlerSnippet);
```

**Step 2: Run test to verify it fails**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~TransactionFormLayoutTests"
```

Expected: FAIL because the current handler still focuses the note entry.

**Step 3: Write minimal implementation**

Update `TransactionFormPage.xaml.cs`:

```csharp
private void OnAmountEntryCompleted(object? sender, EventArgs e)
{
    CategoryPicker.Focus();
}
```

**Step 4: Run tests to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/Views/TransactionFormPage.xaml AccountingApp/Views/TransactionFormPage.xaml.cs AccountingApp.Tests/TransactionFormLayoutTests.cs
git commit -m "feat: move transaction amount flow to category"
```

### Task 3: Add calendar open/complete hooks to the inline date picker

**Files:**
- Modify: `AccountingApp/Views/Controls/CalendarDatePicker.xaml.cs`
- Modify: `AccountingApp.Tests/CalendarDatePickerLayoutTests.cs`

**Step 1: Write the failing test**

Add a test that asserts `CalendarDatePicker` exposes lifecycle notifications for the hosting page.

Example:

```csharp
Assert.Contains("public event EventHandler? CalendarOpened;", code);
Assert.Contains("public event EventHandler? CalendarCompleted;", code);
```

**Step 2: Run test to verify it fails**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~CalendarDatePickerLayoutTests"
```

Expected: FAIL because the control currently has no lifecycle events.

**Step 3: Write minimal implementation**

Add lightweight events and raise them from the control:

```csharp
public event EventHandler? CalendarOpened;
public event EventHandler? CalendarCompleted;
```

Raise them in:
- `OpenCalendar()` after visibility changes
- `SelectCalendarDate(...)` after closing
- `CloseCalendarCommand` when the user taps completion

**Step 4: Run tests to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/Views/Controls/CalendarDatePicker.xaml.cs AccountingApp.Tests/CalendarDatePickerLayoutTests.cs
git commit -m "feat: expose transaction calendar lifecycle hooks"
```

### Task 4: Scroll the form to the calendar and continue to note

**Files:**
- Modify: `AccountingApp/Views/TransactionFormPage.xaml`
- Modify: `AccountingApp/Views/TransactionFormPage.xaml.cs`
- Test: `AccountingApp.Tests/TransactionFormLayoutTests.cs`

**Step 1: Write the failing test**

Add assertions that the transaction form page owns a named `ScrollView` and listens for calendar completion.

Example:

```csharp
Assert.Contains("x:Name=\"FormScrollView\"", xaml);
Assert.Contains("FormCalendarDatePicker.CalendarOpened", code);
Assert.Contains("FormCalendarDatePicker.CalendarCompleted", code);
Assert.Contains("await FormScrollView.ScrollToAsync", code);
Assert.Contains("NoteEntry.Focus()", code);
```

**Step 2: Run test to verify it fails**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~TransactionFormLayoutTests"
```

Expected: FAIL because the page is not wired to scroll or continue focus after the date step.

**Step 3: Write minimal implementation**

Update the form page:

```xml
<ScrollView x:Name="FormScrollView">
```

In `TransactionFormPage.xaml.cs`, subscribe to calendar events and implement:

```csharp
private async void OnCalendarOpened(object? sender, EventArgs e)
{
    await MainThread.InvokeOnMainThreadAsync(async () =>
    {
        await Task.Yield();
        await FormScrollView.ScrollToAsync(FormCalendarDatePicker, ScrollToPosition.MakeVisible, true);
    });
}

private void OnCalendarCompleted(object? sender, EventArgs e)
{
    NoteEntry.Focus();
}
```

Subscribe in the constructor and unsubscribe in `OnDisappearing()` if needed.

**Step 4: Run tests to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/Views/TransactionFormPage.xaml AccountingApp/Views/TransactionFormPage.xaml.cs AccountingApp.Tests/TransactionFormLayoutTests.cs
git commit -m "feat: scroll transaction form to inline calendar"
```

### Task 5: Verify the complete flow

**Files:**
- Reuse previous files only if fixes are needed

**Step 1: Run focused tests**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~TransactionFormLayoutTests|FullyQualifiedName~CalendarDatePickerLayoutTests"
```

Expected: PASS.

**Step 2: Run simulator build**

Run:

```bash
dotnet build AccountingApp/AccountingApp.csproj -f net8.0-ios -p:RuntimeIdentifier=iossimulator-arm64 -p:_DeviceName="iPhone 15 Pro"
```

Expected: `Build succeeded`.

**Step 3: Manual verification**

Verify on simulator/device:
- Enter amount and press keyboard next → category picker opens/focuses
- Tap date field → inline calendar opens and becomes visible without manual scrolling
- Select a date → focus moves to note
- Open date and press completion without changing date → focus also moves to note

**Step 4: Commit final polish**

```bash
git add AccountingApp/Views/TransactionFormPage.xaml AccountingApp/Views/TransactionFormPage.xaml.cs AccountingApp/Views/Controls/CalendarDatePicker.xaml.cs AccountingApp.Tests/TransactionFormLayoutTests.cs AccountingApp.Tests/CalendarDatePickerLayoutTests.cs
git commit -m "feat: improve transaction form input flow"
```
