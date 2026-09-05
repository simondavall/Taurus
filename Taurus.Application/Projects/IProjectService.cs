namespace Taurus.Application.Projects;

public interface IProjectService {
    Task<ApplicationResult<Project>> CreateProjectAsync(CreateProjectRequest request);
    Task<ApplicationResult> DeleteProjectAsync(Guid id);
    Task<IReadOnlyList<Project>> GetProjectsAsync();
    Task<ApplicationResult> UpdateProjectAsync(UpdateProjectRequest request);
}