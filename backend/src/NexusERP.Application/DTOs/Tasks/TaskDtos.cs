using NexusERP.Domain.Enums;
using TaskStatus = NexusERP.Domain.Enums.TaskStatus;

namespace NexusERP.Application.DTOs.Tasks;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    TaskStatus Status,
    TaskPriority Priority,
    int Order,
    DateTime? DueDate,
    DateTime? StartDate,
    decimal? EstimatedHours,
    decimal? ActualHours,
    Guid ProjectId,
    string ProjectName,
    Guid? AssigneeId,
    string? AssigneeName,
    Guid? ParentTaskId,
    string? Tags,
    DateTime CreatedAt);

public record KanbanColumnDto(TaskStatus Status, string Label, IReadOnlyList<TaskDto> Tasks);

public record CreateTaskRequest(
    string Title,
    string? Description,
    TaskStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    DateTime? StartDate,
    decimal? EstimatedHours,
    Guid ProjectId,
    Guid? AssigneeId,
    Guid? ParentTaskId,
    string? Tags);

public record UpdateTaskRequest(
    Guid Id,
    string Title,
    string? Description,
    TaskStatus Status,
    TaskPriority Priority,
    int Order,
    DateTime? DueDate,
    DateTime? StartDate,
    decimal? EstimatedHours,
    decimal? ActualHours,
    Guid? AssigneeId,
    string? Tags);

public record MoveTaskRequest(Guid TaskId, TaskStatus NewStatus, int NewOrder);
