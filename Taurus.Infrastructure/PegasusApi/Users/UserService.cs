using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using PegasusApi.Abstractions.Users;
using Taurus.Application.Users;

namespace Taurus.Infrastructure.PegasusApi.Users;

public sealed class UserService(HttpClient httpClient, ILogger<UserService> logger) : IUserService
{
    public async Task<IReadOnlyList<User>> GetUsersAsync()
    {
        logger.LogInformation("Retrieving users from PegasusApi");

        try {
            var response = await httpClient.GetFromJsonAsync<UsersResponse>("api/users");
            if (response is null) throw new InvalidOperationException("PegasusApi returned an empty users response.");

            var users = response.Items
                .Select(MapUser)
                .OrderBy(user => user.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            logger.LogInformation("Retrieved {UserCount} users from PegasusApi", users.Length);

            return users;
        } catch (Exception exception) {
            logger.LogError(exception, "Failed to retrieve users from PegasusApi");
            throw;
        }
    }

    private static User MapUser(UserResponse user)
    {
        return new User(user.Id, user.DisplayName);
    }
}