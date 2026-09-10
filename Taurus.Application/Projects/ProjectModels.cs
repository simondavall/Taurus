namespace Taurus.Application.Projects;

public sealed record Project(
    Guid Id,
    string Title,
    string Prefix,
    string? LatestVersion,
    bool RequireFixedInRelease,
    bool IsActive);
    
public sealed record CreateProjectRequest(
    string Title,
    string Prefix);

public sealed record UpdateProjectRequest(
    Guid Id,
    string Title,
    string Prefix,
    string? LatestVersion,
    bool RequireFixedInRelease,
    bool IsActive);