This document provides a high-level overview of the project. It describes what the application is, why it exists, the technologies it uses, and the overall design philosophy. It should help a developer understand the purpose and scope of the project before looking at implementation details.

# Project overview

Taurus is a modern replacement for the existing PegasusUI application.

Pegasus is a lightweight Jira-style work management application that has been used successfully for a number of years. Rather than incrementally modernising the existing UI, Taurus replaces it with a new Blazor-based web application while preserving the proven business functionality provided by PegasusApi.

Taurus initially focuses on reproducing the existing Pegasus user experience using modern technologies and responsive design. New features and workflow improvements will be introduced only after the existing functionality has been successfully replaced.

PegasusApi remains the system of record for all business data. Taurus consumes the existing API through its published abstractions and deliberately treats PegasusApi as an external dependency whose contract cannot be changed.

User authentication is provided by Soteria. Taurus authenticates users through Soteria while maintaining its own authenticated application session. API authorisation is intentionally deferred until after the initial UI replacement has been completed.

The project aims to provide a clean, maintainable and responsive application that is straightforward to understand, extend and support. The implementation favours explicit behaviour and pragmatic design over unnecessary abstraction.

# Goals

The primary goals of the project are:

- Replace PegasusUI.
- Preserve existing business functionality while modernising the user experience.
- Provide a fully responsive user interface.
- Keep the application simple to understand and maintain.
- Integrate with Soteria for user authentication.
- Build a foundation that supports future enhancements without introducing unnecessary complexity.

# Technology stack

## Backend

- .NET 10
- ASP.NET Core Blazor Web App
- PegasusApi
- Soteria

## Frontend

- MudBlazor 9.6
- Minimal JavaScript

## Technology Guidance

The technologies and versions listed above are the source of truth for this project.

When suggesting framework-specific implementations:

- Verify behaviour against the versions used by the project rather than relying on memory.
- Do not recommend APIs or features that are unavailable in the project's versions.
- Where official framework documentation conflicts with prior knowledge, prefer the documentation for the project's versions.

# High-level architecture

The application uses a Vertical Slice Architecture.

Each feature owns its own:

- Pages
- Components
- Services

Business logic is implemented within feature services.

The application consumes PegasusApi through a dedicated integration layer, allowing the UI to remain independent of the external API contract.

Shared functionality should emerge from proven implementation patterns rather than speculation.

# Folder structure

```text
Taurus
│
├── Components
│   ├── Features
│   ├── Layout
│   ├── Shared
│   ├── Status
│   └── Theme
├── Application
└── Docs
```

Features contain all UI and services relating to a single functional area.

Shared contains reusable UI components.

Application contains the application's integration and business services.

Docs contains the project documentation.

# Design principles

The project values:

- Explicit behaviour over hidden behaviour.
- Readability over cleverness.
- Purposeful abstraction over repeated implementation.
- Small, focused components and services.
- Clear ownership of responsibilities.
- Responsive design from the outset.
- Consistent user experience.

# Documentation overview

| Document | Purpose |
|----------|---------|
| project.md | What the project is. |
| architecture.md | How the application is structured. |
| decisions.md | Architectural decisions and project rules. |
| roadmap.md | What capabilities are delivered, and in what order. |
| delivery-plan.md | How we intend to deliver the current and upcoming work. |
| current-state.md | Current progress and next feature. |
| completed-development.md | Record of completed implementation work. |
| patterns.md | Proven implementation patterns. |
| backlog.md | Future work and enhancements. |
| collaboration.md | How the assistant should collaborate. |
| coding-conventions.md | Coding style and conventions. |
| decisions-log.md | Record of important architectural decisions. |

# Non-goals

The project deliberately does not aim to:

- Introduce unnecessary architectural layers.
- Couple the UI directly to the PegasusApi models.
- Optimise for hypothetical future requirements.