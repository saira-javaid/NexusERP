using MediatR;
using NexusERP.Application.DTOs.Users;

namespace NexusERP.Application.Features.Users.Commands;

public record CreateUserCommand(CreateUserRequest Request) : IRequest<UserDetailDto>;

public record UpdateUserCommand(UpdateUserRequest Request) : IRequest<UserDetailDto>;

public record DeleteUserCommand(Guid Id) : IRequest<Unit>;
