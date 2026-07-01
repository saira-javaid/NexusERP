using MediatR;
using NexusERP.Application.DTOs.Meetings;

namespace NexusERP.Application.Features.Meetings.Commands;

public record CreateMeetingCommand(CreateMeetingRequest Request) : IRequest<MeetingDetailDto>;
public record UpdateMeetingCommand(UpdateMeetingRequest Request) : IRequest<MeetingDetailDto>;
public record DeleteMeetingCommand(Guid Id) : IRequest<Unit>;
