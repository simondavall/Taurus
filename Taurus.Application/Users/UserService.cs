namespace Taurus.Application.Users;

public interface IUserService
{
    Task<IReadOnlyList<User>> GetUsersAsync();
}

public sealed class UserService(IUserDataProvider dataProvider) : IUserService
{
    public Task<IReadOnlyList<User>> GetUsersAsync()
    {
        return dataProvider.GetUsersAsync();
    }
}