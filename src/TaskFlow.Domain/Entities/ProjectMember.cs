namespace TaskFlow.Domain.Entities;

/// <summary>
/// Join entity representing project membership for a user.
/// </summary>
public class ProjectMember
{
    public Guid ProjectId { get; set; }

    public Guid UserId { get; set; }

    public DateTime JoinedAt { get; set; }

    public Project Project { get; set; } = null!;

    public User User { get; set; } = null!;
}
