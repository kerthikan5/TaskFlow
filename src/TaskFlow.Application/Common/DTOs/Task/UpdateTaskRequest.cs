using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Common.DTOs.Task;

public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
}
