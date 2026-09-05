namespace Taurus.Application.Users;

public sealed record User(
    Guid Id,
    string DisplayName);