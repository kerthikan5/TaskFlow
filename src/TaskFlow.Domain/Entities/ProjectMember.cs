namespace TaskFlow.Domain.Entities;

/// <summary>
/// Join table representing the many-to-many relationship between User and Project.
/// A row in this table means: "this user is a member of this project."
///
/// Primary key is composite: (ProjectId, UserId) — enforces uniqueness at the database level.
/// </summary>
public class ProjectMember
{
    // ─────────────────────────────────────────────────────────
    // Composite primary key (configured in EF Core entity config)
    // ─────────────────────────────────────────────────────────

    /// <summary>Part 1 of composite PK. Foreign key → projects table.</summary>
    public Guid ProjectId { get; set; }

    /// <summary>Part 2 of composite PK. Foreign key → users table.</summary>
    public Guid UserId { get; set; }

    // ─────────────────────────────────────────────────────────
    // Membership metadata
    // ─────────────────────────────────────────────────────────

    /// <summary>UTC timestamp when this user was added to the project.</summary>
    public DateTime JoinedAt { get; set; }

    // ─────────────────────────────────────────────────────────
    // Navigation properties
    // ─────────────────────────────────────────────────────────

    /// <summary>The project this membership belongs to.</summary>
    public Project Project { get; set; } = null!;

    /// <summary>The user who is a member.</summary>
    public User User { get; set; } = null!;
}
