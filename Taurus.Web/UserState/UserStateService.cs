using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Taurus.Application.Tickets;
using Taurus.Application.UserState;

namespace Taurus.UserState;

public sealed class UserStateService(ProtectedLocalStorage localStorage) : IUserStateService
{
    private const string SelectedProjectIdKey = "Taurus.SelectedProjectId";
    private const string SelectedTicketFilterKey = "Taurus.SelectedTicketFilter";

    public async Task<Guid?> GetSelectedProjectIdAsync()
    {
        try {
            var result = await localStorage.GetAsync<Guid?>(SelectedProjectIdKey);
            return result.Success ? result.Value : null;
        } catch {
            return null;
        }
    }

    public async Task SetSelectedProjectIdAsync(Guid? projectId)
    {
        if (projectId.HasValue) {
            await localStorage.SetAsync(SelectedProjectIdKey, projectId);
            return;
        }

        await localStorage.DeleteAsync(SelectedProjectIdKey);
    }

    public async Task<TicketFilter?> GetSelectedTicketFilterAsync()
    {
        try {
            var result = await localStorage.GetAsync<TicketFilter>(SelectedTicketFilterKey);
            if (!result.Success || !Enum.IsDefined(result.Value)) return null;
            return result.Value;
        } catch {
            return null;
        }
    }

    public Task SetSelectedTicketFilterAsync(TicketFilter filter)
    {
        return localStorage.SetAsync(SelectedTicketFilterKey, filter).AsTask();
    }
}