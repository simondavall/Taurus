This document records proven implementation patterns that have emerged during development. Unlike decisions.md, which defines project rules, this document provides practical examples of how common features are implemented. New features should follow these patterns where appropriate to maintain consistency across the application.

## Application Theme

- Define the application theme in a dedicated `TaurusTheme` class.
- Expose the theme through a static `Default` property.
- Apply the theme globally using the root `MudThemeProvider`.
- Use MudBlazor Material icons directly where required.
- Introduce semantic application icon identifiers only if repeated usage demonstrates a useful shared vocabulary.
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
- Pass feature-specific API query options explicitly through the application service contract.
- Construct PegasusApi query strings within the integration boundary rather than the Web layer.
- Keep query-string parameter names aligned with the published PegasusApi contract.
- Introduce shared query-building abstractions only after repeated integrations demonstrate a common requirement.
- Represent expected operation outcomes using Taurus-owned application result types.
- Use `ApplicationResult` for operations that return success or failure without a value.
- Use `ApplicationResult<T>` for operations that return a value on success.
- Interpret published PegasusApi validation and expected failure responses within the integration boundary.
- Return Taurus-owned failure information to the Web layer rather than PegasusApi failure types.
- Treat expected API failures as application results and unexpected integration failures as exceptions.
- Continue logging unexpected HTTP, network and deserialization failures before allowing them to propagate.
- Keep operation-specific decisions about expected HTTP status codes explicit within the owning integration operation.

## Code-behind

- Larger pages and layout components use Razor code-behind files.
- Keep Razor files declarative.
- Place interaction logic in the corresponding `.razor.cs` file.
- Simple components without behaviour may remain as a single Razor file.

## Interactive Editor Validation

- Use `MudForm` for interactive MudBlazor editor forms.
- Use FluentValidation validators for editor-model validation.
- Implement the shared `IMudValidator<T>` interface on validators used by MudBlazor editor forms.
- Pass the editor model to `MudForm.Model`.
- Pass `IMudValidator<T>.ValidateValueAsync` to `MudForm.Validation`.
- Associate validated MudBlazor fields with their model properties using `For`.
- Implement `ValidateValueAsync` by validating only the requested property through FluentValidation `IncludeProperties`.
- Before persistence, explicitly call `MudForm.ValidateAsync()` and continue only when `MudForm.IsValid`.
- Allow MudBlazor to present field-level validation errors rather than manually mapping FluentValidation errors into component-specific error state.
- Keep validation rules in the feature-owned FluentValidation validator rather than in the Razor component or code-behind.
- Keep validation rules specific to each editor; sharing the MudBlazor integration mechanism does not require sharing or generalising individual validation rules.
- Do not introduce a common validator base class solely to remove the small `IMudValidator<T>` integration implementation from individual validators.

## Feature Listings

- Use MudBlazor tables for tabular feature listings.
- Keep listing pages focused on presentation and workflow coordination.
- Obtain listing data through the feature's application service rather than defining data directly in the UI.
- Use `DataLabel` on table cells to preserve field context in responsive table layouts.
- Use `MudTableSortLabel` for sortable columns.
- Configure an explicit initial sort where the feature has a natural default ordering.

## Ticket State Presentation

- Resolve ticket behaviour from stable PegasusApi reference-data codes rather than numeric lookup identifiers or display titles.
- Use `TicketReferenceIds` to resolve the known ticket status and priority codes into the current PegasusApi identifiers.
- Keep the complete set of Taurus-owned ticket reference semantics together in `TicketReferenceIds`, even when an individual consumer uses only part of the set.
- Use `TicketPresentation` for deterministic ticket presentation rules shared across multiple ticket views.
- Use the shared age presentation for ticket listings, comments and sub-task listings.
- Treat Completed and Obsolete tickets as inactive for presentation while preserving normal navigation and interaction.
- Use `TicketStateIndicators` wherever the standard ticket priority and status indicators are required.
- Display High and Critical priority using the established lightning indicator with fixed high-contrast amber and red colours.
- Display In Progress and On Hold using the established status indicators with theme-based colours.
- Allow priority and status indicators to appear simultaneously.
- Keep the surrounding ticket-row layout within the owning presentation rather than introducing a generic ticket-row component.
- Extract shared ticket presentation only where repeated implementations demonstrate identical behaviour; retain workflow and layout differences in their owning pages.

## Responsive Layout

- Use `MudBreakpointProvider` to adapt the shared application shell to the current viewport.
- Use a persistent navigation drawer on desktop layouts.
- Use a temporary overlay drawer on tablet and mobile layouts.
- Keep the navigation drawer open by default on desktop and closed by default on smaller viewports.
- Allow feature pages to provide dedicated responsive presentations where the desktop presentation does not adapt effectively to smaller viewports.
- Keep feature-specific responsive presentations within the owning feature rather than introducing shared responsive abstractions without demonstrated reuse.
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

## Application Logging

- Use Serilog as the application logging provider.
- Use `ILogger<T>` within Taurus application code rather than depending directly on Serilog APIs.
- Use structured message templates with named properties rather than interpolated log messages.
- Configure logging sinks through environment-specific application configuration.
- Log to the console during Development.
- Log to daily rolling files in Production.
- Store Production Taurus logs under `C:\inetpub\logs\Taurus`.
- Retain Production log files according to the configured retention policy.
- Log useful operation context while avoiding credentials, tokens and other sensitive information.
- Log integration failures with the associated exception before allowing unhandled failures to propagate.

## Local Environment Execution

- Distinguish local execution from the ASP.NET Core environment name.
- Use an explicit Taurus local-execution environment variable when running non-Development environments locally.
- Load local `.env` configuration only during explicitly identified local execution.
- Do not load `.env` files in deployed Production environments.
- Use `UseStaticWebAssets()` during local execution outside Development so static web assets remain available.
- Use IIS application-pool environment variables for Production secrets.

## Data Protection

- Configure ASP.NET Core Data Protection explicitly.
- Use the stable application name `Taurus`.
- Persist Data Protection keys to a host-configured filesystem location.
- Protect persisted Data Protection keys at rest using an X.509 certificate.
- Load the Data Protection certificate from a host-configured PKCS#12/PFX file.
- Keep the key-ring path, certificate path and certificate password outside source-controlled application configuration.
- Use separate Data Protection certificates and key rings for local and Production environments.
- Do not depend on Windows DPAPI or the Windows certificate store for application Data Protection.
- Grant the application write access only to the Data Protection key-ring directory.
- Grant the application read access only to the Data Protection certificate location.
- Never commit Data Protection key rings, private-key certificates or certificate passwords to source control.

## Persistent Browser State

- Access persistent browser state through Taurus-owned application services rather than directly from feature components.
- Use Protected Local Storage for lightweight user preferences and persistent application context.
- Keep persisted state models incremental and introduce values only when required by implemented features.
- Define an explicit default or fallback for every persisted value.
- Treat missing, invalid or stale persisted state as its defined fallback rather than preventing application use.
- Access protected browser storage only after the application becomes interactive because browser storage is unavailable during static prerendering.

## Reference Data

- Map PegasusApi reference data to Taurus-owned application models at the integration boundary.
- Use stable reference-data codes when Taurus behaviour depends on the semantic meaning of a lookup value.
- Resolve semantic codes to the current lookup IDs before comparing them with foreign-key identifiers on application models.
- Do not hard-code PegasusApi lookup IDs or use display titles to determine application behaviour.
- Keep Taurus-owned predefined filters separate from PegasusApi reference-data models.
- Apply client-side ticket filtering before ordering and pagination.