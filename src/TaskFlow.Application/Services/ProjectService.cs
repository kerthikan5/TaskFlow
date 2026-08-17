using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.DTOs.Auth;
using TaskFlow.Application.Common.DTOs.Project;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ProjectService(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Project name is required.");
        }

        var currentUser = await _context.Users.FindAsync(new object[] { currentUserId }, cancellationToken)
            ?? throw new NotFoundException("User", currentUserId);

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            OwnerId = currentUserId,
            Status = request.Status,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Automatically add owner as the first member in project_members
        var ownerMembership = new ProjectMember
        {
            ProjectId = project.Id,
            UserId = currentUserId,
            JoinedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        _context.ProjectMembers.Add(ownerMembership);

        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            OwnerName = $"{currentUser.FirstName} {currentUser.LastName}",
            Status = project.Status,
            DueDate = project.DueDate,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            MemberCount = 1,
            TaskCount = 0
        };
    }

    public async Task<IReadOnlyList<ProjectResponse>> GetAccessibleProjectsAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        var projects = await _context.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == currentUserId || p.Members.Any(m => m.UserId == currentUserId))
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);

        return projects.Select(p => new ProjectResponse
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            OwnerId = p.OwnerId,
            OwnerName = $"{p.Owner.FirstName} {p.Owner.LastName}",
            Status = p.Status,
            DueDate = p.DueDate,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            MemberCount = p.Members.Count,
            TaskCount = p.Tasks.Count
        }).ToList();
    }

    public async Task<ProjectDetailsResponse> GetProjectByIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .SingleOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException("Project", projectId);
        }

        // Authorization check: User must be owner OR member
        var isAuthorized = project.OwnerId == currentUserId || project.Members.Any(m => m.UserId == currentUserId);
        if (!isAuthorized)
        {
            throw new ForbiddenException("You do not have access to this project.");
        }

        return new ProjectDetailsResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Owner = MapToUserDto(project.Owner),
            Status = project.Status,
            DueDate = project.DueDate,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            Members = project.Members.Select(m => MapToUserDto(m.User)).ToList()
        };
    }

    public async Task<ProjectResponse> UpdateProjectAsync(Guid projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Project name is required.");
        }

        var project = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .SingleOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException("Project", projectId);
        }

        // Mutation authorization: ONLY Owner can update
        if (project.OwnerId != currentUserId)
        {
            throw new ForbiddenException("Only the project owner can modify project details.");
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description?.Trim();
        project.Status = request.Status;
        project.DueDate = request.DueDate;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            OwnerId = project.OwnerId,
            OwnerName = $"{project.Owner.FirstName} {project.Owner.LastName}",
            Status = project.Status,
            DueDate = project.DueDate,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            MemberCount = project.Members.Count,
            TaskCount = project.Tasks.Count
        };
    }

    public async Task DeleteProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        var project = await _context.Projects.FindAsync(new object[] { projectId }, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException("Project", projectId);
        }

        // Mutation authorization: ONLY Owner can delete
        if (project.OwnerId != currentUserId)
        {
            throw new ForbiddenException("Only the project owner can delete this project.");
        }

        _context.Projects.Remove(project);
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

    private static UserDto MapToUserDto(User user) => new()
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
