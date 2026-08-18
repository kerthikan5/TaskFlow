using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Project entity representing a workspace containing tasks and team members.
/// </summary>
public class Project
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid OwnerId { get; set; }

    public User Owner { get; set; } = null!;

    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // EF Core Navigation Properties
    public ICollection<ProjectMember> Members { get; set; } = [];

    public ICollection<TaskItem> Tasks { get; set; } = [];
}
