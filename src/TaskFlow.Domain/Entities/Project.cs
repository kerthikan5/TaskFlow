using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// A project groups related tasks together and is owned by a single user.
/// </summary>
public class Project
{
    // ─────────────────────────────────────────────────────────
    // Identity
    // ─────────────────────────────────────────────────────────

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // ─────────────────────────────────────────────────────────
    // Ownership
    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// Foreign key — the ID of the User who owns this project.
    /// Ownership determines who can update/delete the project and manage members.
    /// This is NOT a global role — it is per-project.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>Navigation property — EF Core loads the full User object for the owner.</summary>
    public User Owner { get; set; } = null!;

    // ─────────────────────────────────────────────────────────
    // Status and deadline
    // ─────────────────────────────────────────────────────────

    public ProjectStatus Status { get; set; } = ProjectStatus.Active;

    /// <summary>Optional project deadline (UTC). Nullable — not all projects need a deadline.</summary>
    public DateTime? DueDate { get; set; }

    // ─────────────────────────────────────────────────────────
    // Timestamps
    // ─────────────────────────────────────────────────────────

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // ─────────────────────────────────────────────────────────
    // Navigation properties
    // ─────────────────────────────────────────────────────────

    /// <summary>All users who are members of this project (via the join table).</summary>
    public ICollection<ProjectMember> Members { get; set; } = [];

    /// <summary>All tasks belonging to this project.</summary>
    public ICollection<TaskItem> Tasks { get; set; } = [];
}
