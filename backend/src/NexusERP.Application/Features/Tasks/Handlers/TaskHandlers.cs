using MediatR;
using NexusERP.Application.Common;
using NexusERP.Application.Common.Interfaces;
using NexusERP.Application.DTOs.Tasks;
using NexusERP.Application.Features.Tasks.Commands;
using NexusERP.Application.Features.Tasks.Queries;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Interfaces;
using TaskStatus = NexusERP.Domain.Enums.TaskStatus;

namespace NexusERP.Application.Features.Tasks.Handlers;

public class GetTasksQueryHandler : IRequestHandler<GetTasksQuery, PagedResult<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTasksQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<TaskDto>> Handle(GetTasksQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _unitOfWork.Tasks.GetPagedAsync(
            request.ProjectId, request.Page, request.PageSize, request.Status, cancellationToken);

        return new PagedResult<TaskDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    internal static TaskDto MapToDto(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority, t.Order,
        t.DueDate, t.StartDate, t.EstimatedHours, t.ActualHours,
        t.ProjectId, t.Project?.Name ?? "", t.AssigneeId, t.Assignee?.FullName,
        t.ParentTaskId, t.Tags, t.CreatedAt);
}

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTaskByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<TaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetWithDetailsAsync(request.Id, cancellationToken);
        return task == null ? null : GetTasksQueryHandler.MapToDto(task);
    }
}

public class GetKanbanBoardQueryHandler : IRequestHandler<GetKanbanBoardQuery, IReadOnlyList<KanbanColumnDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetKanbanBoardQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<KanbanColumnDto>> Handle(GetKanbanBoardQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _unitOfWork.Tasks.GetByProjectForKanbanAsync(request.ProjectId, cancellationToken);
        var statuses = new[] { TaskStatus.Todo, TaskStatus.InProgress, TaskStatus.InReview, TaskStatus.Done };

        return statuses.Select(status => new KanbanColumnDto(
            status,
            status.ToString(),
            tasks.Where(t => t.Status == status).OrderBy(t => t.Order)
                .Select(GetTasksQueryHandler.MapToDto).ToList()
        )).ToList();
    }
}

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;

    public CreateTaskCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, INotificationService notifications)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var task = new TaskItem
        {
            Title = req.Title,
            Description = req.Description,
            Status = req.Status,
            Priority = req.Priority,
            DueDate = req.DueDate,
            StartDate = req.StartDate,
            EstimatedHours = req.EstimatedHours,
            ProjectId = req.ProjectId,
            AssigneeId = req.AssigneeId,
            ParentTaskId = req.ParentTaskId,
            Tags = req.Tags,
            CreatedBy = _currentUser.UserName
        };

        await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (req.AssigneeId.HasValue)
            await _notifications.SendToUserAsync(req.AssigneeId.Value, "Task Assigned", $"You were assigned: {task.Title}", cancellationToken: cancellationToken);

        return GetTasksQueryHandler.MapToDto(task);
    }
}

public class MoveTaskCommandHandler : IRequestHandler<MoveTaskCommand, TaskDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public MoveTaskCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<TaskDto> Handle(MoveTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(request.Request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException("Task not found.");

        task.Status = request.Request.NewStatus;
        task.Order = request.Request.NewOrder;
        task.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return GetTasksQueryHandler.MapToDto(task);
    }
}

public class GetCalendarTasksQueryHandler : IRequestHandler<GetCalendarTasksQuery, IReadOnlyList<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCalendarTasksQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<TaskDto>> Handle(GetCalendarTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _unitOfWork.Tasks.GetCalendarTasksAsync(request.From, request.To, request.ProjectId, cancellationToken);
        return tasks.Select(GetTasksQueryHandler.MapToDto).ToList();
    }
}

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notifications;

    public UpdateTaskCommandHandler(IUnitOfWork unitOfWork, INotificationService notifications)
    {
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var task = await _unitOfWork.Tasks.GetWithDetailsAsync(req.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Task not found.");

        var previousAssignee = task.AssigneeId;
        task.Title = req.Title;
        task.Description = req.Description;
        task.Status = req.Status;
        task.Priority = req.Priority;
        task.Order = req.Order;
        task.DueDate = req.DueDate;
        task.StartDate = req.StartDate;
        task.EstimatedHours = req.EstimatedHours;
        task.ActualHours = req.ActualHours;
        task.AssigneeId = req.AssigneeId;
        task.Tags = req.Tags;
        task.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (req.AssigneeId.HasValue && req.AssigneeId != previousAssignee)
            await _notifications.SendToUserAsync(req.AssigneeId.Value, "Task Assigned", $"You were assigned: {task.Title}", cancellationToken: cancellationToken);

        return GetTasksQueryHandler.MapToDto(task);
    }
}

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Task not found.");

        task.IsDeleted = true;
        task.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
