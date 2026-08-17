using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Common.DTOs.Task;

public class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public Guid? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
