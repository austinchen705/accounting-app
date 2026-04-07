## ADDED Requirements

### Requirement: Transaction form supports one attachment image per transaction
The system SHALL allow the user to attach one image to a transaction from either the camera or the photo library.

#### Scenario: Attach image from camera
- **WHEN** the user chooses the camera option from the transaction form
- **THEN** the app SHALL capture one photo
- **AND** the form SHALL associate that photo with the current transaction draft as its attachment image

#### Scenario: Attach image from photo library
- **WHEN** the user chooses the photo library option from the transaction form
- **THEN** the app SHALL let the user select one photo
- **AND** the form SHALL associate that photo with the current transaction draft as its attachment image

### Requirement: Transaction attachment is stored as an app-managed local file
The system SHALL store the transaction attachment image as an app-managed file in sandbox storage and SHALL persist only relative file-reference metadata in the transaction record.

#### Scenario: Save transaction with attachment
- **WHEN** the user saves a transaction that currently has an attachment image
- **THEN** the app SHALL persist a relative attachment path with the transaction record
- **AND** the referenced file SHALL exist in app-managed local storage

#### Scenario: Existing transaction loads attachment state
- **WHEN** the user opens an existing transaction that has an attachment image
- **THEN** the form SHALL load the persisted attachment state
- **AND** the user SHALL be able to view or replace the existing image without re-selecting it first

### Requirement: Transaction attachment can be viewed, replaced, and removed
The system SHALL let the user view the current attachment image, replace it with a different image, or remove it from the transaction.

#### Scenario: View attachment image
- **WHEN** the transaction form has an attachment image and the user chooses to view it
- **THEN** the app SHALL display the full attachment image in an image-viewing surface

#### Scenario: Replace attachment image
- **WHEN** the transaction form has an existing attachment image and the user selects a different image
- **THEN** the new image SHALL become the transaction's current attachment
- **AND** the previously attached file SHALL no longer remain linked after the transaction update succeeds

#### Scenario: Remove attachment image
- **WHEN** the transaction form has an existing attachment image and the user removes it
- **THEN** the transaction SHALL save without an attachment image
- **AND** the previously attached file SHALL be deleted from app-managed storage after the update succeeds

### Requirement: Transaction attachment lifecycle follows transaction lifecycle
The system SHALL keep managed attachment files consistent with transaction create, update, and delete operations.

#### Scenario: Delete transaction with attachment
- **WHEN** the user deletes a transaction that has an attachment image
- **THEN** the transaction record SHALL be removed
- **AND** the managed attachment file SHALL also be deleted

#### Scenario: Failed save does not orphan replacement state
- **WHEN** the user stages a new attachment image but the transaction save fails
- **THEN** the transaction SHALL keep its previously persisted attachment state
- **AND** the app SHALL not leave the transaction pointing at a missing file

### Requirement: Attachment import reduces oversized source images
The system SHALL normalize imported attachment images to a smaller managed file suitable for receipt viewing and local storage.

#### Scenario: Persist compressed managed image
- **WHEN** the user imports a large camera or library image
- **THEN** the app SHALL write a managed image file optimized for local storage
- **AND** the transaction attachment feature SHALL not require storing the full original photo bytes in the database
