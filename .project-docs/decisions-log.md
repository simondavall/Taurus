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
