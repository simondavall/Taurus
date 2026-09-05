namespace Taurus.Application.Users;

public interface IUserService
{
    Task<IReadOnlyList<User>> GetUsersAsync();
}