# Repository Guidelines

## Project Structure & Module Organization

`AccountingApp/` contains the .NET MAUI app UI, view models, services, and resources. `AccountingApp.Core/` holds shared domain and service logic. `AccountingApp.Tests/` contains xUnit coverage for layout contracts, view model behavior, and service logic. Planning and workflow artifacts live in `openspec/` and `docs/plans/`. Use `.worktrees/` for isolated feature workspaces; do not develop directly on `master`.

## Build, Test, and Development Commands

- `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal`: run the full test suite.
- `dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal --filter "FullyQualifiedName~Statistics"`: run focused tests while iterating.
- `dotnet build AccountingApp/AccountingApp.csproj -f net8.0-ios -p:RuntimeIdentifier=iossimulator-arm64 -p:_DeviceName="iPhone 15 Pro"`: build or run the iOS simulator target locally.
- `openspec status --change "<name>"`: inspect OpenSpec progress for a feature change.

## Coding Style & Naming Conventions

Use 4-space indentation and existing C# / XAML style. Prefer clear MVVM naming: `*Page.xaml`, `*ViewModel.cs`, `*Service.cs`, `*Tests.cs`. Keep user-facing copy in `.resx` resources, not hard-coded strings. Follow existing commit prefixes from history, e.g. `feat:`, `fix:`, `test:`.

## Testing Guidelines

This repo uses xUnit. Add or update focused tests before implementation when changing behavior. Keep test names descriptive, e.g. `StatisticsCategoryTrendTests` or `SettingsLanguageRefreshTests`. Before claiming completion, run the relevant focused tests and then the full `dotnet test` command above.

## Commit & Pull Request Guidelines

Prefer small, reviewable commits with one concern each. PRs should include a short summary, the main user-visible impact, and the exact test command used. Include screenshots for UI changes such as MAUI page updates or chart behavior changes.

## Agent Workflow

For feature work, follow this sequence: use Superpowers `brainstorming` first, then generate OpenSpec artifacts with fast-forward flow, create an isolated branch with `using-git-worktrees`, expand tasks into `docs/plans/` with `writing-plans`, and implement with `executing-plans` or `subagent-driven-development`. Finish with `requesting-code-review`, `finishing-a-development-branch`, and archive the change in OpenSpec after merge.
