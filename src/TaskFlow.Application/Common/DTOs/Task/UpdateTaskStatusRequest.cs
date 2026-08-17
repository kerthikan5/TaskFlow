using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Common.DTOs.Task;

public class UpdateTaskStatusRequest
{
    public TaskItemStatus Status { get; set; }
}
