using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.DTOs.Project;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Gets all projects accessible by the authenticated user (owned or member of).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> GetAccessibleProjects(CancellationToken cancellationToken)
    {
        var projects = await _projectService.GetAccessibleProjectsAsync(cancellationToken);
        return Ok(projects);
    }

    /// <summary>
    /// Gets details of a specific project by ID.
    /// User must be owner or member of the project.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailsResponse>> GetProjectById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var project = await _projectService.GetProjectByIdAsync(id, cancellationToken);
        return Ok(project);
    }

    /// <summary>
    /// Creates a new project and sets the current user as the Owner.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProjectResponse>> CreateProject(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var project = await _projectService.CreateProjectAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProjectById), new { id = project.Id }, project);
    }

    /// <summary>
    /// Updates project details. Only the project Owner can perform this action.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectResponse>> UpdateProject(
        [FromRoute] Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var updatedProject = await _projectService.UpdateProjectAsync(id, request, cancellationToken);
        return Ok(updatedProject);
    }

    /// <summary>
    /// Deletes a project. Only the project Owner can perform this action.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _projectService.DeleteProjectAsync(id, cancellationToken);
        return NoContent();
    }
}
