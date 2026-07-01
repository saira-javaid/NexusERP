using NexusERP.Domain.Common;
using NexusERP.Domain.Enums;
using TaskStatus = NexusERP.Domain.Enums.TaskStatus;

namespace NexusERP.Domain.Entities;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public int Order { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? StartDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public Guid ProjectId { get; set; }
    public Guid? AssigneeId { get; set; }
    public Guid? ParentTaskId { get; set; }
    public string? Tags { get; set; }

    public Project Project { get; set; } = null!;
    public ApplicationUser? Assignee { get; set; }
    public TaskItem? ParentTask { get; set; }
    public ICollection<TaskItem> SubTasks { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}
