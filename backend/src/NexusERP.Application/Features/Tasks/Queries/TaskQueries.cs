using MediatR;
using NexusERP.Application.Common;
using NexusERP.Application.DTOs.Tasks;
using NexusERP.Domain.Enums;
using TaskStatus = NexusERP.Domain.Enums.TaskStatus;

namespace NexusERP.Application.Features.Tasks.Queries;

public record GetTasksQuery(Guid? ProjectId, int Page = 1, int PageSize = 20, TaskStatus? Status = null)
    : IRequest<PagedResult<TaskDto>>;

public record GetKanbanBoardQuery(Guid ProjectId) : IRequest<IReadOnlyList<KanbanColumnDto>>;

public record GetCalendarTasksQuery(DateTime From, DateTime To, Guid? ProjectId = null)
    : IRequest<IReadOnlyList<TaskDto>>;

public record GetTaskByIdQuery(Guid Id) : IRequest<TaskDto?>;
