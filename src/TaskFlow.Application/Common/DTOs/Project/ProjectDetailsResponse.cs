using TaskFlow.Application.Common.DTOs.Auth;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Common.DTOs.Project;

public class ProjectDetailsResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public UserDto Owner { get; set; } = null!;
    public ProjectStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<UserDto> Members { get; set; } = new();
}
