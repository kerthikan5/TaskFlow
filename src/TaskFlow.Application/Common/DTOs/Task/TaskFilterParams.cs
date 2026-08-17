using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Common.DTOs.Task;

public class TaskFilterParams
{
    public TaskItemStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public Guid? AssigneeId { get; set; }
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
