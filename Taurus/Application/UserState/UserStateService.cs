using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Taurus.Application.UserState;

public interface IUserStateService
{
    Task<Guid?> GetSelectedProjectIdAsync();
    Task SetSelectedProjectIdAsync(Guid? projectId);
}

public sealed class UserStateService(ProtectedLocalStorage localStorage) : IUserStateService
{
    private const string SelectedProjectIdKey = "Taurus.SelectedProjectId";

    public async Task<Guid?> GetSelectedProjectIdAsync()
    {
        try
        {
            var result = await localStorage.GetAsync<Guid?>(SelectedProjectIdKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task SetSelectedProjectIdAsync(Guid? projectId)
    {
        if (projectId.HasValue)
        {
            await localStorage.SetAsync(SelectedProjectIdKey, projectId);
            return;
        }

        await localStorage.DeleteAsync(SelectedProjectIdKey);
    }
}