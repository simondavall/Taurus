This document provides a snapshot of the current implementation state of the project. 
It records completed work, the feature currently being developed, and the next expected 
steps. It should be updated regularly and is intended to help developers quickly understand 
where development should continue.

# Current phase

- Phase 1 – Application Foundation

# Current milestone

- Milestone 1.2 – Authentication

# Current task

- Implement login.

# Remaining milestone tasks

- Implement logout.
- Display authenticated user information.

# Completed

- Created the Taurus solution.
- Created the .NET 10 Blazor Web App project.
- Configured Interactive Server rendering.
- Verified the application builds successfully.
- Verified the application runs successfully.
- Created the initial project documentation.
- Added the MudBlazor package.
- Configured MudBlazor services.
- Added the required MudBlazor providers.
- Verified MudBlazor components render correctly.
- Created the Taurus application theme.
- Configured the application colour palette.
- Configured typography.
- Established the initial application icon vocabulary.
- Applied the theme globally.
- Verified the theme is applied- Replaced the generated Blazor layout with the Taurus shared application layout.
- Created the application header.
- Created the primary navigation drawer.
- Created the main content area.
- Added user-controlled navigation drawer toggle.
- Verified the shared layout is used consistently across the application.
- Established the initial Vertical Slice project structure.
- Created the application layer registration boundary.
- Organised pages into feature-based folders.
- Introduced application-level status components.
- Verified the application structure supports future feature development.
- Established responsive layout foundations.
- Verified desktop, tablet and mobile layout behaviour.
- Configured ASP.NET Core cookie authentication.
- Configured OpenID Connect authentication with Soteria.
- Enabled Authorization Code flow with PKCE.
- Configured token persistence.
- Configured OpenID Connect scopes.
- Configured authentication and authorization middleware.
- Adopted development-only `.env` configuration for local authentication settings.
- Restricted anonymous access to the sample pages.
- Protected the Counter page using ASP.NET Core authorization.
- Protected the Weather page using ASP.NET Core authorization.
- Verified anonymous users are challenged when accessing protected pages.
