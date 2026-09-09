# Architecture overview

Taurus is a .NET 10 Blazor Web App that replaces the existing PegasusUI application.

The application is responsible for presenting and coordinating the Pegasus user experience. PegasusApi is the system of record for project and ticket data.

Taurus uses Interactive Server rendering. UI interactions execute on the Taurus server, and the browser maintains an interactive Blazor connection to the application.

The initial architecture is intentionally limited to the boundaries required for the first implementation. Additional structure and abstractions should emerge from proven implementation needs rather than being defined speculatively.

## System context

Taurus interacts with two existing systems:

- PegasusApi supplies project and ticket data.
- Soteria provides user authentication.

The initial system flow is:

```text
User
  |
  | Uses the application through a browser
  v
Taurus
  |
  | Authenticates through OpenID Connect
  v
Soteria

Taurus
  |
  | Sends JSON API requests using the PegasusApi contract
  v
PegasusApi
```

Taurus maintains its own authenticated application session after authentication through Soteria.

PegasusApi currently requires no authentication. It is a locally hosted third party api.

## Application responsibilities

Taurus owns:

- User interface presentation.
- Responsive layout and interaction.
- Navigation.
- Application workflow coordination.
- Authentication through Soteria.
- Access to the authenticated user's identity.
- User settings and preferences.
- Communication with PegasusApi.
- Mapping between Taurus models and PegasusApi contracts.
- Presentation of API validation and failure responses.

Taurus does not own:

- Project or ticket persistence.
- Pegasus business data.
- PegasusApi request or response contracts.
- User identity management.

## Rendering model

Taurus uses global Interactive Server rendering.

This model keeps application execution and PegasusApi communication on the server while allowing the UI to behave as an interactive web application.

The browser does not call PegasusApi directly.

This avoids coupling browser code to the API, avoids introducing browser-side API token handling, and allows future access tokens to remain within the Taurus server application.

## Application structure

Taurus is structured as three projects with explicit dependency boundaries while preserving Vertical Slice Architecture within each project.

```text
Taurus.Web
  ├── Taurus.Application
  └── Taurus.Infrastructure
        └── Taurus.Application
```
Vertical feature slicing is retained within these project boundaries. The projects define responsibility and dependency boundaries; they do not replace feature-oriented organisation with horizontal technical layers.

### Taurus.Web

The Web project is the application host and composition root.

It owns:

- Blazor pages and Razor components.
- MudBlazor presentation.
- Form and editor models.
- UI validation.
- Navigation.
- Interactive UI state.
- Protected browser state.
- Responsive behaviour.
- Authentication and application hosting.
- Display of validation and operation failures.

The Web project consumes Taurus application services and models.

The Web project must not consume PegasusApi request or response models directly.

### Taurus.Application

The Application project defines Taurus-owned application contracts, models and application behaviour.

It owns:

- Taurus request and response models.
- Application service interfaces.
- Taurus-owned result types.
- Application workflow and orchestration that is independent of concrete infrastructure implementations.
- Infrastructure capability abstractions required by application workflows, such as caching.
- Shared content-processing abstractions such as Markdown rendering and HTML sanitisation.

Application services may coordinate multiple infrastructure capabilities where this represents genuine application behaviour. For example, a service may coordinate cached access to an underlying data provider while remaining independent of the concrete cache and data-source implementations.

The Application project does not depend on the Web or Infrastructure projects.

The Application project must not depend on MudBlazor, Blazor presentation infrastructure, concrete caching implementations or PegasusApi transport models.

### Taurus.Infrastructure

The Infrastructure project implements external and technical capabilities required by Taurus.

It owns:

- PegasusApi HTTP communication.
- PegasusApi data-provider implementations.
- Mapping Taurus requests to PegasusApi requests.
- Mapping PegasusApi responses to Taurus responses.
- Interpreting PegasusApi validation and failure responses.
- Concrete caching implementations.
- Infrastructure-specific dependency registration.

PegasusApi.Abstractions is referenced only by the Infrastructure project and terminates at this boundary.

Where Application coordinates infrastructure-independent behaviour, Infrastructure implements the required Application-owned capability abstractions rather than owning the orchestration itself.

Caching follows this model: Application owns cache policy and coordinates cached data access, while Infrastructure provides the concrete cache implementation and external data provider.

### Dependency direction

The allowed project dependencies are:

```
Taurus.Web
├── Taurus.Application
└── Taurus.Infrastructure
└── Taurus.Application
```
Taurus.Application must remain independent of both Taurus.Web and Taurus.Infrastructure.

## API integration

PegasusApi is treated as an immutable external system.

Taurus references the PegasusApi abstraction package only within its integration boundary.

A typical request flow is:

```
Blazor page or component
  |
  | Taurus request model
  v
Taurus application service
  |
  | Maps to PegasusApi request
  v
PegasusApi client
  |
  | JSON API request
  v
PegasusApi
  |
  | PegasusApi response
  v
Taurus application service
  |
  | Interprets and maps response
  v
Taurus response model
  |
  v
Blazor page or component
```

The UI must not depend directly on PegasusApi transport models.

This allows Taurus to:

- Keep presentation models focused on UI requirements.
- Isolate changes in the external API contract.
- Interpret API validation consistently.
- Avoid leaking transport concerns into components.
- Introduce feature-specific models where different workflows require different representations of the same API entity.


## Authentication

Taurus authenticates users through Soteria.

Taurus acts as an OpenID Connect client and maintains its own local authenticated session after successful sign-in.

The authenticated user identifier is obtained from the Soteria principal and supplied to PegasusApi where the existing API contract requires it, including operations such as:

- Creating tickets.
- Creating comments.
- Recording the user who performed an operation.
- Allocating work to a user.

The current user's identifier must be derived from the authenticated session rather than accepted from editable user input when it represents the acting user.

Until PegasusApi is protected, the supplied user identifier is contextual application data rather than independently verified API identity.

## Persistence

Taurus does not initially have its own database.

Project and ticket data remain owned by PegasusApi.

User preferences and persistent application context are stored in protected browser local storage.

Taurus uses ASP.NET Core Protected Local Storage so persisted browser state is protected using the application's Data Protection configuration.

Persisted browser state is scoped to the browser profile rather than explicitly to the authenticated user.

Every persisted value must have a defined default or fallback behaviour so that Taurus continues to operate when:

- The stored value does not exist.
- Browser storage has been cleared.
- The stored value is incomplete.
- The stored value is invalid.
- The stored value refers to application state that is no longer available.
- New persisted values are introduced after existing state was created.

A Taurus database should only be introduced when a demonstrated requirement cannot be met appropriately through PegasusApi or cookie-based preferences.

## Features

The initial application includes the following functional areas:

- Ticket listing.
- Ticket details and editing.
- Project administration.
- User settings and preferences.
- Authentication and logout.

The ticket-listing feature includes contextual controls for:

- Selecting the current project.
- Selecting predefined ticket filters.
- Pagination.
- Ticket display preferences.

The existing behaviour should be reproduced before new capabilities are introduced.

Subtle workflow and display requirements will be identified and documented as each feature is implemented.

## Responsive design

Taurus is fully responsive.

Desktop layouts may use secondary columns and persistent contextual widgets.

On smaller screens, the same functionality may be presented through alternative responsive controls such as drawers, menus, dialogs or stacked content.

Responsive changes may alter layout and interaction while preserving the underlying behaviour.

The existing PegasusUI layout should not be treated as a fixed visual architecture.

## Feature organisation

Taurus uses Vertical Slice Architecture.

Features own their related:

- Pages.
- Components.
- Models.
- Services.
- Validation.
- API integration behaviour where it is feature-specific.

Shared functionality should be introduced only when it is required by more than one proven implementation.

Shared abstractions must remain small, explicit and focused.

## Error and validation handling

PegasusApi validation and failure responses are interpreted within the integration layer.

The Web layer receives Taurus-owned results and validation information rather than raw PegasusApi failure types.

The exact result abstraction should be established from implemented workflows rather than designed in advance.

The application should distinguish between:

- User-correctable validation failures.
- Business-rule failures.
- Authentication failures.
- Unexpected API responses.
- Network or service availability failures.

The UI should provide clear feedback while preserving sufficient diagnostic information for application logging.

## Hosting

Taurus, Soteria and PegasusApi are hosted as separate applications under IIS.

Each application has an independent responsibility boundary and deployment lifecycle.

Taurus communicates with Soteria through OpenID Connect and with PegasusApi through HTTP JSON API calls.

Detailed production hosting, credential and API-authorisation decisions will be documented when those concerns become current implementation work.

## Develelopment Environment

Rider IDE
Powershell

## Local configuration

Developer-specific secrets are stored in a local `.env` file.

The `.env` file is excluded from source control and loaded during application startup.

A committed `.env.example` file documents the required configuration keys using placeholder values.

Production deployments should supply the same configuration using the hosting environment's standard configuration mechanisms rather than a `.env` file.

## Application settings

Taurus converts its combined ASP.NET Core configuration into a Taurus-owned application settings model during startup.

Configuration providers such as application settings, environment variables and local `.env` values remain hosting concerns. After these sources have been loaded, the Web composition root creates and validates the application settings authority.

The application settings model is owned by `Taurus.Application` so it can be consumed by both Web and Infrastructure without reversing project dependencies.

The startup flow is:

```text
Configuration providers
        |
        v
IConfiguration
        |
        | Read and validate once during startup
        v
TaurusSettings
        |
        +----> Taurus.Web
        |
        +----> Taurus.Application
        |
        +----> Taurus.Infrastructure
```
TaurusSettings provides strongly typed settings grouped by application concern.

All settings represented by TaurusSettings must be valid before application startup can continue. Validation collects configuration failures and reports them together through a startup exception.

Once TaurusSettings has been successfully created, consumers may rely on represented settings being present, parsed and valid without introducing local configuration defaults or repeated validation.

Raw IConfiguration should not be passed into Application or Infrastructure for settings that have been migrated to TaurusSettings.

