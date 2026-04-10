# Settings App Expiration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Show the installed app version and installation expiration date on the settings page.

**Architecture:** Add a small app-info service in the MAUI app layer to provide version text and parse the embedded provisioning profile expiration date when available. Keep `SettingsViewModel` responsible only for display strings, and render the new information in a compact localized card on `SettingsPage`.

**Tech Stack:** .NET MAUI, XAML, ViewModel binding, xUnit.

---

### Task 1: Add tests for settings app-info surface

**Files:**
- Modify: `AccountingApp.Tests/SettingsPageLayoutTests.cs`
- Modify: `AccountingApp.Tests/SettingsLanguageRefreshTests.cs`

**Step 1: Write failing tests**
- Assert the settings page contains a localized app-info section and binds version/expiration strings.
- Assert the settings view model exposes `AppVersionText` and `AppExpirationDateText`.

**Step 2: Run tests to verify they fail**
- Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~SettingsPageLayoutTests|FullyQualifiedName~SettingsLanguageRefreshTests"`

**Step 3: Write minimal implementation**
- Add an app-info service.
- Inject it into `SettingsViewModel`.
- Add the settings-page card and localized copy.

**Step 4: Run tests to verify they pass**
- Run the same command as Step 2.

**Step 5: Commit**
- `git commit -m "feat: show app expiration info in settings"`
