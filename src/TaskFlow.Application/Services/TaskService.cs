using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.DTOs.Task;
using TaskFlow.Application.Common.Exceptions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Models;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Services;

public class TaskService : ITaskService
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public TaskService(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<TaskResponse> CreateTaskAsync(Guid projectId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ValidationException("Task title is required.");
        }

        // Verify project access
        await EnsureUserHasProjectAccessAsync(projectId, currentUserId, cancellationToken);

        // Verify assignee membership if assigned
        if (request.AssigneeId.HasValue)
        {
            await EnsureAssigneeIsProjectMemberAsync(projectId, request.AssigneeId.Value, cancellationToken);
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Status = TaskItemStatus.ToDo,
            Priority = request.Priority,
            ProjectId = projectId,
            CreatedById = currentUserId,
            AssigneeId = request.AssigneeId,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetTaskByIdAsync(task.Id, cancellationToken);
    }

    public async Task<PagedResponse<TaskResponse>> GetProjectTasksAsync(Guid projectId, TaskFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();
        await EnsureUserHasProjectAccessAsync(projectId, currentUserId, cancellationToken);

        var query = _context.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId);

        query = ApplyFilters(query, filterParams);

        var totalCount = await query.CountAsync(cancellationToken);

        var tasks = await query
            .Include(t => t.Project)
            .Include(t => t.CreatedBy)
            .Include(t => t.Assignee)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = tasks.Select(MapToTaskResponse).ToList();

        return new PagedResponse<TaskResponse>(dtos, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<PagedResponse<TaskResponse>> GetMyAssignedTasksAsync(TaskFilterParams filterParams, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        var query = _context.Tasks
            .AsNoTracking()
            .Where(t => t.AssigneeId == currentUserId);

        query = ApplyFilters(query, filterParams);

        var totalCount = await query.CountAsync(cancellationToken);

        var tasks = await query
            .Include(t => t.Project)
            .Include(t => t.CreatedBy)
            .Include(t => t.Assignee)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = tasks.Select(MapToTaskResponse).ToList();

        return new PagedResponse<TaskResponse>(dtos, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<TaskResponse> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        var task = await _context.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .Include(t => t.CreatedBy)
            .Include(t => t.Assignee)
            .SingleOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException("Task", taskId);
        }

        await EnsureUserHasProjectAccessAsync(task.ProjectId, currentUserId, cancellationToken);

        return MapToTaskResponse(task);
    }

    public async Task<TaskResponse> UpdateTaskAsync(Guid taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ValidationException("Task title is required.");
        }

        var task = await _context.Tasks
            .SingleOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException("Task", taskId);
        }

        await EnsureUserHasProjectAccessAsync(task.ProjectId, currentUserId, cancellationToken);

        if (request.AssigneeId.HasValue && request.AssigneeId != task.AssigneeId)
        {
            await EnsureAssigneeIsProjectMemberAsync(task.ProjectId, request.AssigneeId.Value, cancellationToken);
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Status = request.Status;
        task.Priority = request.Priority;
        task.AssigneeId = request.AssigneeId;
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetTaskByIdAsync(taskId, cancellationToken);
    }

    public async Task<TaskResponse> UpdateTaskStatusAsync(Guid taskId, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        var task = await _context.Tasks
            .SingleOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException("Task", taskId);
        }

        await EnsureUserHasProjectAccessAsync(task.ProjectId, currentUserId, cancellationToken);

        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetTaskByIdAsync(taskId, cancellationToken);
    }

    public async Task DeleteTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserIdOrThrow();

        var task = await _context.Tasks
            .SingleOrDefaultAsync(t => t.Id == taskId, cancellationToken);

        if (task == null)
        {
            throw new NotFoundException("Task", taskId);
        }

        await EnsureUserHasProjectAccessAsync(task.ProjectId, currentUserId, cancellationToken);

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<TaskItem> ApplyFilters(IQueryable<TaskItem> query, TaskFilterParams filterParams)
    {
        if (filterParams.Status.HasValue)
        {
            query = query.Where(t => t.Status == filterParams.Status.Value);
        }

        if (filterParams.Priority.HasValue)
        {
            query = query.Where(t => t.Priority == filterParams.Priority.Value);
        }

        if (filterParams.AssigneeId.HasValue)
        {
            query = query.Where(t => t.AssigneeId == filterParams.AssigneeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
        {
            var search = filterParams.SearchTerm.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(search) ||
                                     (t.Description != null && t.Description.ToLower().Contains(search)));
        }

        return query;
    }

    private async Task EnsureUserHasProjectAccessAsync(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException("Project", projectId);
        }

        var isMember = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);

        if (project.OwnerId != userId && !isMember)
        {
            throw new ForbiddenException("You do not have access to tasks in this project.");
        }
    }

    private async Task EnsureAssigneeIsProjectMemberAsync(Guid projectId, Guid assigneeId, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Id == projectId, cancellationToken);

        if (project == null)
        {
            throw new NotFoundException("Project", projectId);
        }

        var isOwner = project.OwnerId == assigneeId;
        var isMember = await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == assigneeId, cancellationToken);

        if (!isOwner && !isMember)
        {
            throw new ValidationException($"Selected assignee ({assigneeId}) is not a member of this project.");
        }
    }

    private Guid GetCurrentUserIdOrThrow()
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            throw new ForbiddenException("User is not authenticated.");
        }
        return _currentUserService.UserId.Value;
    }

    private static TaskResponse MapToTaskResponse(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        Priority = task.Priority,
        ProjectId = task.ProjectId,
        ProjectName = task.Project?.Name ?? string.Empty,
        CreatedById = task.CreatedById,
        CreatedByName = task.CreatedBy != null ? $"{task.CreatedBy.FirstName} {task.CreatedBy.LastName}" : string.Empty,
        AssigneeId = task.AssigneeId,
        AssigneeName = task.Assignee != null ? $"{task.Assignee.FirstName} {task.Assignee.LastName}" : null,
        DueDate = task.DueDate,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt
    };
}
