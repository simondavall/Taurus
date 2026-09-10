namespace Taurus.Application.Projects;

public sealed record Project(
    Guid Id,
    string Title,
    string Prefix,
    string? LatestVersion,
    bool RequireFixedInRelease,
    bool IsActive);
    
public sealed record CreateProject(
    string Title,
    string Prefix);

public sealed record UpdateProject(
    Guid Id,
    string Title,
    string Prefix,
    string? LatestVersion,
    bool RequireFixedInRelease,
    bool IsActive);