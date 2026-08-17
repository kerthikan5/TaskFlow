using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.DTOs.Task;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/tasks")]
[Authorize]
public class ProjectTasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public ProjectTasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Gets a paginated, filtered list of tasks for a specific project.
    /// Requires project membership or ownership.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<TaskResponse>>> GetProjectTasks(
        [FromRoute] Guid projectId,
        [FromQuery] TaskFilterParams filterParams,
        CancellationToken cancellationToken)
    {
        var result = await _taskService.GetProjectTasksAsync(projectId, filterParams, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new task within a project. Assignee must be a project member.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> CreateTask(
        [FromRoute] Guid projectId,
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await _taskService.CreateTaskAsync(projectId, request, cancellationToken);
        return CreatedAtAction("GetTaskById", "Tasks", new { id = task.Id }, task);
    }
}
