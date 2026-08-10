using PegasusApi.Abstractions.Projects;

namespace Taurus.Application.Projects;

public interface IProjectService
{
    Task<IReadOnlyList<Project>> GetProjectsAsync();
}

public sealed class ProjectService(HttpClient httpClient, ILogger<ProjectService> logger) : IProjectService
{
    public async Task<IReadOnlyList<Project>> GetProjectsAsync()
    {
        logger.LogInformation("Retrieving projects from PegasusApi");

        try
        {
            var response = await httpClient.GetFromJsonAsync<ProjectsResponse>("api/projects");
            if (response is null)
            {
                throw new InvalidOperationException("PegasusApi returned an empty projects response.");
            }

            var projects = response.Items
                .Select(project => new Project(
                    project.Id,
                    project.Title,
                    project.Prefix,
                    project.IsActive))
                .ToArray();

            logger.LogInformation("Retrieved {ProjectCount} projects from PegasusApi", projects.Length);
            return projects;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to retrieve projects from PegasusApi");
            throw;
        }
    }
}