using MediatR;
using NexusERP.Application.DTOs.Roles;

namespace NexusERP.Application.Features.Roles.Commands;

public record CreateRoleCommand(CreateRoleRequest Request) : IRequest<RoleDetailDto>;

public record UpdateRoleCommand(UpdateRoleRequest Request) : IRequest<RoleDetailDto>;

public record UpdateRolePermissionsCommand(Guid RoleId, UpdateRolePermissionsRequest Request) : IRequest<RoleDetailDto>;

public record DeleteRoleCommand(Guid Id) : IRequest<Unit>;
