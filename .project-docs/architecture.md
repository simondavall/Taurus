# Architecture overview

Taurus is a .NET 10 Blazor Web App that replaces the existing PegasusUI application.

The application is responsible for presenting and coordinating the Pegasus user experience. PegasusApi remains the system of record for project and ticket data and is treated as an external dependency whose contract cannot be changed.

Taurus uses Interactive Server rendering. UI interactions execute on the Taurus server, and the browser maintains an interactive Blazor connection to the application.

The initial architecture is intentionally limited to the boundaries required for the first implementation. Additional structure and abstractions should emerge from proven implementation needs rather than being defined speculatively.

## System context

Taurus interacts with two existing systems:

- PegasusApi supplies project and ticket data and performs the available business operations.
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

PegasusApi is initially unprotected. API authentication and authorisation will be introduced separately after the initial Taurus UI has been completed.

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
- PegasusApi implementation.
- PegasusApi request or response contracts.
- User identity management.
- API authorisation during the initial implementation.

## Rendering model

Taurus uses global Interactive Server rendering.

This model keeps application execution and PegasusApi communication on the server while allowing the UI to behave as an interactive web application.

The browser does not call PegasusApi directly.

This avoids coupling browser code to the API, avoids introducing browser-side API token handling, and allows future access tokens to remain within the Taurus server application.

## Application structure

Taurus will contain at least two logical layers.

### Web layer

The Web layer owns:

- Blazor pages.
- Razor components.
- MudBlazor presentation.
- Form and editor models.
- Navigation.
- Interactive UI state.
- Responsive behaviour.
- Authentication UI.
- Display of validation and operation failures.

The Web layer consumes Taurus application services and models.

The Web layer must not consume PegasusApi request or response models directly.

### Application and integration layer

The application and integration layer owns:

- Taurus request models.
- Taurus response models.
- Application workflow services.
- PegasusApi HTTP communication.
- Mapping Taurus requests to PegasusApi requests.
- Mapping PegasusApi responses to Taurus responses.
- Interpreting API validation and failure responses.
- Supplying authenticated user information to API operations where required.

PegasusApi abstraction types terminate at this boundary.

The exact class and project structure should evolve as the first features are implemented.

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

User settings and preferences are stored in a browser cookie.

Every stored setting must have a defined default value so that Taurus continues to operate when:

- The cookie does not exist.
- The cookie has been removed.
- The stored value is incomplete.
- The stored value is invalid.
- New settings are introduced after the cookie was created.

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

