using System.Security.Cryptography.X509Certificates;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MudBlazor.Services;
using Serilog;
using Taurus.Application;
using Taurus.Components;
using Taurus.Components.Features.Shared;
using Taurus.Infrastructure;
using Taurus.UserState;

var builder = WebApplication.CreateBuilder(args);

var isLocalExecution = string.Equals(Environment.GetEnvironmentVariable("TAURUS_LOCAL_EXECUTION"), "true", StringComparison.OrdinalIgnoreCase);

if (isLocalExecution) {
    Env.NoClobber()
        .TraversePath()
        .Load();

    builder.Configuration.AddEnvironmentVariables();

    builder.WebHost.UseStaticWebAssets();
}

builder.Services.AddSerilog((services, configuration) => {
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

ValidateRequiredConfiguration(builder.Configuration);

ConfigureDataProtection(builder.Services, builder.Configuration);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddAuthentication(options => {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options => {
        var configuration = builder.Configuration.GetSection("OpenIdConnect");

        options.Authority = configuration["Authority"];
        options.ClientId = configuration["ClientId"];
        options.ClientSecret = configuration["ClientSecret"];

        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.UsePkce = true;
        options.SaveTokens = true;
        options.MapInboundClaims = false;

        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "role";

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.Scope.Add("offline_access");
        options.Scope.Add("reference_api");

        options.Events.OnRemoteFailure = context => {
            var error = context.Failure?.Data["error"]?.ToString();

            var reason = error switch {
                "access_denied" => "unauthorized",
                "unauthorized_client" => "disabled",
                _ => null
            };

            if (reason is not null) {
                context.Response.Redirect($"/access-denied?reason={reason}");
                context.HandleResponse();
            }

            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options => { options.FallbackPolicy = options.DefaultPolicy; });
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddMudServices();

builder.Services.AddTaurusApplication();
builder.Services.AddTaurusInfrastructure(builder.Configuration);

builder.Services.AddScoped<INavigationHistoryService, NavigationHistoryService>();
builder.Services.AddScoped<IUserStateService, UserStateService>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets()
    .Add(endpointBuilder =>
        endpointBuilder.Metadata.Add(new AllowAnonymousAttribute()));

app.MapGet("/authentication/login", (string? returnUrl) => {
    var properties = new AuthenticationProperties {
        RedirectUri = IsLocalReturnUrl(returnUrl) ? returnUrl! : "/"
    };

    return Results.Challenge(properties,
    [
        OpenIdConnectDefaults.AuthenticationScheme
    ]);
}).AllowAnonymous();

app.MapGet("/authentication/logout", () => {
    var properties = new AuthenticationProperties {
        RedirectUri = "/"
    };

    return Results.SignOut(properties,
    [
        CookieAuthenticationDefaults.AuthenticationScheme,
        OpenIdConnectDefaults.AuthenticationScheme
    ]);
}).AllowAnonymous();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

return;

static void ValidateRequiredConfiguration(IConfiguration configuration)
{
    string[] keys = [
        "OpenIdConnect:Authority",
        "OpenIdConnect:ClientId",
        "OpenIdConnect:ClientSecret",
        "PegasusApi:BaseAddress",
        "DataProtection:KeysPath",
        "DataProtection:CertificatePath",
        "DataProtection:CertificatePassword"
    ];

    var missing = keys
        .Where(key => string.IsNullOrWhiteSpace(configuration[key]))
        .ToArray();

    if (missing.Length == 0)
        return;

    throw new InvalidOperationException(
        "Missing required configuration values:" + Environment.NewLine +
        string.Join(Environment.NewLine, missing.Select(key => $" - {key}")));
}

static bool IsLocalReturnUrl(string? returnUrl)
{
    if (string.IsNullOrWhiteSpace(returnUrl))
        return false;

    return returnUrl.StartsWith('/')
           && !returnUrl.StartsWith("//")
           && !returnUrl.StartsWith("/\\");
}

static void ConfigureDataProtection(IServiceCollection services, IConfiguration configuration)
{
    var keysPath = configuration["DataProtection:KeysPath"];
    var certificatePath = configuration["DataProtection:CertificatePath"];
    var certificatePassword = configuration["DataProtection:CertificatePassword"];

    var certificate = X509CertificateLoader.LoadPkcs12FromFile(
        certificatePath!,
        certificatePassword,
        X509KeyStorageFlags.EphemeralKeySet);

    services
        .AddDataProtection()
        .SetApplicationName("Taurus")
        .PersistKeysToFileSystem(new DirectoryInfo(keysPath!))
        .ProtectKeysWithCertificate(certificate);
}