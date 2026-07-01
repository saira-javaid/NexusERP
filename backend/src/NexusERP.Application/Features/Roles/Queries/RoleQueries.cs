using MediatR;
using NexusERP.Application.Common;
using NexusERP.Application.DTOs.Roles;

namespace NexusERP.Application.Features.Roles.Queries;

public record GetRolesQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<PagedResult<RoleDto>>;

public record GetRoleByIdQuery(Guid Id) : IRequest<RoleDetailDto?>;

public record GetPermissionsQuery() : IRequest<IReadOnlyList<PermissionDto>>;
