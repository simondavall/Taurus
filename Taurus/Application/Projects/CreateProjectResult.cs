namespace Taurus.Application.Projects;

public sealed record CreateProjectResult(
    Project? Project,
    string? ErrorMessage)
{
    public bool Succeeded => Project is not null;

    public static CreateProjectResult Success(Project project)
    {
        return new CreateProjectResult(project, null);
    }

    public static CreateProjectResult Failure(string errorMessage)
    {
        return new CreateProjectResult(null, errorMessage);
    }
}