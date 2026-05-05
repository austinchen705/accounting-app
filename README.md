# accounting-app

Personal accounting app built with .NET MAUI.

## Project structure

- `AccountingApp/`: MAUI app UI, services, resources, and platform files
- `AccountingApp.Core/`: shared domain and business logic
- `AccountingApp.Tests/`: xUnit tests

## Common commands

Run tests:

```bash
dotnet test AccountingApp.Tests/AccountingApp.Tests.csproj -f net8.0 -v minimal
```

Build for the iOS simulator:

```bash
dotnet build AccountingApp/AccountingApp.csproj -c Debug -f net8.0-ios -p:RuntimeIdentifier=iossimulator-arm64
```

## iOS device release build and deploy

This project currently needs the debug entitlements file when deploying a Release build to an iPhone from the CLI. The normal iOS entitlements file includes `aps-environment`, which does not match the Personal Team provisioning profile used in local development.

Verified command:

```bash
dotnet build AccountingApp/AccountingApp.csproj \
  -t:Run \
  -c Release \
  -f net8.0-ios \
  -p:RuntimeIdentifier=ios-arm64 \
  -p:_DeviceName=00008130-000C5C1236D8001C \
  -p:CodesignKey="Apple Development: awei705@gmail.com (DYXHU43BUQ)" \
  -p:CodesignEntitlements="Platforms/iOS/Entitlements.Debug.plist"
```

Notes:

- Replace `_DeviceName` with the target device UDID from `xcrun xctrace list devices`.
- Replace `CodesignKey` with the signing identity available on the current Mac.
- Keep `CodesignEntitlements` pointed at `Platforms/iOS/Entitlements.Debug.plist` for this local Personal Team workflow.
