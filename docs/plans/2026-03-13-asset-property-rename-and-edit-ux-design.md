# Asset Property Rename And Edit UX Design

## Goal

Rename the asset snapshot field currently called `Fund3` to `Property`, and improve the asset trend edit interaction so editing a snapshot is clear and discoverable.

## Scope

In scope:

- rename the data field from `Fund3` to `Property`
- rename the CSV column from `Fund3` to `Property`
- update all asset trend UI labels to `Property(房產)`
- improve edit UX so tapping edit clearly moves the user into edit mode

Out of scope:

- dynamic user-configurable asset bucket names
- changing the number of asset buckets
- Google Drive import behavior

## Data Model

The rename should be consistent end-to-end:

- `AssetSnapshot.Fund3` becomes `AssetSnapshot.Property`
- trend transformation output should expose `PropertyValues`
- CSV parsing should expect `Property` instead of `Fund3`

This keeps the code, CSV schema, and UI terminology aligned. We should not keep `Fund3` internally as an alias because that would preserve confusion rather than remove it.

## CSV Format

The new CSV format should be:

```csv
Date,Stock,Cash,FirstTrade,Property
2026-01-01,100000,20000,5000,3000
```

For this change, the importer can be strict and require `Property` rather than supporting both `Fund3` and `Property`. That matches the approved “rename everywhere” direction and keeps the parsing rules simple.

## UI Changes

All displayed `Fund3` text should become `Property(房產)`.

This includes:

- form labels
- snapshot history labels
- chart legend / series labels
- any import or summary text that exposes the bucket name

## Edit UX

The current edit behavior reuses the top form, but it is easy to miss because the user has to notice the page state changed and then scroll back up manually.

The smallest useful fix is:

- tapping `編輯` loads the selected snapshot into the top form
- the page scrolls to the top automatically
- the form header changes to something like `編輯資產快照`
- the primary button stays in sync with edit mode, for example `更新快照`
- `取消編輯` remains visible during edit mode
- optionally show the date being edited so the state change is obvious

This keeps the existing architecture and avoids adding a modal or inline editor.

## Testing

Add or update tests for:

- `AssetSnapshot.Property` and total helper behavior
- CSV parsing with `Property` header
- trend transformation using `Property`
- asset trend page/layout strings showing `Property(房產)`
- edit contract/layout hints showing edit-mode UX surface

## Recommendation

Do a strict rename from `Fund3` to `Property` across data model, CSV, tests, and UI, then improve edit UX by making the reused top form explicit and auto-scrolling into view. This gives the cleanest long-term naming and fixes the current discoverability problem with minimal architectural churn.
