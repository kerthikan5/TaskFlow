using TaskFlow.Application.Common.DTOs.Task;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Application.Common.Interfaces;

public interface ITaskService
{
    Task<TaskResponse> CreateTaskAsync(Guid projectId, CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<TaskResponse>> GetProjectTasksAsync(Guid projectId, TaskFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<PagedResponse<TaskResponse>> GetMyAssignedTasksAsync(TaskFilterParams filterParams, CancellationToken cancellationToken = default);
    Task<TaskResponse> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<TaskResponse> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskResponse> UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default);
    Task DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
}
