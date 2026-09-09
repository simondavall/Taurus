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
using Taurus.Application.Configuration;
using Taurus.Components;
using Taurus.Components.Features.Shared;
using Taurus.Infrastructure;
using Taurus.UserState;

var builder = WebApplication.CreateBuilder(args);

_ = bool.TryParse(Environment.GetEnvironmentVariable("TAURUS_LOCAL_EXECUTION"), out var isLocalExecution);
if (isLocalExecution) {
    Env
        .NoClobber()
        .TraversePath()
        .Load();

    builder.Configuration.AddEnvironmentVariables();

    builder.WebHost.UseStaticWebAssets();
}

var settings = TaurusSettings.Create(builder.Configuration);
builder.Services.AddSingleton(settings);
builder.Services.AddSingleton(settings.Projects);
builder.Services.AddSingleton(settings.Tickets);

builder.Services.AddSerilog((services, configuration) => {
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

ConfigureDataProtection(builder.Services, settings.DataProtection);

builder
    .Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder
    .Services
    .AddAuthentication(options => {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options => {
        options.Authority = settings.OpenIdConnect.Authority.ToString();
        options.ClientId = settings.OpenIdConnect.ClientId;
        options.ClientSecret = settings.OpenIdConnect.ClientSecret;

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

builder.Services.AddAuthorization(options => {
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddMudServices();

builder.Services.AddTaurusApplication();

builder.Services.AddTaurusInfrastructure(settings);

builder.Services.AddScoped<INavigationHistoryService, NavigationHistoryService>();
builder.Services.AddScoped<IUserStateService, UserStateService>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() =>
{
    Log.Information("{Application} is starting up...", app.Environment.ApplicationName);
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
    Log.Information("App version: {Version}", typeof(Program).Assembly.GetName().Version);
});

lifetime.ApplicationStopping.Register(() => {
    Log.Warning("{Application} is shutting down...", app.Environment.ApplicationName);
});

lifetime.ApplicationStopped.Register(() =>
{
    Log.Warning("{Application} has stopped", app.Environment.ApplicationName);
    Log.CloseAndFlush();
});

if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app
    .MapStaticAssets()
    .Add(endpointBuilder => endpointBuilder.Metadata.Add(new AllowAnonymousAttribute()));

app
    .MapGet(
        "/authentication/login",
        (string? returnUrl) => {
            var properties = new AuthenticationProperties { RedirectUri = IsLocalReturnUrl(returnUrl) ? returnUrl! : "/" };
            return Results.Challenge(properties, [OpenIdConnectDefaults.AuthenticationScheme]);
        })
    .AllowAnonymous();

app
    .MapGet(
        "/authentication/logout",
        () => {
            var properties = new AuthenticationProperties { RedirectUri = "/" };
            return Results.SignOut(properties, [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]);
        })
    .AllowAnonymous();

app
    .MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

return;

static bool IsLocalReturnUrl(string? returnUrl)
{
    if (string.IsNullOrWhiteSpace(returnUrl))
        return false;

    return returnUrl.StartsWith('/')
           && !returnUrl.StartsWith("//")
           && !returnUrl.StartsWith("/\\");
}

static void ConfigureDataProtection(IServiceCollection services, DataProtectionSettings settings)
{
    var certificate = X509CertificateLoader.LoadPkcs12FromFile(
        settings.CertificatePath,
        settings.CertificatePassword,
        X509KeyStorageFlags.EphemeralKeySet);

    services
        .AddDataProtection()
        .SetApplicationName("Taurus")
        .PersistKeysToFileSystem(new DirectoryInfo(settings.KeysPath))
        .ProtectKeysWithCertificate(certificate);
}