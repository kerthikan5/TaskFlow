using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Represents a registered user account in the TaskFlow system.
/// </summary>
public class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // EF Core Navigation Properties
    public ICollection<Project> OwnedProjects { get; set; } = [];

    public ICollection<ProjectMember> ProjectMemberships { get; set; } = [];

    public ICollection<TaskItem> CreatedTasks { get; set; } = [];

    public ICollection<TaskItem> AssignedTasks { get; set; } = [];
}
