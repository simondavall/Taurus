# Taurus Site Publishing

This document records the steps required to create and publish the Taurus Production site to IIS from a clean machine.

## Prerequisites

* Install IIS.
* Install the .NET 10 Hosting Bundle.
* Ensure Soteria is available at `https://soteria.local`.
* Ensure PegasusApi is available at `https://pegasusapi.local`.
* Ensure the Taurus source code can be built successfully.
* Ensure NuGet can resolve all required packages, including `PegasusApi.Abstractions`.

## 1. Configure Production application settings

Create or update `appsettings.Production.json`:

```json
{
  "Authentication": {
    "OpenIdConnect": {
      "Authority": "https://soteria.local"
    }
  },
  "PegasusApi": {
    "BaseAddress": "https://pegasusapi.local"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

Do not store the Production OpenID Connect client secret in source control.

## 2. Create the Production Soteria client

Create a Soteria client registration for Taurus.

Use:

```text
Client: Taurus
```

Configure the redirect URI:

```text
https://taurus.local/signin-oidc
```

Configure the post-logout redirect URI:

```text
https://taurus.local/signout-callback-oidc
```

Configure the required scopes:

```text
openid
profile
email
offline_access
reference_api
```

Record the Production client ID and client secret.

Keep the Development client as a separate Soteria registration, for example:

```text
TaurusDev
```

## 3. Configure local name resolution

Edit:

```text
C:\Windows\System32\drivers\etc\hosts
```

Add:

```text
127.0.0.1 taurus.local
```

If IIS is hosted on another machine, use the IIS server IP instead of `127.0.0.1`.

## 4. Create the self-signed certificate

Run PowerShell as Administrator.

Create the certificate:

```powershell
$cert = New-SelfSignedCertificate `
    -DnsName "taurus.local" `
    -CertStoreLocation "cert:\LocalMachine\My" `
    -FriendlyName "Taurus Https" `
    -NotAfter (Get-Date).AddYears(01)
```

Export the public certificate:

```powershell
Export-Certificate `
    -Cert $cert `
    -FilePath "$env:TEMP\taurus.local.cer"
```

Trust the certificate:

```powershell
Import-Certificate `
    -FilePath "$env:TEMP\taurus.local.cer" `
    -CertStoreLocation "cert:\LocalMachine\Root"
```

Remove the temporary certificate file:

```powershell
Remove-Item "$env:TEMP\taurus.local.cer"
```

## 5. Publish Taurus

From the Taurus project directory run:

```powershell
dotnet publish -c Release -o C:\inetpub\wwwroot\Taurus
```

Verify the published folder exists:

```text
C:\inetpub\wwwroot\Taurus
```

Verify the publish output contains:

```text
web.config
```

Do not manually maintain `web.config`; allow the ASP.NET Core publish process to generate it.

## 6. Create the IIS application pool

Create a new IIS application pool:

```text
Name: Taurus
.NET CLR Version: No Managed Code
Managed Pipeline Mode: Integrated
Identity: ApplicationPoolIdentity
```

## 7. Configure Production environment variables

Configure the following environment variables for the Taurus IIS application pool:

```text
ASPNETCORE_ENVIRONMENT=Production
Authentication__OpenIdConnect__ClientId=<Taurus Production ClientId>
Authentication__OpenIdConnect__ClientSecret=<Taurus Production ClientSecret>
```

Recycle the Taurus application pool after changing environment variables.

## 8. Configure folder permissions

Ensure the Taurus application-pool identity has read and execute access to:

```text
C:\inetpub\wwwroot\Taurus
```

The application does not currently require general write access to the application folder.

Additional write permission may be required later for Production log files.

## 9. Create the IIS website

Create a new IIS website:

```text
Site name: Taurus
Application pool: Taurus
Physical path: C:\inetpub\wwwroot\Taurus
```

Configure the HTTPS binding:

```text
Type: https
IP address: All Unassigned
Port: 443
Host name: taurus.local
Require Server Name Indication: Enabled
SSL certificate: Taurus Https
```

If another IIS site already uses port 443 with a different certificate, ensure hostname-specific SNI bindings are used.

## 10. Start the site

Start:

```text
Taurus application pool
Taurus website
```

Browse to:

```text
https://taurus.local
```

## 11. Verify the deployment

Confirm:

* Taurus starts successfully under IIS.
* `https://taurus.local` resolves correctly.
* The browser trusts the Taurus certificate.
* An unauthenticated user is redirected to Soteria.
* Authentication succeeds using the Production Taurus client.
* Authentication returns to `https://taurus.local`.
* Deep links return to the originally requested Taurus page after authentication.
* The authenticated user name is displayed.
* Logout terminates the Taurus and Soteria sessions.
* The Projects page loads data from `https://pegasusapi.local`.
* Active only and Include deleted project options work.
* Static assets load correctly.
* MudBlazor styling is applied.
* Access Denied displays correctly.
* Error displays correctly.
* Not Found displays correctly.
* Recycling the Taurus application pool allows Taurus to start successfully again.

## 12. Republish after application changes

Stop or recycle the Taurus application pool if required.

Publish again:

```powershell
dotnet publish -c Release -o C:\inetpub\wwwroot\Taurus
```

Recycle the Taurus application pool.

Verify:

```text
https://taurus.local
```
