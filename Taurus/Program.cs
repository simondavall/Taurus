using DotNetEnv;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using MudBlazor.Services;
using Taurus.Application;
using Taurus.Components;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    Env.NoClobber()
        .TraversePath()
        .Load();

    builder.Configuration.AddEnvironmentVariables();
}

ValidateRequiredConfiguration(builder.Configuration);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        var configuration = builder.Configuration.GetSection("Authentication:OpenIdConnect");

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
        
        options.Events.OnRemoteFailure = context =>
        {
            var error = context.Failure?.Data["error"]?.ToString();
            
            var reason = error switch
            {
                "access_denied" => "unauthorized",
                "unauthorized_client" => "disabled",
                _ => null
            };

            if (reason is not null)
            {
                context.Response.Redirect($"/access-denied?reason={reason}");
                context.HandleResponse();
            }

            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddMudServices();
builder.Services.AddTaurusApplication();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
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

app.MapGet("/authentication/login", (string? returnUrl) =>
{
    var properties = new AuthenticationProperties
    {
        RedirectUri = IsLocalReturnUrl(returnUrl) ? returnUrl! : "/"
    };

    return Results.Challenge(properties, 
        [
            OpenIdConnectDefaults.AuthenticationScheme
        ]);
}).AllowAnonymous();

app.MapGet("/authentication/logout", () =>
{
    var properties = new AuthenticationProperties
    {
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
    string[] keys =
    [
        "Authentication:OpenIdConnect:Authority",
        "Authentication:OpenIdConnect:ClientId",
        "Authentication:OpenIdConnect:ClientSecret"
    ];
    
    var missing = keys
        .Where(key => string.IsNullOrWhiteSpace(configuration[key]))
        .ToArray();

    if (missing.Length == 0)
    {
        return;
    }

    throw new InvalidOperationException(
        "Missing required configuration values:" + Environment.NewLine +
        string.Join(Environment.NewLine, missing.Select(key => $" - {key}")));
}

static bool IsLocalReturnUrl(string? returnUrl)
{
    if (string.IsNullOrWhiteSpace(returnUrl))
    {
        return false;
    }

    return returnUrl.StartsWith('/')
           && !returnUrl.StartsWith("//")
           && !returnUrl.StartsWith("/\\");
}