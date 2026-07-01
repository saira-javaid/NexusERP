using MediatR;
using NexusERP.Application.Common;
using NexusERP.Application.DTOs.Projects;
using NexusERP.Domain.Enums;

namespace NexusERP.Application.Features.Projects.Queries;

public record GetProjectsQuery(int Page = 1, int PageSize = 20, string? Search = null, ProjectStatus? Status = null)
    : IRequest<PagedResult<ProjectDto>>;

public record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDetailDto?>;
