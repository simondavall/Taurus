using PegasusApi.Abstractions.Projects;

namespace Taurus.Application.Projects;

public interface IProjectService
{
    Task<IReadOnlyList<Project>> GetProjectsAsync();
}

public sealed class ProjectService(HttpClient httpClient) : IProjectService
{
    public async Task<IReadOnlyList<Project>> GetProjectsAsync()
    {
        var response = await httpClient.GetFromJsonAsync<ProjectsResponse>("api/projects");

        if (response is null)
        {
            throw new InvalidOperationException("PegasusApi returned an empty projects response.");
        }

        return response.Items
            .Select(project => new Project(
                project.Id,
                project.Title,
                project.Prefix,
                project.IsActive))
            .ToArray();
    }
}