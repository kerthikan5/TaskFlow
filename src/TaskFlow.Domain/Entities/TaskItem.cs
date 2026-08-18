using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Represents an individual task item within a project.
/// </summary>
public class TaskItem
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public Guid CreatedById { get; set; }

    public User CreatedBy { get; set; } = null!;

    public Guid? AssigneeId { get; set; }

    public User? Assignee { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
