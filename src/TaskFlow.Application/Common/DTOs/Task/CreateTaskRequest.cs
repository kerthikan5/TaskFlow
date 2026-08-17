using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Common.DTOs.Task;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public Guid? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
}
