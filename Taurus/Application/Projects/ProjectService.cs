using System.Net;
using PegasusApi.Abstractions.Projects;
using Taurus.Application.PegasusApi;
using PegasusCreateProjectRequest = PegasusApi.Abstractions.Projects.CreateProjectRequest;
using PegasusUpdateProjectRequest = PegasusApi.Abstractions.Projects.UpdateProjectRequest;

namespace Taurus.Application.Projects;

public interface IProjectService
{
    Task<IReadOnlyList<Project>> GetProjectsAsync();
    Task<ApplicationResult<Project>> CreateProjectAsync(CreateProjectRequest request);
    Task<ApplicationResult> UpdateProjectAsync(UpdateProjectRequest request);
    Task<ApplicationResult> DeleteProjectAsync(Guid id);
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
                .Select(MapProject)
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

    public async Task<ApplicationResult<Project>> CreateProjectAsync(CreateProjectRequest request)
    {
        logger.LogInformation("Creating project in PegasusApi");

        try
        {
            var apiRequest = new PegasusCreateProjectRequest
            {
                Title = request.Title,
                Prefix = request.Prefix
            };

            using var response = await httpClient.PostAsJsonAsync("api/projects", apiRequest);

            if (response.IsSuccessStatusCode)
            {
                var projectResponse = await response.Content.ReadFromJsonAsync<ProjectResponse>();
                if (projectResponse is null)
                {
                    throw new InvalidOperationException("PegasusApi returned an empty project response after project creation.");
                }

                var project = MapProject(projectResponse);

                logger.LogInformation("Created project {ProjectId} in PegasusApi", project.Id);
                return ApplicationResult<Project>.Success(project);
            }

            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict)
            {
                var errorMessage = await PegasusApiFailureReader.ReadAsync(
                    response,
                    "The project could not be created because PegasusApi rejected the supplied details.");

                logger.LogWarning(
                    "PegasusApi rejected project creation with status code {StatusCode}",
                    (int)response.StatusCode);

                return ApplicationResult<Project>.Failure(errorMessage);
            }

            response.EnsureSuccessStatusCode();

            throw new InvalidOperationException("PegasusApi project creation failed unexpectedly.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to create project in PegasusApi");
            throw;
        }
    }

    public async Task<ApplicationResult> UpdateProjectAsync(UpdateProjectRequest request)
    {
        logger.LogInformation("Updating project {ProjectId} in PegasusApi", request.Id);

        try
        {
            var apiRequest = new PegasusUpdateProjectRequest
            {
                Title = request.Title,
                Prefix = request.Prefix,
                IsActive = request.IsActive,
                IsDeleted = false,
                LatestVersion = request.LatestVersion,
                RequireFixedInRelease = request.RequireFixedInRelease
            };

            using var response = await httpClient.PutAsJsonAsync($"api/projects/{request.Id}", apiRequest);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Updated project {ProjectId} in PegasusApi", request.Id);
                return ApplicationResult.Success();
            }

            if (response.StatusCode is HttpStatusCode.BadRequest
                or HttpStatusCode.NotFound
                or HttpStatusCode.Conflict)
            {
                var errorMessage = await PegasusApiFailureReader.ReadAsync(
                    response,
                    "The project could not be updated because PegasusApi rejected the supplied details.");

                logger.LogWarning(
                    "PegasusApi rejected update of project {ProjectId} with status code {StatusCode}",
                    request.Id,
                    (int)response.StatusCode);

                return ApplicationResult.Failure(errorMessage);
            }

            response.EnsureSuccessStatusCode();

            throw new InvalidOperationException("PegasusApi project update failed unexpectedly.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to update project {ProjectId} in PegasusApi", request.Id);
            throw;
        }
    }

    public async Task<ApplicationResult> DeleteProjectAsync(Guid id)
    {
        logger.LogInformation("Deleting project {ProjectId} in PegasusApi", id);

        try
        {
            using var response = await httpClient.DeleteAsync($"api/projects/{id}");

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Deleted project {ProjectId} in PegasusApi", id);
                return ApplicationResult.Success();
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogWarning(
                    "PegasusApi could not delete project {ProjectId} because it was not found",
                    id);

                return ApplicationResult.Failure(
                    "The project could not be deleted because it no longer exists.");
            }

            response.EnsureSuccessStatusCode();

            throw new InvalidOperationException("PegasusApi project deletion failed unexpectedly.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to delete project {ProjectId} in PegasusApi", id);
            throw;
        }
    }

    private static Project MapProject(ProjectResponse project)
    {
        return new Project(
            project.Id,
            project.Title,
            project.Prefix,
            project.LatestVersion,
            project.RequireFixedInRelease,
            project.IsActive);
    }
}