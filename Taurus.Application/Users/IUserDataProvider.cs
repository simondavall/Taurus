namespace Taurus.Application.Users;

public interface IUserDataProvider
{
    Task<IReadOnlyList<User>> GetUsersAsync();
}