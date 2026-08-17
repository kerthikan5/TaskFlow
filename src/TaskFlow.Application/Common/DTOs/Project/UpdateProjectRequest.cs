using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Common.DTOs.Project;

public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
}
