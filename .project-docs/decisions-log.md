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

