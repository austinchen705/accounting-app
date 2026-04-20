## Context

The current asset trend page combines snapshot entry, a compact chart card, and snapshot history in a single screen. That layout works for quick status checks, but it constrains the chart height and forces the Y-axis labels to stay coarse, which makes older data points difficult to read precisely.

The existing data flow is already suitable for reuse: snapshot history is loaded once, transformed into chart series by the asset trend view model, and displayed through LiveCharts. The change therefore does not need a new data source or storage model. The main constraint is to add a detail-focused chart experience without making the existing page heavier or harder to scan.

## Goals / Non-Goals

**Goals:**
- Add a dedicated full-screen asset trend chart page navigated from the existing chart section.
- Keep the existing asset trend page chart as a summary view.
- Present denser Y-axis ticks and full amount labels on the full-screen chart page.
- Reuse the same snapshot-backed chart data so both chart views stay consistent.

**Non-Goals:**
- Changing how asset snapshots are stored, imported, or edited.
- Reworking the X-axis date strategy beyond what is necessary to reuse the current chart.
- Adding zoom, pan, or chart gesture editing.

## Decisions

### Use a separate full-screen page instead of an inline expansion state

The detailed chart will live on a new route/page rather than toggling the existing page into an expanded state. This keeps the current page simple, avoids mixing form/history layout state with chart presentation state, and makes the full-screen chart easier to test as an isolated surface.

Alternative considered: expanding the chart in place on the existing page. This was rejected because the page already contains multiple cards and edit flows, so inline expansion would create more UI state coupling for less clarity.

### Keep one data source but support two chart presentation modes

The current asset trend chart data should remain snapshot-driven from the existing asset trend flow. The implementation should distinguish between summary and detail chart presentation through axis configuration and label formatting, not by creating separate persistence or transformation pipelines.

Alternative considered: a completely separate view model with duplicated chart-building logic. This was rejected because it would increase drift risk between the summary and detailed chart views.

### Full-screen mode uses denser Y-axis ticks and full amount formatting

The summary chart keeps its compact readability-oriented formatting. The full-screen chart should use a tighter Y-axis step target and full numeric labels with grouping separators so users can inspect actual amounts without mental conversion from abbreviated labels.

Alternative considered: switching both charts to full-value labels. This was rejected because it would make the embedded chart noisier and reduce quick-scan readability on the main page.

## Risks / Trade-offs

- [Axis labels become crowded on smaller devices] → Keep the denser step target bounded and use the dedicated full-screen layout so the chart has more vertical room.
- [Chart configuration logic drifts between summary and detail modes] → Centralize the mode-specific axis configuration instead of duplicating ad hoc formatting.
- [Navigation adds one more page to maintain] → Keep the new page chart-only, with no duplicated form or history logic.

## Migration Plan

No data migration is required. The change is additive to the asset trend UI and can be rolled back by removing the full-screen route and its launch affordance.

## Open Questions

No open questions remain for implementation.
