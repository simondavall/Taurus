namespace Taurus.Application.Projects;

public sealed record Project(
    Guid Id,
    string Title,
    string Prefix,
    bool IsActive);