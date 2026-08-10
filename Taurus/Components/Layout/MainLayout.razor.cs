using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Taurus.Components.Layout;

public partial class MainLayout
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool _navigationOpen = true;

    private DrawerVariant _drawerVariant = DrawerVariant.Persistent;

    private Task BreakpointChanged(Breakpoint breakpoint)
    {
        var mobile = breakpoint <= Breakpoint.Sm;

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
    
    private void ToggleNavigation()
    {
        _navigationOpen = !_navigationOpen;
    }
}