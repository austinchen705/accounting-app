## MODIFIED Requirements

### Requirement: System visualizes asset trend over time
The system SHALL render a personal asset trend chart using snapshot history, showing per-date stacked bucket values and total asset visibility, and SHALL provide both a summary chart view and a detailed full-screen chart view for the same dataset.

#### Scenario: Render summary trend chart with historical data
- **WHEN** there are two or more stored snapshots
- **THEN** the asset trend page displays a summary chart with dates on the X-axis and value on the Y-axis
- **AND** each date includes stacked bars for Stock, Cash, FirstTrade, and Property
- **AND** the chart includes a total asset indicator
- **AND** the summary chart MAY use compact value labels optimized for quick scanning

#### Scenario: Open detailed full-screen trend chart
- **WHEN** the user activates the chart expansion affordance from the asset trend page
- **THEN** the system opens a dedicated full-screen chart page for the same asset trend dataset
- **AND** the full-screen chart page MUST display the same dates and asset bucket values represented on the summary chart

#### Scenario: Detailed full-screen chart improves value readability
- **WHEN** the full-screen trend chart page is shown
- **THEN** the Y-axis MUST use denser tick marks than the summary chart
- **AND** the Y-axis labels MUST display full amount values rather than abbreviated compact values

#### Scenario: Empty-state trend chart behavior
- **WHEN** there are no stored snapshots
- **THEN** the system MUST show an empty-state message instead of an empty chart
