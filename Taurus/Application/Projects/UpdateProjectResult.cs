namespace Taurus.Application.Projects;

public sealed record UpdateProjectResult(bool Succeeded, string? ErrorMessage)
{
    public static UpdateProjectResult Success()
    {
        return new UpdateProjectResult(true, null);
    }

    public static UpdateProjectResult Failure(string errorMessage)
    {
        return new UpdateProjectResult(false, errorMessage);
    }
}