## Purpose

Define how statistics trend charts choose dynamic Y-axis steps for the visible data range.

## Requirements

### Requirement: Statistics trend charts use dynamic Y-axis scaling
The system SHALL choose Y-axis step sizes for the statistics trend charts based on the current data range instead of using a fixed `50,000` step.

#### Scenario: Use fine-grained steps for small values
- **WHEN** all visible values in a statistics trend chart are below large-money ranges such as `50,000`
- **THEN** the chart SHALL choose a smaller Y-axis step size appropriate to that range
- **AND** the chart SHALL show readable intermediate ticks such as `1k`, `2k`, or `5k` style intervals rather than a single coarse step

#### Scenario: Keep coarse steps for large values
- **WHEN** the visible values in a statistics trend chart span large-money ranges
- **THEN** the chart SHALL choose a larger Y-axis step size suitable for that range
- **AND** the chart SHALL avoid overcrowding the Y-axis with excessive labels

### Requirement: Statistics trend charts share one axis-scaling rule
The system SHALL apply the same dynamic Y-axis scaling strategy to both the main income/expense trend chart and the category trend chart on the statistics page.

#### Scenario: Apply consistent scaling logic across both charts
- **WHEN** the statistics page builds Y axes for the two trend charts
- **THEN** both charts SHALL use the same dynamic scaling strategy
- **AND** the chosen step size SHALL be derived from each chart's own visible data values
