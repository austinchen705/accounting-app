# Asset Trend Prefill Latest Snapshot Design

## Goal

Add a shortcut in the asset trend create form so a user can start a new asset snapshot by copying the latest saved asset values, then adjusting them before saving.

## Scope

In scope:

- add a create-mode button that prefills the form from the latest saved snapshot
- keep the create date on today when prefilling
- leave edit mode behavior unchanged
- hide or disable the shortcut when there is no prior snapshot to copy

Out of scope:

- changing snapshot persistence rules
- auto-prefilling the form without explicit user action
- copying values while editing an existing snapshot

## UX

The asset trend page already uses one top form for both create and edit flows. The new shortcut should stay inside that form and only appear in create mode, because the user's intent is "start a new entry from the latest one" rather than "overwrite the current edit."

The button should sit near the other form actions. Pressing it should:

- load the most recent snapshot values into `Stock`, `Cash`, `FirstTrade`, and `Property`
- preserve `SnapshotDate` as today instead of reusing the old snapshot date
- clear any prior error state so the copied draft feels like a fresh create action

When no snapshot history exists, the button should not be shown. That keeps the empty-state form simple and avoids a dead action.

## Data Flow

No new storage is needed. The ViewModel can derive the latest snapshot from the already-loaded `Snapshots` collection, which is refreshed from `AssetSnapshotService.GetAllAsync()` and sorted newest-first for display.

The copy flow should:

1. confirm the page is in create mode
2. read the latest snapshot from `Snapshots`
3. copy the four asset amount fields into the form
4. keep `SnapshotDate` unchanged

Because the source data is already in memory, no new service method is required unless the existing collection shape makes the logic awkward.

## Testing

Add or update tests for:

- ViewModel surface exposing a prefill command and create-mode visibility state
- ViewModel behavior that copies latest amounts but does not change the date
- page layout wiring for the new button
- localized resource strings for the new action label

## Recommendation

Implement the feature as an explicit create-mode-only button backed by ViewModel logic that copies the newest saved snapshot values from the current in-memory collection. This keeps the behavior obvious, avoids extra persistence API changes, and matches the approved interaction with minimal architectural churn.
