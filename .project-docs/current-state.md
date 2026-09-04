This document provides a snapshot of the current implementation state of the project. 
It records completed work, the feature currently being developed, and the next expected 
steps. It should be updated regularly and is intended to help developers quickly understand 
where development should continue.

# Current phase

# Enhancements

# Current milestone

- None

# Current task

- Add "No Tickets" dcisplay when no tickets are returned from selection.

# Remaining milestone tasks

- Only include Active projects in All selection.
- Restructure project into abstracted layers.
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