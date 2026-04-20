# Settings Expiration Timezone Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Show Settings app expiration with full time precision in `GMT+08:00`.

**Architecture:** Keep the expiration source in `embedded.mobileprovision`, move parsing to `DateTimeOffset`, and format through a dedicated helper that always converts to `GMT+08:00`. Add focused service tests so the format is locked without relying on a device package.

**Tech Stack:** .NET MAUI, C#, xUnit

---

### Task 1: Make expiration formatting timezone-aware

**Files:**
- Modify: `AccountingApp/Services/AppInstallInfoService.cs`
- Create: `AccountingApp/Properties/AssemblyInfo.cs`
- Test: `AccountingApp.Tests/AppInstallInfoServiceTests.cs`

**Step 1: Write the failing test**

Add a service test that formats a UTC expiration timestamp and expects `yyyy/MM/dd HH:mm:ss GMT+08:00`.

**Step 2: Run test to verify it fails**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AppInstallInfoServiceTests"`

Expected: FAIL because the service currently returns only a date.

**Step 3: Write minimal implementation**

Update `AppInstallInfoService` to:
- parse `ExpirationDate` as `DateTimeOffset`
- convert it to `GMT+08:00`
- format it as `yyyy/MM/dd HH:mm:ss GMT+08:00`
- keep the existing fallback text

**Step 4: Run test to verify it passes**

Run: `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~AppInstallInfoServiceTests|FullyQualifiedName~SettingsPageLayoutTests|FullyQualifiedName~SettingsLanguageRefreshTests"`

Expected: PASS

**Step 5: Commit**

```bash
git add docs/plans/2026-04-20-settings-expiration-timezone-design.md docs/plans/2026-04-20-settings-expiration-timezone.md AccountingApp/Services/AppInstallInfoService.cs AccountingApp/Properties/AssemblyInfo.cs AccountingApp.Tests/AppInstallInfoServiceTests.cs
git commit -m "feat: show settings expiration time with timezone"
```
