using MediatR;
using NexusERP.Application.DTOs.Tasks;

namespace NexusERP.Application.Features.Tasks.Commands;

public record CreateTaskCommand(CreateTaskRequest Request) : IRequest<TaskDto>;
public record UpdateTaskCommand(UpdateTaskRequest Request) : IRequest<TaskDto>;
public record MoveTaskCommand(MoveTaskRequest Request) : IRequest<TaskDto>;
public record DeleteTaskCommand(Guid Id) : IRequest<Unit>;
