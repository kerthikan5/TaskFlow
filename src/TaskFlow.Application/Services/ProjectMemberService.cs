using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.DTOs.ProjectMember;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Services;

public class ProjectMemberService : IProjectMemberService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ProjectMemberService(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ProjectMemberDto> AddMemberAsync(Guid projectId, AddProjectMemberRequest request, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ValidationException("Email is required.");
        }

        var project = await _context.Projects
            .SingleOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException("Project", projectId);
        }

        // Authorization: Only the Project Owner can add members
        if (project.OwnerId != currentUserId)
        {
            throw new ForbiddenException("Only the project owner can invite new members.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var targetUser = await _context.Users
            .SingleOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (targetUser == null)
        {
            throw new NotFoundException($"User with email '{request.Email}' was not found.");
        }

        // Check if user is already a member
        var existingMembership = await _context.ProjectMembers
            .SingleOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == targetUser.Id, cancellationToken);

        if (existingMembership != null)
        {
            throw new ConflictException($"User '{targetUser.Email}' is already a member of this project.");
        }

        var newMembership = new ProjectMember
        {
            ProjectId = projectId,
            UserId = targetUser.Id,
            JoinedAt = DateTime.UtcNow
        };

        _context.ProjectMembers.Add(newMembership);
        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectMemberDto
        {
            UserId = targetUser.Id,
            Email = targetUser.Email,
            FirstName = targetUser.FirstName,
            LastName = targetUser.LastName,
            Role = targetUser.Role,
            JoinedAt = newMembership.JoinedAt,
            IsOwner = project.OwnerId == targetUser.Id
        };
    }

    public async Task<IReadOnlyList<ProjectMemberDto>> GetProjectMembersAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        var project = await _context.Projects
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException("Project", projectId);
        }

        // Authorization: Current user must be Owner OR a member
        var isMember = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == currentUserId, cancellationToken);

        if (!isMember && project.OwnerId != currentUserId)
        {
            throw new ForbiddenException("You do not have access to view members of this project.");
        }

        var members = await _context.ProjectMembers
            .AsNoTracking()
            .Where(m => m.ProjectId == projectId)
            .Include(m => m.User)
            .OrderBy(m => m.JoinedAt)
            .ToListAsync(cancellationToken);

        return members.Select(m => new ProjectMemberDto
        {
            UserId = m.UserId,
            Email = m.User.Email,
            FirstName = m.User.FirstName,
            LastName = m.User.LastName,
            Role = m.User.Role,
            JoinedAt = m.JoinedAt,
            IsOwner = project.OwnerId == m.UserId
        }).ToList();
    }

    public async Task RemoveMemberAsync(Guid projectId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        var project = await _context.Projects
            .SingleOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException("Project", projectId);
        }

        // Cannot remove the project owner
        if (project.OwnerId == targetUserId)
        {
            throw new ValidationException("The project owner cannot be removed from the project.");
        }

        // Authorization rules:
        // 1. Project Owner can remove ANY member
        // 2. Member can remove THEMSELVES (Leave Project)
        // 3. Anyone else is Forbidden
        var isOwner = project.OwnerId == currentUserId;
        var isSelfRemoval = currentUserId == targetUserId;

        if (!isOwner && !isSelfRemoval)
        {
            throw new ForbiddenException("Only the project owner can remove other team members.");
        }

        var membership = await _context.ProjectMembers
            .SingleOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == targetUserId, cancellationToken);

        if (membership == null)
        {
            throw new NotFoundException($"User ({targetUserId}) is not a member of project ({projectId}).");
        }

        _context.ProjectMembers.Remove(membership);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private Guid GetCurrentUserIdOrThrow()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new ForbiddenException("User is not authenticated.");
        }
        return _currentUserService.UserId.Value;
    }
}
