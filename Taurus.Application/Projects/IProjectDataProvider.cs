namespace Taurus.Application.Projects;

public interface IProjectDataProvider
{
    Task<ApplicationResult<Project>> CreateProjectAsync(CreateProject createProject);
    Task<ApplicationResult> DeleteProjectAsync(Guid id);
    Task<IReadOnlyList<Project>> GetProjectsAsync();
    Task<ApplicationResult> UpdateProjectAsync(UpdateProject updateProject);
}