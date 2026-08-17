using TaskFlow.Application.Common.DTOs.Auth;
using TaskFlow.Application.Common.DTOs.User;

namespace TaskFlow.Application.Common.Interfaces;

public interface IUserService
{
    Task<UserDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
