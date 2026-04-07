# Transaction Form iOS Next Accessory Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add an iOS-only `Next` keyboard accessory for the transaction amount field so numeric keypad users can move into category selection reliably.

**Architecture:** Keep the page flow logic in `TransactionFormPage`, and add a narrow opt-in `Entry` attached property that only the amount field uses. On iOS, an `EntryHandler` mapper will read that property and install a `UIToolbar` accessory button that triggers the page's existing amount completion flow.

**Tech Stack:** .NET MAUI, XAML attached properties, iOS handler mapping, UIKit, xUnit source-contract tests.

---

### Task 1: Lock down the opt-in XAML and iOS accessory hook contracts

**Files:**
- Modify: `AccountingApp.Tests/TransactionFormLayoutTests.cs`
- Create: `AccountingApp.Tests/IosEntryAccessoryLayoutTests.cs`

**Step 1: Write the failing tests**

Add a transaction form assertion that the amount entry opts into the new accessory behavior:

```csharp
Assert.Contains("ios:IosEntryAccessory.Next=\"True\"", xaml);
```

Add a new iOS accessory contract test asserting the shared and iOS implementation surfaces exist:

```csharp
Assert.Contains("BindableProperty.CreateAttached", code);
Assert.Contains("nameof(Next)", code);
Assert.Contains("EntryHandler.Mapper.AppendToMapping", code);
Assert.Contains("InputAccessoryView", code);
Assert.Contains("UIBarButtonItem", code);
```

**Step 2: Run tests to verify they fail**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~TransactionFormLayoutTests|FullyQualifiedName~IosEntryAccessoryLayoutTests"
```

Expected: FAIL because the amount entry does not opt in and no accessory implementation exists yet.

**Step 3: Write minimal implementation support in XAML and shared code**

Create a shared attached-property helper and apply it in `TransactionFormPage.xaml`:

```xml
xmlns:ios="clr-namespace:AccountingApp.Platforms.iOS"
```

```xml
<Entry ...
       ios:IosEntryAccessory.Next="True" />
```

**Step 4: Run tests to verify partial green**

Run the same command as Step 2.

Expected: the XAML assertion passes; the iOS implementation assertions still fail.

**Step 5: Commit**

```bash
git add AccountingApp/Views/TransactionFormPage.xaml AccountingApp/Platforms/iOS/IosEntryAccessory.cs AccountingApp.Tests/TransactionFormLayoutTests.cs AccountingApp.Tests/IosEntryAccessoryLayoutTests.cs
git commit -m "test: define ios amount next accessory hooks"
```

### Task 2: Implement the iOS keyboard accessory toolbar

**Files:**
- Modify: `AccountingApp/MauiProgram.cs`
- Modify: `AccountingApp/Platforms/iOS/IosEntryAccessory.cs`
- Test: `AccountingApp.Tests/IosEntryAccessoryLayoutTests.cs`

**Step 1: Write the failing behavior test**

Extend the iOS accessory tests to assert the mapper installs a next button and forwards to the entry completion flow:

```csharp
Assert.Contains("new UIToolbar()", code);
Assert.Contains("new UIBarButtonItem(\"Next\"", code);
Assert.Contains("handler.PlatformView?.SendActionForControlEvents(UIControlEvent.EditingDidEndOnExit);", code);
Assert.Contains("handler.PlatformView?.ResignFirstResponder();", code);
```

**Step 2: Run tests to verify it fails**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~IosEntryAccessoryLayoutTests"
```

Expected: FAIL because the mapper does not yet install the toolbar.

**Step 3: Write minimal implementation**

In `IosEntryAccessory.cs`, add the attached property and iOS-only mapper logic:

```csharp
public static readonly BindableProperty NextProperty = BindableProperty.CreateAttached(...);
```

```csharp
EntryHandler.Mapper.AppendToMapping("IosEntryAccessory", (handler, view) =>
{
    if (!GetNext(view))
    {
        return;
    }

    handler.PlatformView.InputAccessoryView = CreateNextToolbar(handler);
});
```

Build a toolbar with a flexible space and a `Next` button that triggers the native entry submit path.

**Step 4: Run tests to verify it passes**

Run the same command as Step 2.

Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/MauiProgram.cs AccountingApp/Platforms/iOS/IosEntryAccessory.cs AccountingApp.Tests/IosEntryAccessoryLayoutTests.cs
git commit -m "feat: add ios amount next accessory"
```

### Task 3: Verify the end-to-end contract and iOS build

**Files:**
- Reuse previous files only if fixes are needed

**Step 1: Run focused tests**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~TransactionFormLayoutTests|FullyQualifiedName~IosEntryAccessoryLayoutTests"
```

Expected: PASS.

**Step 2: Run the full test suite**

Run:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal
```

Expected: PASS.

**Step 3: Build the iOS app target**

Run:

```bash
dotnet build AccountingApp/AccountingApp.csproj -f net8.0-ios -p:RuntimeIdentifier=iossimulator-arm64 -p:_DeviceName="iPhone 15 Pro"
```

Expected: BUILD SUCCEEDED.

**Step 4: Commit**

```bash
git add AccountingApp/MauiProgram.cs AccountingApp/Platforms/iOS/IosEntryAccessory.cs AccountingApp/Views/TransactionFormPage.xaml AccountingApp.Tests/TransactionFormLayoutTests.cs AccountingApp.Tests/IosEntryAccessoryLayoutTests.cs
git commit -m "feat: support ios next accessory on transaction amount"
```
