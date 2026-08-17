using TaskFlow.Application.Common.DTOs.Project;

namespace TaskFlow.Application.Common.Interfaces;

public interface IProjectService
{
    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectResponse>> GetAccessibleProjectsAsync(CancellationToken cancellationToken = default);
    Task<ProjectDetailsResponse> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<ProjectResponse> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}
