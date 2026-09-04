Yes. DPAPI is Windows-specific, so if you want to preserve Taurus's cross-platform portability, I would avoid making it part of the application configuration.

The most straightforward cross-platform option is **X.509 certificate protection**. ASP.NET Core Data Protection can persist its key ring to the filesystem and encrypt those keys at rest with a certificate using `ProtectKeysWithCertificate(...)`. Microsoft documents this as a supported key-at-rest mechanism alongside filesystem persistence. ([Microsoft Learn][1])

Conceptually:

```csharp
builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
    .ProtectKeysWithCertificate(certificate);
```

That works cross-platform as long as the application can access the certificate/private key on the host. The operational detail then becomes how you provision that certificate on Windows versus Linux, rather than changing Taurus's Data Protection design.

Another cross-platform option is an external key-management service such as **Azure Key Vault**, which ASP.NET Core supports for wrapping Data Protection keys. ([Microsoft Learn][2]) That's appropriate for cloud/distributed hosting, but it would be unnecessary infrastructure for your current single-machine IIS deployment.

There is also an important distinction between **persistence** and **encryption at rest**. You could simply persist the Data Protection keys to a protected filesystem directory and rely on OS filesystem permissions. ASP.NET Core supports filesystem key persistence cross-platform. However, once you explicitly choose the filesystem repository, the framework no longer automatically encrypts those key files at rest, and Microsoft recommends adding an explicit encryption mechanism for Production. ([Microsoft Learn][3])

For Taurus, I'd rank the options:

1. **Filesystem + X.509 certificate** — best fit if you want cross-platform application design without adding cloud infrastructure.
2. **Filesystem permissions only** — simplest, but leaves the key material unencrypted on disk.
3. **Azure Key Vault or similar external key service** — strongest portability/distributed-hosting story, but overkill for the current deployment.
4. **DPAPI** — simplest specifically for IIS/Windows, but explicitly ties this part of the configuration to Windows.

Given the project's aim to remain straightforward and avoid unnecessary infrastructure, I'd choose **filesystem persistence plus X.509 certificate protection** if cross-platform portability is a real requirement. It solves the warnings without baking Windows DPAPI into Taurus.

[1]: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-10.0&utm_source=chatgpt.com "Configure ASP.NET Core Data Protection"
[2]: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/extensions.aspnetcore.dataprotection.keys-readme?view=azure-dotnet&utm_source=chatgpt.com "Azure Key Vault Key Encryptor for Microsoft.AspNetCore. ..."
[3]: https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/implementation/key-storage-providers?view=aspnetcore-10.0&utm_source=chatgpt.com "Key storage providers in ASP.NET Core"