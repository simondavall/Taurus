namespace Taurus.Application.Projects;

public sealed record UpdateProjectRequest(
    Guid Id,
    string Title,
    string Prefix,
    string? LatestVersion,
    bool RequireFixedInRelease,
    bool IsActive);