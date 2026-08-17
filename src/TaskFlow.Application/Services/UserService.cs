using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.DTOs.Auth;
using TaskFlow.Application.Common.DTOs.User;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Application.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UserService(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<UserDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new ForbiddenException("User is not authenticated.");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ValidationException("First Name and Last Name are required.");
        }

        var userId = _currentUserService.UserId.Value;

        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
