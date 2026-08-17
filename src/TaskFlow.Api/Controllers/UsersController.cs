using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.DTOs.Auth;
using TaskFlow.Application.Common.DTOs.User;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Updates the profile (FirstName and LastName) of the currently authenticated user.
    /// Requires Bearer JWT Token in Authorization header.
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var updatedUser = await _userService.UpdateProfileAsync(request, cancellationToken);
        return Ok(updatedUser);
    }
}
