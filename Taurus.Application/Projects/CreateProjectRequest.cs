namespace Taurus.Application.Projects;

public sealed record CreateProjectRequest(
    string Title,
    string Prefix);