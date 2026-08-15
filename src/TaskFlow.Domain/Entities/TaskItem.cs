using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// A unit of work within a project.
///
/// Named TaskItem (not Task) to avoid conflict with System.Threading.Tasks.Task,
/// which is used extensively by .NET's async/await pattern.
/// </summary>
public class TaskItem
{
    // ─────────────────────────────────────────────────────────
    // Identity
    // ─────────────────────────────────────────────────────────

    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    // ─────────────────────────────────────────────────────────
    // Status and priority
    // ─────────────────────────────────────────────────────────

    public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    // ─────────────────────────────────────────────────────────
    // Project relationship
    // ─────────────────────────────────────────────────────────

    /// <summary>Foreign key — the project this task belongs to. Required.</summary>
    public Guid ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    // ─────────────────────────────────────────────────────────
    // User relationships
    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// Foreign key — the user who created this task. Required.
    /// This is always recorded and never changes after creation.
    /// </summary>
    public Guid CreatedById { get; set; }

    public User CreatedBy { get; set; } = null!;

    /// <summary>
    /// Foreign key — the user assigned to work on this task. Optional (nullable).
    /// A task can exist without being assigned to anyone.
    /// </summary>
    public Guid? AssigneeId { get; set; }

    /// <summary>
    /// Navigation property for the assignee. Nullable because AssigneeId is nullable.
    /// </summary>
    public User? Assignee { get; set; }

    // ─────────────────────────────────────────────────────────
    // Deadline and timestamps
    // ─────────────────────────────────────────────────────────

    /// <summary>Optional deadline for this task (UTC).</summary>
    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
