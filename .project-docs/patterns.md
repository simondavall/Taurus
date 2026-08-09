This document records proven implementation patterns that have emerged during development. Unlike decisions.md, which defines project rules, this document provides practical examples of how common features are implemented. New features should follow these patterns where appropriate to maintain consistency across the application.

## Application Theme

- Define the application theme in a dedicated `TaurusTheme` class.
- Expose the theme through a static `Default` property.
- Apply the theme globally using the root `MudThemeProvider`.
- Centralise application icon identifiers in a dedicated `TaurusIcons` class using semantic names rather than embedding Material icon constants throughout components.
- Keep feature components theme-aware by using MudBlazor semantic colours (`Color.Primary`, `Color.Secondary`, `Color.Success`, etc.) instead of hard-coded colour values.

## Shared Application Layout

- The application shell is implemented in `MainLayout`.
- Keep `MainLayout.razor` declarative and place behaviour in `MainLayout.razor.cs`.
- Use `MudLayout` as the root layout component.
- Host global providers (`MudThemeProvider`, `MudPopoverProvider`, `MudDialogProvider` and `MudSnackbarProvider`) before the layout.
- Use `MudAppBar` for application-level actions and branding.
- Use a `MudDrawer` for primary navigation.
- Keep the application name in the app bar only; avoid duplicate branding within the navigation drawer.
- Allow the navigation drawer to be collapsed from the app bar while leaving responsive behaviour to the responsive layout implementation.
- Render routed page content inside `MudMainContent`.

## Project Structure

- Organise user-facing functionality under `Components/Features`.
- Each feature owns its pages, components and feature-specific code.
- Shared UI belongs in `Components/Shared`.
- Application-wide status pages belong in `Components/Status`.
- Shared application shell components belong in `Components/Layout`.
- Application theme assets belong in `Components/Theme`.
- Application services and integration infrastructure belong under `Application`.

## Dependency Injection

- Expose application-layer registrations through a single `DependencyInjection` extension class.
- Register the application layer from `Program.cs` using `AddTaurusApplication()`.
- Register services explicitly as they are introduced.
- Do not introduce marker interfaces or automatic assembly scanning.

## Application Services

- Define application-facing service interfaces where the service implementation is expected to change while its consumer contract remains stable.
- Keep application models independent of external API transport models.
- UI features consume Taurus-owned application models through application services.
- Register application services explicitly through `AddTaurusApplication()`.

## PegasusApi Integration

- Configure the PegasusApi base address through application configuration.
- Validate required PegasusApi configuration during application startup.
- Use typed `HttpClient` registrations for feature services that communicate with PegasusApi.
- Use the published `PegasusApi.Abstractions` package for PegasusApi request and response contracts.
- Keep PegasusApi abstraction types within the application integration boundary.
- Map PegasusApi responses to Taurus-owned application models before returning data to the Web layer.
- Include only fields required by Taurus when mapping external API models to application models.
- Keep API endpoint paths in integration code rather than environment configuration where the path is part of the stable API contract.

## Code-behind

- Larger pages and layout components use Razor code-behind files.
- Keep Razor files declarative.
- Place interaction logic in the corresponding `.razor.cs` file.
- Simple components without behaviour may remain as a single Razor file.

## Feature Listings

- Use MudBlazor tables for tabular feature listings.
- Keep listing pages focused on presentation and workflow coordination.
- Obtain listing data through the feature's application service rather than defining data directly in the UI.
- Use `DataLabel` on table cells to preserve field context in responsive table layouts.
- Use `MudTableSortLabel` for sortable columns.
- Configure an explicit initial sort where the feature has a natural default ordering.

## Responsive Layout

- Use `MudBreakpointProvider` to adapt the shared application shell to the current viewport.
- Use a persistent navigation drawer on desktop layouts.
- Use a temporary overlay drawer on tablet and mobile layouts.
- Keep the navigation drawer open by default on desktop and closed by default on smaller viewports.
- Control drawer state from `MainLayout`.
- Use the application bar menu button as the single mechanism for opening and closing the navigation drawer across all viewport sizes.
- Keep responsive behaviour within the shared layout rather than individual feature pages wherever practical.

## Authentication Configuration

- Configure authentication in `Program.cs`.
- Use ASP.NET Core Cookie authentication as the application authentication scheme.
- Use OpenID Connect as the default challenge scheme.
- Configure OpenID Connect explicitly rather than relying on framework defaults.
- Request the required Soteria scopes explicitly.
- Configure the authentication middleware before the authorization middleware.
- Place primary authentication actions in the shared application app bar.
- Use `AuthorizeView` to switch between anonymous and authenticated app bar content.
- Initiate OpenID Connect authentication through a dedicated server endpoint rather than directly from an interactive Blazor component.
- Use normal browser navigation for authentication endpoints so the OpenID Connect handler can modify the HTTP response.
- Global authorization is enforced using a fallback policy.
- Explicitly allow anonymous access to infrastructure that must operate before authentication, such as authentication endpoints, status pages and static assets.

## Local Development Configuration

- Store developer-specific configuration in a local `.env` file.
- Load the `.env` file only when running in the Development environment.
- Use `Env.NoClobber().TraversePath().Load()` so existing environment variables take precedence.
- Use standard ASP.NET Core environment variables for production deployments.
- Commit a `.env.example` file containing placeholder values for all required settings.