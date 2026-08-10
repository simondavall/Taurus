using System.Net;
using System.Text.Json;
using PegasusApi.Abstractions.Projects;
using PegasusCreateProjectRequest = PegasusApi.Abstractions.Projects.CreateProjectRequest;
using PegasusUpdateProjectRequest = PegasusApi.Abstractions.Projects.UpdateProjectRequest;

namespace Taurus.Application.Projects;

public interface IProjectService
{
    Task<IReadOnlyList<Project>> GetProjectsAsync();
    Task<CreateProjectResult> CreateProjectAsync(CreateProjectRequest request);
    Task<UpdateProjectResult> UpdateProjectAsync(UpdateProjectRequest request);
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

    public async Task<CreateProjectResult> CreateProjectAsync(CreateProjectRequest request)
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
                return CreateProjectResult.Success(project);
            }

            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Conflict)
            {
                var errorMessage = await ReadExpectedFailureAsync(
                    response,
                    "The project could not be created because PegasusApi rejected the supplied details.");

                logger.LogWarning("PegasusApi rejected project creation with status code {StatusCode}", (int)response.StatusCode);
                return CreateProjectResult.Failure(errorMessage);
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

    public async Task<UpdateProjectResult> UpdateProjectAsync(UpdateProjectRequest request)
    {
        logger.LogInformation("Updating project {ProjectId} in PegasusApi", request.Id);

        try
        {
            var apiRequest = new PegasusUpdateProjectRequest
            {
                Title = request.Title,
                Prefix = request.Prefix,
                IsActive = request.IsActive,
                IsDeleted = false
            };

            using var response = await httpClient.PutAsJsonAsync($"api/projects/{request.Id}", apiRequest);

            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Updated project {ProjectId} in PegasusApi", request.Id);
                return UpdateProjectResult.Success();
            }

            if (response.StatusCode is HttpStatusCode.BadRequest
                or HttpStatusCode.NotFound
                or HttpStatusCode.Conflict)
            {
                var errorMessage = await ReadExpectedFailureAsync(
                    response,
                    "The project could not be updated because PegasusApi rejected the supplied details.");

                logger.LogWarning(
                    "PegasusApi rejected update of project {ProjectId} with status code {StatusCode}",
                    request.Id,
                    (int)response.StatusCode);

                return UpdateProjectResult.Failure(errorMessage);
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
    
    private static Project MapProject(ProjectResponse project)
    {
        return new Project(
            project.Id,
            project.Title,
            project.Prefix,
            project.IsActive);
    }

    private static async Task<string> ReadExpectedFailureAsync(HttpResponseMessage response, string fallbackMessage)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
        {
            return fallbackMessage;
        }

        try
        {
            using var document = JsonDocument.Parse(content);

            var messages = GetFailureMessages(document.RootElement)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .Distinct()
                .ToArray();

            return messages.Length > 0 ? string.Join(" ", messages) : fallbackMessage;
        }
        catch (JsonException)
        {
            return fallbackMessage;
        }
    }

    private static IEnumerable<string> GetFailureMessages(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            yield break;
        }

        foreach (var property in root.EnumerateObject())
        {
            if (property.Name.Equals("errors", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var message in GetStrings(property.Value))
                {
                    yield return message;
                }
            }
            else if (property.Name.Equals("message", StringComparison.OrdinalIgnoreCase)
                     || property.Name.Equals("detail", StringComparison.OrdinalIgnoreCase))
            {
                if (property.Value.ValueKind == JsonValueKind.String)
                {
                    yield return property.Value.GetString()!;
                }
            }
        }
    }

    private static IEnumerable<string> GetStrings(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                yield return element.GetString()!;
                break;

            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    foreach (var value in GetStrings(item))
                    {
                        yield return value;
                    }
                }

                break;

            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    foreach (var value in GetStrings(property.Value))
                    {
                        yield return value;
                    }
                }

                break;
        }
    }
}