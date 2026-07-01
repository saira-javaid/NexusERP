using MediatR;
using NexusERP.Application.Common;
using NexusERP.Application.DTOs.Users;

namespace NexusERP.Application.Features.Users.Queries;

public record GetUsersQuery(int Page = 1, int PageSize = 20, string? Search = null, bool? IsActive = null)
    : IRequest<PagedResult<UserListDto>>;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDetailDto?>;
