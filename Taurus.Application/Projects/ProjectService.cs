using Taurus.Application.Caching;

namespace Taurus.Application.Projects;

public interface IProjectService
{
    Task<ApplicationResult<Project>> CreateProjectAsync(CreateProject createProject);
    Task<ApplicationResult> DeleteProjectAsync(Guid id);
    Task<IReadOnlyList<Project>> GetProjectsAsync();
    Task<ApplicationResult> UpdateProjectAsync(UpdateProject updateProject);
}

public sealed class ProjectService(IProjectDataProvider dataProvider, ICacheService cacheService, ProjectCacheOptions cacheOptions) : IProjectService
{
    private const string ProjectsCacheKey = "projects:list";

    public Task<IReadOnlyList<Project>> GetProjectsAsync()
    {
        return cacheService.GetOrCreateAsync(
            ProjectsCacheKey,
            cacheOptions.Duration,
            dataProvider.GetProjectsAsync);
    }

    public async Task<ApplicationResult<Project>> CreateProjectAsync(CreateProject project)
    {
        var result = await dataProvider.CreateProjectAsync(project);

        if (result.Succeeded)
            cacheService.Remove(ProjectsCacheKey);

        return result;
    }

    public async Task<ApplicationResult> UpdateProjectAsync(UpdateProject project)
    {
        var result = await dataProvider.UpdateProjectAsync(project);

        if (result.Succeeded)
            cacheService.Remove(ProjectsCacheKey);

        return result;
    }

    public async Task<ApplicationResult> DeleteProjectAsync(Guid id)
    {
        var result = await dataProvider.DeleteProjectAsync(id);

        if (result.Succeeded)
            cacheService.Remove(ProjectsCacheKey);

        return result;
    }
}