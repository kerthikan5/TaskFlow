using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Common.DTOs.ProjectMember;

public class ProjectMemberDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsOwner { get; set; }
}
