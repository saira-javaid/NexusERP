using MediatR;
using NexusERP.Application.Common;
using NexusERP.Application.DTOs.Meetings;
using NexusERP.Domain.Enums;

namespace NexusERP.Application.Features.Meetings.Queries;

public record GetMeetingsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    MeetingStatus? Status = null,
    DateTime? From = null,
    DateTime? To = null,
    Guid? OrganizerId = null) : IRequest<PagedResult<MeetingDto>>;

public record GetMeetingByIdQuery(Guid Id) : IRequest<MeetingDetailDto?>;
