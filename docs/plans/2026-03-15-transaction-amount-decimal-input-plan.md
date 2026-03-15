# Transaction Amount Decimal Input Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make the transaction amount field accept only numeric input with a single decimal point, while preserving smooth typing flow on iOS simulator/device.

**Architecture:** Replace direct `decimal` Entry binding with a string input property (`AmountText`) in `TransactionFormViewModel`, sanitize input on each change, and parse/validate into `decimal` only at save time. Keep `Keyboard="Numeric"` for keypad hint, but enforce correctness in app logic through deterministic sanitizer methods and unit tests.

**Tech Stack:** .NET MAUI (XAML + ViewModel), xUnit (`AccountingApp.Tests`), C#

---

### Task 1: Add failing sanitizer tests first

**Files:**
- Create: `AccountingApp.Tests/TransactionAmountInputSanitizerTests.cs`
- Modify: `AccountingApp.Tests/AccountingApp.Tests.csproj` (only if compile include is needed)

**Step 1: Write the failing test**

```csharp
using AccountingApp.ViewModels;

namespace AccountingApp.Tests;

public class TransactionAmountInputSanitizerTests
{
    [Theory]
    [InlineData("123", "123")]
    [InlineData("12.34", "12.34")]
    [InlineData("..12a3.4", ".1234")]
    [InlineData("1.2.3", "1.23")]
    [InlineData("abc", "")]
    public void Sanitize_keeps_digits_and_single_dot(string raw, string expected)
    {
        var actual = TransactionAmountInputSanitizer.Sanitize(raw);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("100", true, 100)]
    [InlineData("0.5", true, 0.5)]
    [InlineData(".5", true, 0.5)]
    [InlineData("0", false, 0)]
    [InlineData("", false, 0)]
    [InlineData("..", false, 0)]
    public void TryParsePositiveDecimal_parses_only_positive_values(string input, bool ok, decimal expected)
    {
        var result = TransactionAmountInputSanitizer.TryParsePositiveDecimal(input, out var value);
        Assert.Equal(ok, result);
        if (ok) Assert.Equal(expected, value);
    }
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj --filter "FullyQualifiedName~TransactionAmountInputSanitizerTests"`

Expected: FAIL because `TransactionAmountInputSanitizer` does not exist yet.

**Step 3: Commit**

```bash
git add AccountingApp.Tests/TransactionAmountInputSanitizerTests.cs
git commit -m "test: add failing tests for amount input sanitizer"
```

### Task 2: Implement minimal sanitizer to pass tests

**Files:**
- Create: `AccountingApp/ViewModels/TransactionAmountInputSanitizer.cs`
- Test: `AccountingApp.Tests/TransactionAmountInputSanitizerTests.cs`

**Step 1: Write minimal implementation**

```csharp
using System.Globalization;
using System.Text;

namespace AccountingApp.ViewModels;

public static class TransactionAmountInputSanitizer
{
    public static string Sanitize(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;

        var sb = new StringBuilder(raw.Length);
        var hasDot = false;
        foreach (var ch in raw)
        {
            if (char.IsDigit(ch))
            {
                sb.Append(ch);
                continue;
            }

            if (ch == '.' && !hasDot)
            {
                sb.Append(ch);
                hasDot = true;
            }
        }

        return sb.ToString();
    }

    public static bool TryParsePositiveDecimal(string? input, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var normalized = Sanitize(input);
        if (!decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            return false;
        if (parsed <= 0m) return false;

        value = parsed;
        return true;
    }
}
```

**Step 2: Run test to verify it passes**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj --filter "FullyQualifiedName~TransactionAmountInputSanitizerTests"`

Expected: PASS.

**Step 3: Commit**

```bash
git add AccountingApp/ViewModels/TransactionAmountInputSanitizer.cs AccountingApp.Tests/TransactionAmountInputSanitizerTests.cs
git commit -m "feat: add amount sanitizer and positive decimal parser"
```

### Task 3: Move TransactionForm amount binding from decimal to sanitized string

**Files:**
- Modify: `AccountingApp/ViewModels/TransactionFormViewModel.cs`
- Modify: `AccountingApp/Views/TransactionFormPage.xaml`
- Test: `AccountingApp.Tests/TransactionFormLayoutTests.cs`

**Step 1: Write failing layout assertions**

Add assertions to ensure amount Entry binds to `AmountText` and remains numeric keyboard:

```csharp
[Fact]
public void TransactionForm_amount_entry_binds_to_amounttext_and_uses_numeric_keyboard()
{
    var xaml = ReadTransactionFormXaml();
    Assert.Contains("Text=\"{Binding AmountText}\"", xaml);
    Assert.Contains("Keyboard=\"Numeric\"", xaml);
}
```

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj --filter "FullyQualifiedName~TransactionFormLayoutTests"`

Expected: FAIL because XAML still binds `Amount`.

**Step 2: Minimal ViewModel changes**

Implement string property and sanitized setter:

```csharp
private string _amountText = string.Empty;

public string AmountText
{
    get => _amountText;
    set
    {
        var sanitized = TransactionAmountInputSanitizer.Sanitize(value);
        if (_amountText == sanitized) return;
        _amountText = sanitized;
        OnPropertyChanged();
    }
}
```

Update load/edit path:
- In `LoadExistingAsync`, set `AmountText = txn.Amount.ToString(CultureInfo.InvariantCulture);`

Update save path:
- Replace `if (Amount <= 0)` with sanitizer parse:

```csharp
if (!TransactionAmountInputSanitizer.TryParsePositiveDecimal(AmountText, out var amount))
{
    ErrorMessage = "請輸入有效金額（大於 0）";
    HasError = true;
    return;
}
```

- Use `Amount = amount` when constructing transaction model.

**Step 3: Minimal XAML change**

```xml
<Entry Text="{Binding AmountText}" Placeholder="0" Keyboard="Numeric" FontSize="28" />
```

**Step 4: Run tests**

Run:
- `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj --filter "FullyQualifiedName~TransactionAmountInputSanitizerTests"`
- `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj --filter "FullyQualifiedName~TransactionFormLayoutTests"`

Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp/ViewModels/TransactionFormViewModel.cs AccountingApp/Views/TransactionFormPage.xaml AccountingApp.Tests/TransactionFormLayoutTests.cs
git commit -m "feat: bind amount input to sanitized decimal text"
```

### Task 4: Regression tests for save validation behavior

**Files:**
- Create: `AccountingApp.Tests/TransactionFormAmountValidationTests.cs`
- Modify: `AccountingApp/ViewModels/TransactionFormViewModel.cs` (if command extraction is required for testability)

**Step 1: Write failing validation test**

Target behavior:
- Input `"12.5"` can save
- Input `"abc"` cannot save and returns error message

Example test skeleton:

```csharp
[Fact]
public async Task Save_rejects_invalid_amount_text_and_sets_error_message()
{
    // Arrange VM with fake services and category
    // vm.AmountText = "abc";
    // Act await invoke save
    // Assert vm.HasError true and message set
}
```

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj --filter "FullyQualifiedName~TransactionFormAmountValidationTests"`

Expected: FAIL first.

**Step 3: Add minimal seams for testability (if needed)**

If direct `Shell.Current.GoToAsync` blocks test execution, extract navigation behind a tiny interface and default implementation:
- `IAppNavigator.GoBackAsync()`
- ViewModel calls navigator instead of static Shell directly.

Keep this minimal and YAGNI.

**Step 4: Re-run targeted tests**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj --filter "FullyQualifiedName~TransactionFormAmountValidationTests"`

Expected: PASS.

**Step 5: Commit**

```bash
git add AccountingApp.Tests/TransactionFormAmountValidationTests.cs AccountingApp/ViewModels/TransactionFormViewModel.cs
git commit -m "test: add regression coverage for decimal amount save validation"
```

### Task 5: Full verification and final commit

**Files:**
- Verify touched files only

**Step 1: Run complete test suite**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj`

Expected: All tests PASS.

**Step 2: Run iOS simulator build sanity check**

Run: `dotnet build AccountingApp/AccountingApp.csproj -f net8.0-ios -p:RuntimeIdentifier=iossimulator-arm64`

Expected: Build succeeded.

**Step 3: Manual behavior check**

Run app on simulator and verify:
- Amount field accepts `123`, `12.34`, `.5`
- Non-digit characters do not remain in field
- Multiple dots are collapsed to a single dot
- Save rejects empty/zero

**Step 4: Final commit**

```bash
git add AccountingApp/ViewModels/TransactionFormViewModel.cs AccountingApp/ViewModels/TransactionAmountInputSanitizer.cs AccountingApp/Views/TransactionFormPage.xaml AccountingApp.Tests/TransactionAmountInputSanitizerTests.cs AccountingApp.Tests/TransactionFormLayoutTests.cs AccountingApp.Tests/TransactionFormAmountValidationTests.cs
git commit -m "feat: enforce decimal-only amount input in transaction form"
```
