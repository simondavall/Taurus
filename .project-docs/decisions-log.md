This document records significant architectural decisions made during the lifetime of the 
project together with the reasoning behind them. It serves as a historical record to help 
future developers understand why important changes were made and to avoid revisiting previously 
resolved discussions.

2026-08-13

### Taurus is an authentication-first application.

Application functionality requires an authenticated user by default. Anonymous access is limited to infrastructure required to support authentication, authentication failure handling, status pages and static assets.

A global authorization fallback policy enforces authenticated access so that new application functionality is protected by default rather than requiring each feature to opt into authorization individually.

2026-08-10

### Use X.509 certificates to protect persisted Data Protection keys

#### Decision

Taurus persists ASP.NET Core Data Protection keys to a host-configured filesystem location and protects the persisted keys at rest using an X.509 certificate loaded from a PKCS#12/PFX file.

#### Rationale

- Data Protection keys must survive application restarts and IIS application-pool recycling.
- Persisted key material must be protected at rest.
- Windows DPAPI was rejected because it would introduce an unnecessary Windows-specific dependency.
- X.509 certificate protection provides the required key-at-rest protection while remaining compatible with future non-Windows hosting.
- Key paths, certificate paths and certificate credentials remain hosting concerns rather than application-specific filesystem assumptions.

2026-08-10

### Use protected browser local storage for persistent UI state

#### Decision

Taurus stores lightweight user preferences and persistent application context using ASP.NET Core Protected Local Storage.

Persisted state is scoped to the browser profile rather than explicitly to the authenticated user.

#### Rationale

- Interactive Server components need to persist UI state after the initial HTTP response has completed.
- Browser local storage naturally survives application restarts and browser sessions.
- Taurus already has explicit persistent ASP.NET Core Data Protection configuration, which can protect values stored through Protected Local Storage.
- The application is intended primarily for local use, so browser-profile scoping is sufficient and avoids unnecessary user-specific persistence infrastructure.
- A Taurus database is not justified for lightweight UI context and preferences.
- The Protected Browser Storage API is currently experimental, but its limitations are acceptable for the application's intended deployment and can be revisited if requirements change.

2026-08-13

### Use stable reference-data codes for ticket semantics

#### Decision

Ticket status, priority and type remain reference data owned by PegasusApi.

PegasusApi provides a stable machine-readable code for each lookup value alongside its numeric identifier, display title and display order.

Taurus maps this reference data into Taurus-owned models and resolves semantic application behaviour using the stable codes rather than numeric database identifiers or display titles.

Predefined ticket filters remain Taurus-owned application concepts.

#### Rationale

- Numeric lookup identifiers are persistence details and should not encode application semantics.
- Display titles are presentation values and may change independently of application behaviour.
- Stable codes allow PegasusApi lookup identifiers and titles to change without requiring corresponding Taurus behaviour changes.
- PegasusApi remains the system of record for ticket reference data.
- Taurus can define application-specific concepts such as Open Tickets and High Priority without pushing those UI concepts into PegasusApi.

2026-08-15

### Sanitise user-authored HTML at the presentation boundary

#### Decision

Taurus sanitises user-authored HTML before rendering it as markup.

The original content remains unchanged when retrieved from or persisted to PegasusApi. Sanitisation is applied only to HTML being rendered by Taurus.

HTML sanitisation is exposed through a Taurus-owned shared abstraction rather than consumed directly from feature code.

#### Rationale

- Existing Pegasus data may contain arbitrary historic HTML and malformed markup.
- User-authored HTML must not be rendered directly because it could introduce cross-site scripting or other unsafe markup.
- Preserving the original content avoids destructive migration or silent modification of PegasusApi data.
- Sanitising only at the rendering boundary allows raw markup to remain editable.
- HTML rendering is expected to be required by multiple Taurus features, so a shared application-owned sanitisation policy provides a single security boundary and avoids feature-specific sanitisation behaviour.

2026-08-15

### Use Markdown for ticket comment content

#### Decision

Ticket comments use Markdown as their canonical authoring and storage format.

Taurus converts stored comment Markdown to HTML for presentation. The generated HTML is passed through the existing Taurus-owned HTML sanitisation boundary before being rendered.

Comment editing and creation continue to use plain-text textareas so users work directly with the stored Markdown.

Existing historic comments will not receive an automated content migration. The limited incompatible historic HTML content will be corrected manually.

#### Rationale

- Markdown provides the required links, lists and text formatting without requiring users to author HTML directly.
- The existing plain-text comment editor already provides a suitable Markdown authoring experience.
- Markdown-to-HTML conversion can be introduced at the presentation boundary without changing the PegasusApi comment contracts.
- Retaining HTML sanitisation after Markdown conversion preserves the established security boundary for rendered user-authored content.
- Historic comments contain sufficiently little HTML that manual correction is preferable to introducing automated migration complexity.

2026-09-05

### Structure Taurus as three projects while preserving Vertical Slice Architecture

#### Decision

Taurus is structured as three physical projects:

- `Taurus.Web`
- `Taurus.Application`
- `Taurus.Infrastructure`

Vertical Slice Architecture is preserved within each project.

`Taurus.Web` is the application host and presentation layer, `Taurus.Application` owns Taurus application contracts and models, and `Taurus.Infrastructure` owns PegasusApi and other external infrastructure implementations.

`Taurus.Application` does not depend on Web or Infrastructure. `PegasusApi.Abstractions` is restricted to Infrastructure.

#### Rationale

- The completed Project and Ticket implementations demonstrated stable responsibility boundaries that justified project-level separation.
- Physical projects allow important dependency rules to be enforced by the compiler rather than convention alone.
- Separating PegasusApi implementations prevents external transport concerns from leaking into Taurus application contracts.
- Preserving vertical slicing keeps feature ownership explicit and avoids returning to horizontal organisation by technical type.
- The structure provides a natural boundary for future infrastructure concerns such as caching.
- Additional architectural layers and abstractions remain unnecessary until demonstrated by implementation needs.

2026-09-09

### Coordinate caching in Application while keeping cache and data implementations independent

#### Decision

Taurus coordinates cached data access through Application services.

Application owns caching policy, including cache keys, expiration and invalidation behaviour. It depends on small Application-owned abstractions for cache access and underlying data retrieval.

Infrastructure provides the concrete cache implementation and external data-provider implementations.

The initial cache provider uses process-local memory caching with configurable absolute expiration. Cache entries are explicitly invalidated after successful mutations where required to prevent Taurus knowingly returning stale data.

#### Rationale

- Lookup caching demonstrated the benefit of separating cache orchestration from PegasusApi retrieval.
- Project-list caching demonstrated the need for explicit invalidation following successful create, update and delete operations.
- Keeping cache policy in Application makes caching part of explicit application behaviour rather than an incidental concern embedded in external data access.
- Keeping the cache provider and data provider independent gives each implementation a single responsibility.
- Application-owned abstractions preserve the dependency direction between Application and Infrastructure.
- Web consumers remain unaware of whether data is cached or how it is retrieved.
- The cache implementation can be replaced by an external caching service without changing application workflows.
- The caching abstraction has grown only in response to demonstrated requirements: cache retrieval was introduced for lookup caching and explicit removal was added when project-list caching required invalidation.

2026-09-09

### Use a validated Taurus-owned application settings authority

#### Decision

Taurus represents application configuration through an immutable `TaurusSettings` model owned by `Taurus.Application`.

The Web composition root loads the configured ASP.NET Core configuration sources and creates `TaurusSettings` during startup.

Creation validates all represented settings and prevents application startup if any are missing or invalid. Validation failures are collected and reported together through a startup exception.

After successful creation, Web, Application and Infrastructure consume the validated settings model rather than independently reading raw configuration for migrated settings.

Settings will be migrated into the authority incrementally as existing configuration usage is reviewed.

#### Rationale

- Application code should be able to rely on required configuration being complete and valid.
- Central validation removes repeated null handling, parsing and fallback behaviour from individual consumers.
- Strongly typed settings make configuration dependencies explicit.
- Owning the settings model in Application allows both Web and Infrastructure to consume it without introducing an invalid project dependency.
- Passing the settings authority avoids increasingly large dependency-registration signatures containing individual configuration values.
- Constructing the authority in Web preserves configuration providers and startup as host responsibilities.
- Keeping `IConfiguration` at the composition boundary prevents lower layers from independently interpreting the same configuration.
- Incremental migration allows the pattern to be adopted without expanding the current task into an application-wide configuration refactoring.
