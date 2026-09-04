This document provides a snapshot of the current implementation state of the project. 
It records completed work, the feature currently being developed, and the next expected 
steps. It should be updated regularly and is intended to help developers quickly understand 
where development should continue.

# Current phase

- Enhancements

# Current milestone

- None

# Current task

- Restructure project into abstracted layers.

# Remaining milestone tasks

- Introduce caching

# Completed

- Implemented project Latest Version.
    - Added Latest Version to project editing and the project listing.
    - Displayed Latest Version alongside the project name on Ticket Details.
- Replaced Global Fixed In Release Requirement with Per-Project Setting.
    - Added Require Fixed In Release to project editing and the project listing.
    - Replaced the global Fixed In Release completion setting with per-project behaviour.
    - Applied the project-specific requirement consistently to ticket creation and editing.
    - Added Fixed In Release support to ticket creation.
- Verified the application builds, starts and affected project and ticket workflows operate successfully.
- Added 'No tickets' text if filter returns no tickets for a project.
- Changed Ticket Details Cancel behaviour to discard unsaved changes and refresh the current ticket from persisted data without leaving the page.