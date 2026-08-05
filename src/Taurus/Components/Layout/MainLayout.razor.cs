using Microsoft.AspNetCore.Components;

namespace Taurus.Components.Layout;

public partial class MainLayout
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool _navigationOpen = true;

    private void ToggleNavigation()
    {
        _navigationOpen = !_navigationOpen;
    }

    private Task RefreshPageAsync()
    {
        NavigationManager.Refresh(forceReload: true);
        return Task.CompletedTask;
    }
}