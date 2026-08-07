using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Taurus.Components.Layout;

public partial class MainLayout
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private IHttpContextAccessor HttpContextAccessor { get; set; } = default!;

    private bool _navigationOpen = true;

    private DrawerVariant _drawerVariant = DrawerVariant.Persistent;

    private Task BreakpointChanged(Breakpoint breakpoint)
    {
        var mobile = breakpoint <= Breakpoint.Md;

        _drawerVariant = mobile ? DrawerVariant.Temporary : DrawerVariant.Persistent;
        _navigationOpen = !mobile;

        StateHasChanged();

        return Task.CompletedTask;
    }
    
    private static string DisplayName(ClaimsPrincipal user)
    {
        return user.FindFirst("display_name")?.Value
               ?? user.Identity?.Name
               ?? string.Empty;
    }

    private async Task LoginAsync()
    {
        var httpContext = HttpContextAccessor.HttpContext;

        if (httpContext is null)
            return;

        await httpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }
    
    private void ToggleNavigation()
    {
        _navigationOpen = !_navigationOpen;
    }
}