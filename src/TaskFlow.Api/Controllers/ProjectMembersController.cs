using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.DTOs.ProjectMember;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/members")]
[Authorize]
public class ProjectMembersController : ControllerBase
{
    private readonly IProjectMemberService _memberService;

    public ProjectMembersController(IProjectMemberService memberService)
    {
        _memberService = memberService;
    }

    /// <summary>
    /// Gets all members of a project. Accessible by Project Owner and Members.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ProjectMemberDto>>> GetMembers(
        [FromRoute] Guid projectId,
        CancellationToken cancellationToken)
    {
        var members = await _memberService.GetProjectMembersAsync(projectId, cancellationToken);
        return Ok(members);
    }

    /// <summary>
    /// Invites/Adds a new team member to the project by email. Only the Project Owner can perform this action.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjectMemberDto>> AddMember(
        [FromRoute] Guid projectId,
        [FromBody] AddProjectMemberRequest request,
        CancellationToken cancellationToken)
    {
        var member = await _memberService.AddMemberAsync(projectId, request, cancellationToken);
        return CreatedAtAction(nameof(GetMembers), new { projectId }, member);
    }

    /// <summary>
    /// Removes a member from a project. Only Owner can remove others, or members can remove themselves.
    /// </summary>
    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(
        [FromRoute] Guid projectId,
        [FromRoute] Guid userId,
        CancellationToken cancellationToken)
    {
        await _memberService.RemoveMemberAsync(projectId, userId, cancellationToken);
        return NoContent();
    }
}
