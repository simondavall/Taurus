using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Taurus.Components.Features.Shared;

public interface INavigationHistoryService
{
    void Start();
    bool TryNavigateBack();
}

public sealed class NavigationHistoryService(NavigationManager navigationManager) : INavigationHistoryService, IDisposable
{
    private readonly List<string> _history = [];
    private bool _started;
    private bool _suppressNextLocation;

    public void Start()
    {
        if (_started)
        {
            return;
        }

        _started = true;

        _history.Add(GetLocalPath(navigationManager.Uri));

        navigationManager.LocationChanged += NavigationManagerOnLocationChanged;
    }

    public bool TryNavigateBack()
    {
        if (_history.Count < 2)
        {
            return false;
        }

        _history.RemoveAt(_history.Count - 1);

        var target = _history[^1];

        _suppressNextLocation = true;
        navigationManager.NavigateTo(target, replace: true);

        return true;
    }

    private void NavigationManagerOnLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        if (_suppressNextLocation)
        {
            _suppressNextLocation = false;
            return;
        }

        var path = GetLocalPath(args.Location);

        if (_history.Count > 0 && string.Equals(_history[^1], path, StringComparison.Ordinal))
        {
            return;
        }

        _history.Add(path);
    }

    private string GetLocalPath(string uri)
    {
        var relativePath = navigationManager.ToBaseRelativePath(uri);
        return string.IsNullOrEmpty(relativePath) ? "/" : $"/{relativePath}";
    }

    public void Dispose()
    {
        if (_started)
        {
            navigationManager.LocationChanged -= NavigationManagerOnLocationChanged;
        }
    }
}