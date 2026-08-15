This document captures the architectural and design decisions that guide development of the project. These decisions are considered project rules and should be followed consistently unless there is a compelling reason to change them. The document focuses on long-lived decisions rather than implementation details or current progress.

# Architecture

- Use Vertical Slice Architecture.
- Organise the application by feature rather than technical layer.
- Keep business logic within feature services.
- PegasusApi is an external dependency.
- Do not couple the UI directly to PegasusApi request or response models.
- Keep API integration behind a dedicated application layer.
- Shared functionality should be actively considered whenever patterns emerge.
- Introduce abstractions that remove repeated implementation while preserving explicit behaviour.
- Prefer small abstractions with a single responsibility.
- Avoid abstractions that hide application flow or introduce unnecessary indirection.

# Rendering

- Use Interactive Server rendering throughout the application.
- Keep PegasusApi communication on the server.
- Do not call PegasusApi directly from browser code.

# Data

- PegasusApi remains the system of record.
- Do not introduce a Taurus database unless a demonstrated requirement exists.
- Store lightweight user preferences and persistent application context in protected browser local storage.
- Treat persisted browser state as scoped to the browser profile rather than explicitly to the authenticated user.
- Every persisted value must have a reliable default or fallback behaviour.

# Authentication

- Authenticate users through Soteria.
- Maintain an application-local authenticated session.
- Obtain the current user identifier from the authenticated principal.
- Never allow editable UI input to determine the acting user.

# Dependencies

- Prefer first-party .NET features where practical.
- No MediatR.
- No AutoMapper.
- Use MudBlazor as the primary UI framework.
- Wrap third-party libraries behind Taurus abstractions where practical.
- Render user-authored HTML only after sanitising it through the Taurus-owned HTML sanitisation boundary.
- Keep persisted user-authored HTML unchanged; sanitisation is a presentation concern rather than a persistence transformation.

# Feature Design

- Each feature owns its pages, components and services.
- Pages coordinate workflow.
- Components encapsulate reusable UI.
- Feature services own business logic and API integration.
- Avoid business logic in Razor markup.

# Components

- Prefer Razor code-behind for pages and larger components.
- Keep Razor markup declarative.
- Shared components are first-class citizens.
- Extract shared components only after duplication appears.
- Shared components should expose an API consistent with MudBlazor where practical.
- Shared components should be preferred over repeated UI patterns once a pattern has been proven.

# Editors

- Edit entities in dialogs where practical.
- Keep simple editors on a single page.
- Use tabs only when they meaningfully improve organisation.
- Place Save and Cancel actions at the bottom right.
- Editors validate before closing.

# Validation

- Validation belongs in the UI layer.
- Use MudBlazor form validation where practical.
- Shared input components should participate in form validation.
- Feature services may assume UI validation has completed, but remain responsible for enforcing business rules and application security.
- Prefer extending shared components rather than duplicating validation logic.

# Styling

- MudBlazor owns the application theme.
- Third-party components should match the MudBlazor look and feel.
- Keep project styling in project CSS rather than modifying third-party libraries.

# User Experience

- Preserve existing Pegasus functionality before introducing enhancements.
- Optimise for efficient day-to-day task management.
- Minimise unnecessary page navigation.
- Prefer inline editing where it improves workflow.
- Use icon buttons with tooltips for common actions.
- Provide immediate visual feedback after user actions.
- Design for responsive behaviour from the outset.

# Visual Language

| Element | Style |
|----------|-------|
| Page title | `MudText Typo.h4` |
| Primary action | Filled Primary button |
| Secondary action | Outlined button |
| Delete | Error icon button |
| Save | Bottom right |
| Cancel | Bottom left |
| Success | Snackbar |
| Delete confirmation | Dialog |

# Shared Patterns

- Encourage abstractions that remove mechanical duplication.
- Discourage abstractions that hide behaviour or obscure control flow.
- Optimise for maintainability over minimal code.
- Build abstractions from proven implementations rather than speculation.

# Philosophy

- Prefer explicit behaviour to hidden behaviour.
- Prefer readability to cleverness.
- Introduce abstractions only after they have demonstrated clear value.
- Prefer abstractions that reduce duplication without hiding behaviour.
- Optimise for maintainability.
- Focus effort where it provides real value.