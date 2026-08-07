This document records significant architectural decisions made during the lifetime of the 
project together with the reasoning behind them. It serves as a historical record to help 
future developers understand why important changes were made and to avoid revisiting previously 
resolved discussions.

2026-08-13

Taurus is an authentication-first application.

Application functionality requires an authenticated user by default. Anonymous access is limited to infrastructure required to support authentication, authentication failure handling, status pages and static assets.

A global authorization fallback policy enforces authenticated access so that new application functionality is protected by default rather than requiring each feature to opt into authorization individually.

