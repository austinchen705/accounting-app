# Settings Expiration Timezone Design

## Goal

Show the app installation expiration value in Settings with full time precision and a fixed `GMT+08:00` timezone suffix.

## Scope

- Keep the data source as the bundled `embedded.mobileprovision` file.
- Preserve the existing fallback when expiration cannot be determined.
- Change display formatting only; no layout structure changes are required.

## Design

`AppInstallInfoService` should continue parsing `ExpirationDate` from the provisioning profile, but it should preserve the original offset by using `DateTimeOffset` instead of `DateTime`. The final UI string should always convert that value to `GMT+08:00` before formatting.

The target display format is:

`yyyy/MM/dd HH:mm:ss GMT+08:00`

To keep the change testable, parsing and formatting logic should be factored into small helpers that can be exercised without reading the actual app package file.

## Testing

- Add focused service tests for:
  - formatting a UTC expiration into `GMT+08:00`
  - fallback behavior when expiration cannot be parsed
- Keep existing settings page layout tests unchanged.
