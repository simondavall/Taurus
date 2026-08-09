using PegasusApi.Abstractions.Projects;

namespace Taurus.Application.Projects;

public interface IProjectService
{
    Task<IReadOnlyList<Project>> GetProjectsAsync(bool activeOnly, bool includeDeleted);
}

public sealed class ProjectService(HttpClient httpClient) : IProjectService
{
    public async Task<IReadOnlyList<Project>> GetProjectsAsync(bool activeOnly, bool includeDeleted)
    {
        var requestUri = $"api/projects?"
                         + $"activeOnly={activeOnly.ToString().ToLowerInvariant()}&"
                         + $"includeDeleted={includeDeleted.ToString().ToLowerInvariant()}";

        var response = await httpClient.GetFromJsonAsync<ProjectsResponse>(requestUri);

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