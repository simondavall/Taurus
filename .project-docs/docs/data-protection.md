### Title

Configure Persistent Data Protection Keys

### Description

Configure ASP.NET Core Data Protection to persist Taurus key material outside the application process and protect the persisted keys at rest using an X.509 certificate. This removes reliance on ephemeral in-memory keys and ensures protected application data remains valid across application restarts and IIS application-pool recycling.

### Goal

Establish secure, persistent and cross-platform-compatible Data Protection key management for Taurus.

### Scope

* Configure ASP.NET Core Data Protection explicitly.
* Persist Production Data Protection keys to `C:\inetpub\keys\Taurus`.
* Protect persisted Data Protection keys at rest using an X.509 certificate.
* Make the key-storage path configurable rather than hard-coding the Windows path into Taurus.
* Make the certificate configuration host/environment-specific.
* Create and configure the Production X.509 certificate.
* Grant `IIS AppPool\Taurus` only the filesystem permissions required for the key directory.
* Configure a stable Taurus Data Protection application name.
* Verify authentication cookies remain usable across IIS application-pool recycling.
* Verify Data Protection keys persist across application restarts.
* Verify persisted key files are encrypted at rest.
* Verify the existing Data Protection ephemeral-key warnings are eliminated.
* Keep the implementation compatible with future non-Windows hosting by avoiding Windows DPAPI dependencies.

### Implementation

Below is the complete inline implementation. It keeps the key-ring location and certificate details out of source control, uses a PFX file rather than a Windows certificate store, and configures Data Protection explicitly in `Program.cs`. ASP.NET Core supports filesystem persistence plus X.509 protection, and .NET 10 provides `X509CertificateLoader.LoadPkcs12FromFile(...)` for loading the PFX. ([Microsoft Learn][1])

### 1. Update `.env.example`

Add these entries:

```text
DataProtection__KeysPath=<path to data protection keys>
DataProtection__CertificatePath=<path to data protection certificate>
DataProtection__CertificatePassword=<certificate password>
```

Your existing local `.env` should then contain local values for those same keys.

For Production, configure the same three values as Taurus App Pool environment variables.

### 2. Update `Program.cs`

Add these usings:

```csharp
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;
```

After:

```csharp
ValidateRequiredConfiguration(builder.Configuration);
```

add:

```csharp
ConfigureDataProtection(builder.Services, builder.Configuration);
```

Then add these methods at the bottom of `Program.cs`:

```csharp
static void ConfigureDataProtection(IServiceCollection services, IConfiguration configuration)
{
    var keysPath = configuration["DataProtection:KeysPath"]
                   ?? throw new InvalidOperationException("DataProtection:KeysPath is not configured.");

    var certificatePath = configuration["DataProtection:CertificatePath"]
                          ?? throw new InvalidOperationException("DataProtection:CertificatePath is not configured.");

    var certificatePassword = configuration["DataProtection:CertificatePassword"]
                              ?? throw new InvalidOperationException("DataProtection:CertificatePassword is not configured.");

    var certificate = X509CertificateLoader.LoadPkcs12FromFile(
        certificatePath,
        certificatePassword,
        X509KeyStorageFlags.EphemeralKeySet);

    services
        .AddDataProtection()
        .SetApplicationName("Taurus")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
        .ProtectKeysWithCertificate(certificate);
}
```

`SetApplicationName("Taurus")` gives Taurus a stable Data Protection discriminator. `PersistKeysToFileSystem(...)` ensures the key ring survives process/app-pool restarts, while `ProtectKeysWithCertificate(...)` encrypts persisted key material at rest. ([Microsoft Learn][2])

Using `EphemeralKeySet` here applies to the **certificate's private key loading**, not the ASP.NET Data Protection key ring. The Data Protection keys still persist to the configured filesystem path; this simply avoids importing the PFX private key permanently into an OS certificate store.

### 3. Extend startup validation

Update the required configuration list to:

```csharp
string[] keys =
[
    "OpenIdConnect:Authority",
    "OpenIdConnect:ClientId",
    "OpenIdConnect:ClientSecret",
    "PegasusApi:BaseAddress",
    "DataProtection:KeysPath",
    "DataProtection:CertificatePath",
    "DataProtection:CertificatePassword"
];
```

That makes incomplete Data Protection configuration a startup failure rather than silently falling back to ephemeral keys.

### 4. No changes to `appsettings*.json`

I would leave all three JSON files unchanged.

The key path and certificate path are host-specific, and the certificate password is secret. Keeping all three outside source control is consistent with the configuration approach you've now established.

### 5. Create the Production directories

Run PowerShell as Administrator:

```powershell
New-Item -ItemType Directory -Path "C:\inetpub\keys\Taurus" -Force
New-Item -ItemType Directory -Path "C:\inetpub\certificates\Taurus" -Force
```

I recommend keeping the PFX outside the Data Protection key-ring directory. The intended layout is:

```text
C:\inetpub\keys\Taurus
    key-....xml
    key-....xml

C:\inetpub\certificates\Taurus
    taurus-dataprotection.pfx
```

### 6. Create the X.509 certificate

Run PowerShell as Administrator.

Prompt for the PFX password:

```powershell
$password = Read-Host "PFX password" -AsSecureString
```

Create the certificate:

```powershell
$cert = New-SelfSignedCertificate `
    -Type Custom `
    -Subject "CN=Taurus Data Protection" `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -KeyExportPolicy Exportable `
    -KeyUsage KeyEncipherment, DataEncipherment `
    -CertStoreLocation "Cert:\LocalMachine\My" `
    -FriendlyName "Taurus Data Protection" `
    -NotAfter (Get-Date).AddYears(5)
```

Export it as a password-protected PFX:

```powershell
Export-PfxCertificate `
    -Cert $cert `
    -FilePath "C:\inetpub\certificates\Taurus\taurus-dataprotection.pfx" `
    -Password $password
```

ASP.NET Core supports X.509 certificates for Data Protection key encryption at rest, including certificates generated with PowerShell. ([Microsoft Learn][3])

Because Taurus loads the PFX directly from disk, the certificate-store copy is no longer required after export. Once you have verified the PFX, you can remove the temporary store entry:

```powershell
Remove-Item "Cert:\LocalMachine\My\$($cert.Thumbprint)"
```

### 7. Configure Production App Pool environment variables

Set these for the `Taurus` application pool:

```text
DataProtection__KeysPath=C:\inetpub\keys\Taurus
DataProtection__CertificatePath=C:\inetpub\certificates\Taurus\taurus-dataprotection.pfx
DataProtection__CertificatePassword=<PFX password>
```

Then recycle the application pool.

### 8. Configure filesystem permissions

Grant the Taurus App Pool **Modify** access to the key-ring directory:

```powershell
$acl = Get-Acl "C:\inetpub\keys\Taurus"

$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    "IIS AppPool\Taurus",
    "Modify",
    "ContainerInherit,ObjectInherit",
    "None",
    "Allow"
)

$acl.AddAccessRule($rule)

Set-Acl "C:\inetpub\keys\Taurus" $acl
```

The application must create and rotate Data Protection key files there.

For the certificate directory, grant **Read & Execute** only:

```powershell
$acl = Get-Acl "C:\inetpub\certificates\Taurus"

$rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
    "IIS AppPool\Taurus",
    "ReadAndExecute",
    "ContainerInherit,ObjectInherit",
    "None",
    "Allow"
)

$acl.AddAccessRule($rule)

Set-Acl "C:\inetpub\certificates\Taurus" $acl
```

The App Pool does not need permission to modify or replace the PFX.

### 9. Configure local `.env`

For normal Rider execution, use local paths, for example:

```text
DataProtection__KeysPath=C:\Development\Taurus\data-protection\keys
DataProtection__CertificatePath=C:\Development\Taurus\data-protection\taurus-dataprotection.pfx
DataProtection__CertificatePassword=<local PFX password>
```

You can create a separate local certificate using the same PowerShell recipe. I would **not** reuse the Production PFX on a development machine.

Because your existing `TAURUS_LOCAL_EXECUTION` mechanism loads `.env` for local Development and local Production execution, both modes will use the local Data Protection configuration. Your current startup implementation already supports that configuration flow.

### Verification

* [ ] Taurus builds successfully.
* [ ] Taurus starts successfully locally.
* [ ] Startup fails clearly if any required Data Protection setting is missing.
* [ ] A Data Protection key XML file is created in the configured local key directory.
* [ ] Restarting the local application reuses the persisted key ring rather than generating an unrelated ephemeral repository.
* [ ] The persisted XML does not contain unencrypted key material.
* [ ] Production publishes and starts successfully under IIS.
* [ ] `C:\inetpub\keys\Taurus` contains Data Protection key files after startup.
* [ ] `IIS AppPool\Taurus` can modify the key directory.
* [ ] `IIS AppPool\Taurus` can read, but does not require write access to, the PFX directory.
* [ ] The previous “Using an in-memory repository” warning no longer appears.
* [ ] The previous “Neither user profile nor HKLM registry available” warning no longer appears.
* [ ] The previous “No XML encryptor configured” warning no longer appears.
* [ ] Log in to Taurus and leave the authenticated browser session open.
* [ ] Recycle the Taurus IIS application pool.
* [ ] Refresh Taurus in the existing browser session.
* [ ] The existing authentication cookie remains valid and the user remains authenticated.
* [ ] Restart the Taurus site/application pool again and verify the same persisted key ring is reused.
* [ ] Existing authentication, logout, Projects, PegasusApi, Serilog and static-asset behaviour remains unchanged.
* [ ] Taurus remains runnable in both local and IIS Production environments.

The resulting design remains portable: a Linux deployment can use different filesystem paths and provision the same PFX format without changing Taurus code. The host supplies paths and credentials; Taurus only depends on standard filesystem and X.509 APIs. ([Microsoft Learn][4])

[1]: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-10.0&utm_source=chatgpt.com "Configure ASP.NET Core Data Protection"
[2]: https://learn.microsoft.com/en-us/aspnet/core/security/cookie-sharing?view=aspnetcore-10.0&utm_source=chatgpt.com "Share authentication cookies among ASP.NET apps"
[3]: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/implementation/key-encryption-at-rest?view=aspnetcore-10.0&utm_source=chatgpt.com "Key encryption at rest in Windows and Azure using ASP. ..."
[4]: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificateloader.loadpkcs12fromfile?view=net-10.0&utm_source=chatgpt.com "X509CertificateLoader.LoadPkcs12FromFile Method"
