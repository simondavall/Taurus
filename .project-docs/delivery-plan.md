# Delivery Plan

This document expands the roadmap into implementation milestones and tasks.

Unlike the roadmap, which describes the long-term delivery of the project, this document records the expected sequence of implementation work.

The roadmap should remain relatively stable.

The delivery plan is expected to evolve as implementation progresses and understanding of the existing Pegasus application increases.

Tasks listed here are intentionally concise. Detailed scope, goals and implementation notes belong in the Jira task.

---

# Phase 1 – Application Foundation

## Milestone 1.1 – Project Foundation

- ✓ Create the Taurus solution.
  - ✓ Create the solution.
  - ✓ Create the Blazor Web App project.
  - ✓ Verify the application builds.
  - ✓ Verify the application runs successfully.

- ✓ Configure MudBlazor.
  - ✓ Add the MudBlazor packages.
  - ✓ Configure the required MudBlazor services.
  - ✓ Configure the required MudBlazor providers.
  - ✓ Verify MudBlazor components render correctly.

- ✓ Establish the application theme.
  - ✓ Create the Taurus theme.
  - ✓ Configure the colour palette.
  - ✓ Configure typography.
  - ✓ Configure icons.
  - ✓ Verify the theme is applied consistently.

- ✓ Establish the shared application layout.
  - ✓ Create the main application layout.
  - ✓ Create the application header.
  - ✓ Create the navigation menu.
  - ✓ Create the main content area.
  - ✓ Verify the shared layout is used throughout the application.

- ✓ Establish the project structure.
  - ✓ Create the feature folder structure.
  - ✓ Create the shared component structure.
  - ✓ Create the application layer.
  - ✓ Configure dependency injection.
  - ✓ Verify the project structure supports feature development.

- ✓ Configure responsive layout foundations.
  - ✓ Establish responsive breakpoints.
  - ✓ Verify desktop layout behaviour.
  - ✓ Verify tablet layout behaviour.
  - ✓ Verify mobile layout behaviour.
  - ✓ Establish reusable responsive layout patterns.

- ✓ Create the initial project documentation.
  - ✓ Create the project document.
  - ✓ Create the architecture document.
  - ✓ Create the decisions document.
  - ✓ Create the roadmap.
  - ✓ Create the delivery plan.
  - ✓ Create the backlog.
  - ✓ Create the remaining project documentation.

**Deliverable**

A runnable Taurus application with the shared layout established.

---

## Milestone 1.2 – Authentication

- ✓ Integrate authentication with Soteria.
- ✓ Configure OpenID Connect authentication.
- ✓ Restrict anonymous access to sample pages.
- ✓ Implement login.
- ✓ Implement logout.
- ✓ Display authenticated user information.
- ✓ Implement authentication-first application flow.
- ✓ Handle denied application access.
- ✓ Validate application environment configuration and authentication redirects.

**Deliverable**

Users authenticate through Soteria before accessing the Taurus application.

---

## Milestone 1.3 – Initial Project Listing with hardcoded data

- ✓ Setup hard coded project data in Application layer.
- ✓ Display projects.

**Deliverable**

Users can view project listing.

---

## Milestone 1.4 – PegasusApi Integration

- ✓ Configure PegasusApi connectivity.
- ✓ Create the Taurus application layer.
- ✓ Establish the API integration pattern.
- ✓ Create the initial request and response mapping.
- ✓ Allow ability to use query string options.
- ✓ Establish initial Production deployment to IIS.
- ✓ Implement application logging.
- ✓ Configure Persistent Data Protection Keys

**Deliverable**

Taurus communicates successfully with PegasusApi through the established integration layer.

---

# Phase 2 – Project Management

## Milestone 2.1 – Project Administration

- ✓ Tidy up project listing.
- ✓ Create projects.
- ✓ Edit projects.
- ✓ Implement consistent PegasusApi error handling.
- ✓ Delete projects.
- ✓ Preserve existing Pegasus project behaviour.

**Deliverable**

Users can administer projects.

---

# Phase 3 – Ticket Management

## Milestone 3.1 – Ticket Listing

- ✓ Display tickets listing.
- ✓ Support project filtering.
- ✓ Support predefined ticket filters.
- ✓ Add ticket listing state indicators.

**Deliverable**

Users can browse and locate tickets.

---

## Milestone 3.2 – Ticket Details

- ✓ Display and edit ticket details.
- ✓ Display comments.
- ✓ Add comments.
- ✓ Update ticket status.
- ✓ Create tickets.

**Deliverable**

Users can inspect, create and update tickets.

---

## Milestone 3.3 – Ticket Editing

- ✓ Review and standardise editor validation.
- Implement Sub Tasks
- Enforce ticket completion restrictions
- Enforce 'fixed in release'
- Display related information.
- Support navigation between tickets.
- Assign tickets.
- Verify existing Pegasus behaviour preserved.

---

# Phase 4 – User Experience

## Milestone 4.1 – Responsive Experience

- Optimise desktop layouts.
- Optimise tablet layouts.
- Optimise mobile layouts.
- Refine responsive navigation.

**Deliverable**

The application provides a responsive experience across supported devices.

---

## Milestone 4.2 – Application Experience

- Improve navigation.
- Improve loading feedback.
- Improve validation presentation.
- Improve error presentation.
- Improve visual consistency.

**Deliverable**

The application provides a polished and consistent user experience.

---

# Phase 5 – Settings

## Milestone 5.1 – User Preferences

- Implement user preferences.
- Persist preferences in cookies.
- Apply preference defaults.
- Preserve preferences across sessions.

**Deliverable**

Users can personalise their application experience.

---

## Milestone 5.2 – Application Settings

- Replace remaining settings screens.
- Preserve existing Pegasus behaviour.
- Complete the settings experience.

**Deliverable**

Existing application settings are fully available in Taurus.

---

## Enhancements

Future enhancements outside the planned implementation phases will be recorded here as the project evolves.