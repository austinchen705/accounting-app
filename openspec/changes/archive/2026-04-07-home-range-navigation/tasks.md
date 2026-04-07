## 1. Home range model

- [ ] 1.1 Introduce a home range enum for `Day`, `Week`, `Month`, `Year`, `All`.
- [ ] 1.2 Replace month-only state in `HomeViewModel` with `selectedRange` and `anchorDate`.
- [ ] 1.3 Add commands to switch range and navigate previous/next period, with `All` disabling navigation.
- [ ] 1.4 Add a computed period label that matches the selected range format.

## 2. Service updates

- [ ] 2.1 Add a reusable date-window helper or service method for `day/week/month/year/all`.
- [ ] 2.2 Add transaction query and summary methods that work on arbitrary date windows.
- [ ] 2.3 Keep monthly methods intact if they are still used elsewhere, but route homepage logic through the new range-based methods.

## 3. Home page UI

- [ ] 3.1 Add `Day / Week / Month / Year / All` range controls to `HomePage.xaml`.
- [ ] 3.2 Update the summary card title and navigation controls to reflect current range semantics.
- [ ] 3.3 Rename recent records copy from `最近記錄` to `期間內記錄`.
- [ ] 3.4 Update empty-state copy to `此期間尚無收支資料`.

## 4. Validation

- [ ] 4.1 Add or update tests for homepage layout to verify range controls and updated copy exist.
- [ ] 4.2 Add focused tests for period label generation and previous/next behavior across all ranges.
- [ ] 4.3 Add focused tests for `All` range summary behavior and navigation disabled state.
- [ ] 4.4 Run homepage-focused tests and `dotnet build AccountingApp/AccountingApp.csproj -f net8.0-ios -p:RuntimeIdentifier=iossimulator-arm64`.
