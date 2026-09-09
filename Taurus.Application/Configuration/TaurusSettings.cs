using Microsoft.Extensions.Configuration;

namespace Taurus.Application.Configuration;

public sealed record TaurusSettings(
    OpenIdConnectSettings OpenIdConnect,
    PegasusApiSettings PegasusApi,
    DataProtectionSettings DataProtection,
    ProjectSettings Projects,
    TicketSettings Tickets,
    CachingSettings Caching)
{
    public static TaurusSettings Create(IConfiguration configuration)
    {
        var errors = new List<string>();

        var openIdConnectAuthority = GetAbsoluteUri(configuration, "OpenIdConnect:Authority", errors);
        var openIdConnectClientId = GetRequiredString(configuration, "OpenIdConnect:ClientId", errors);
        var openIdConnectClientSecret = GetRequiredString(configuration, "OpenIdConnect:ClientSecret", errors);

        var pegasusApiBaseAddress = GetAbsoluteUri(configuration, "PegasusApi:BaseAddress", errors);

        var dataProtectionKeysPath = GetRequiredString(configuration, "DataProtection:KeysPath", errors);
        var dataProtectionCertificatePath = GetRequiredString(configuration, "DataProtection:CertificatePath", errors);
        var dataProtectionCertificatePassword = GetRequiredString(configuration, "DataProtection:CertificatePassword", errors);

        var projectPageSize = GetPositiveInt(configuration, "Projects:PageSize", errors);
        var ticketPageSize = GetPositiveInt(configuration, "Tickets:PageSize", errors);

        var ticketLookupCacheDurationMinutes = GetPositiveInt(configuration, "Caching:TicketLookups:DurationMinutes", errors);
        var projectCacheDurationMinutes = GetPositiveInt(configuration, "Caching:Projects:DurationMinutes", errors);

        if (errors.Count > 0) {
            throw new InvalidOperationException(
                "Invalid Taurus configuration:"
                + Environment.NewLine
                + string.Join(Environment.NewLine, errors.Select(error => $" - {error}")));
        }

        return new TaurusSettings(
            new OpenIdConnectSettings(openIdConnectAuthority!, openIdConnectClientId!, openIdConnectClientSecret!),
            new PegasusApiSettings(pegasusApiBaseAddress!),
            new DataProtectionSettings(dataProtectionKeysPath!, dataProtectionCertificatePath!, dataProtectionCertificatePassword!),
            new ProjectSettings(projectPageSize!.Value),
            new TicketSettings(ticketPageSize!.Value),
            new CachingSettings(
                new CacheSettings(TimeSpan.FromMinutes(ticketLookupCacheDurationMinutes!.Value)),
                new CacheSettings(TimeSpan.FromMinutes(projectCacheDurationMinutes!.Value))));
    }

    private static string? GetRequiredString(IConfiguration configuration, string key, ICollection<string> errors)
    {
        var value = configuration[key];

        if (!string.IsNullOrWhiteSpace(value))
            return value;

        errors.Add($"{key} is required.");
        return null;
    }

    private static Uri? GetAbsoluteUri(IConfiguration configuration, string key, ICollection<string> errors)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value)) {
            errors.Add($"{key} is required.");
            return null;
        }

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return uri;

        errors.Add($"{key} must be a valid absolute URI.");
        return null;
    }

    private static int? GetPositiveInt(IConfiguration configuration, string key, ICollection<string> errors)
    {
        var value = configuration[key];

        if (string.IsNullOrWhiteSpace(value)) {
            errors.Add($"{key} is required.");
            return null;
        }

        if (int.TryParse(value, out var parsedValue) && parsedValue > 0)
            return parsedValue;

        errors.Add($"{key} must be a positive integer.");
        return null;
    }
}

public sealed record OpenIdConnectSettings(
    Uri Authority,
    string ClientId,
    string ClientSecret);

public sealed record PegasusApiSettings(
    Uri BaseAddress);

public sealed record DataProtectionSettings(
    string KeysPath,
    string CertificatePath,
    string CertificatePassword);

public sealed record ProjectSettings(
    int PageSize);

public sealed record TicketSettings(
    int PageSize);

public sealed record CachingSettings(
    CacheSettings TicketLookups,
    CacheSettings Projects);

public sealed record CacheSettings(
    TimeSpan Duration);