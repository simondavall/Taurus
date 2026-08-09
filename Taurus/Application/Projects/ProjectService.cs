namespace Taurus.Application.Projects;

public interface IProjectService
{
    public Task<IReadOnlyList<Project>> GetProjectsAsync();
}

public sealed class ProjectService : IProjectService
{
    private static readonly IReadOnlyList<Project> Projects =
    [
        new(
            Guid.Parse("9fbc39a2-32c4-4708-bbea-c087ce13590d"),
            "Taurus",
            "TAU",
            true),
        new(
            Guid.Parse("da47bd9e-a08d-4385-a824-eb3042902d03"),
            "Pegasus",
            "PEG",
            true),
        new(
            Guid.Parse("572c8d40-65bb-4975-b052-68109966d158"),
            "Archive",
            "ARC",
            false)
    ];

    public Task<IReadOnlyList<Project>> GetProjectsAsync()
    {
        return Task.FromResult(Projects);
    }
}