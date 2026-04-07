# Transaction Form Enter Focus Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make the amount field on the transaction form move focus to the note field when the user presses Enter on the keyboard.

**Architecture:** Keep this as a view-only change in `TransactionFormPage.xaml` by naming the amount and note entries and wiring the amount entry's return action to move focus to the note entry. No ViewModel behavior change is needed.

**Tech Stack:** .NET MAUI, XAML, xUnit layout tests.

---

### Task 1: Add amount-entry return flow to the transaction form

**Files:**
- Modify: `AccountingApp.Tests/TransactionFormLayoutTests.cs`
- Modify: `AccountingApp/Views/TransactionFormPage.xaml`

**Step 1: Write the failing test**

Extend `TransactionFormLayoutTests.cs` with assertions that verify the amount entry is configured to move to the note entry:

```csharp
Assert.Contains("x:Name=\"AmountEntry\"", xaml);
Assert.Contains("x:Name=\"NoteEntry\"", xaml);
Assert.Contains("ReturnType=\"Next\"", xaml);
Assert.Contains("Completed=\"OnAmountEntryCompleted\"", xaml);
```

**Step 2: Run test to verify it fails**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~TransactionFormLayoutTests"
```

Expected: FAIL because the entries are unnamed and no Enter/Completed flow exists.

**Step 3: Write minimal implementation**

Update `TransactionFormPage.xaml` to:

```xml
<Entry x:Name="AmountEntry"
       Text="{Binding AmountText}"
       Keyboard="Numeric"
       ReturnType="Next"
       Completed="OnAmountEntryCompleted" />

<Entry x:Name="NoteEntry"
       Text="{Binding Note}"
       ReturnType="Done" />
```

Add the code-behind handler in `TransactionFormPage.xaml.cs`:

```csharp
private void OnAmountEntryCompleted(object? sender, EventArgs e)
{
    NoteEntry.Focus();
}
```

**Step 4: Run test to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/Views/TransactionFormPage.xaml AccountingApp/Views/TransactionFormPage.xaml.cs AccountingApp.Tests/TransactionFormLayoutTests.cs
git commit -m "feat: move transaction amount enter focus to note"
```
