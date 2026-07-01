using MediatR;
using NexusERP.Application.Common;
using NexusERP.Application.Common.Interfaces;
using NexusERP.Application.DTOs.Meetings;
using NexusERP.Application.Features.Meetings.Commands;
using NexusERP.Application.Features.Meetings.Queries;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Interfaces;

namespace NexusERP.Application.Features.Meetings.Handlers;

public class GetMeetingsQueryHandler : IRequestHandler<GetMeetingsQuery, PagedResult<MeetingDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMeetingsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<MeetingDto>> Handle(GetMeetingsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _unitOfWork.Meetings.GetPagedAsync(
            request.Page, request.PageSize, request.Search, request.Status,
            request.From, request.To, request.OrganizerId, cancellationToken);

        return new PagedResult<MeetingDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    internal static MeetingDto MapToDto(Meeting m) => new(
        m.Id, m.Title, m.Description, m.Location, m.StartAt, m.EndAt, m.Status,
        m.OrganizerId, m.Organizer?.FullName ?? "", m.ProjectId, m.Project?.Name,
        m.Attendees.Count, m.CreatedAt);
}

public class GetMeetingByIdQueryHandler : IRequestHandler<GetMeetingByIdQuery, MeetingDetailDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMeetingByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<MeetingDetailDto?> Handle(GetMeetingByIdQuery request, CancellationToken cancellationToken)
    {
        var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(request.Id, cancellationToken);
        return meeting == null ? null : MapToDetailDto(meeting);
    }

    internal static MeetingDetailDto MapToDetailDto(Meeting m) => new(
        m.Id, m.Title, m.Description, m.Location, m.StartAt, m.EndAt, m.Status,
        m.OrganizerId, m.Organizer?.FullName ?? "", m.ProjectId, m.Project?.Name,
        m.Attendees.Select(a => new MeetingAttendeeDto(
            a.UserId, a.User?.FullName ?? "", a.User?.Email ?? "", a.Role)).ToList(),
        m.CreatedAt);
}

public class CreateMeetingCommandHandler : IRequestHandler<CreateMeetingCommand, MeetingDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly IAuditService _audit;

    public CreateMeetingCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        INotificationService notifications, IAuditService audit)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notifications = notifications;
        _audit = audit;
    }

    public async Task<MeetingDetailDto> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var organizerId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("User must be authenticated.");

        var meeting = new Meeting
        {
            Title = req.Title,
            Description = req.Description,
            Location = req.Location,
            StartAt = req.StartAt,
            EndAt = req.EndAt,
            Status = req.Status,
            OrganizerId = organizerId,
            ProjectId = req.ProjectId,
            CreatedBy = _currentUser.UserName
        };

        foreach (var attendee in req.Attendees)
        {
            meeting.Attendees.Add(new MeetingAttendee
            {
                UserId = attendee.UserId,
                Role = attendee.Role
            });
        }

        await _unitOfWork.Meetings.AddAsync(meeting, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(nameof(Meeting), meeting.Id.ToString(), AuditAction.Create,
            newValues: new { meeting.Title, meeting.StartAt, meeting.EndAt, meeting.Status, AttendeeCount = req.Attendees.Count },
            cancellationToken: cancellationToken);

        if (req.Attendees.Count > 0)
        {
            foreach (var userId in req.Attendees.Select(a => a.UserId).Distinct())
            {
                await _notifications.SendToUserAsync(
                    userId,
                    "Meeting Scheduled",
                    $"You are invited to: {meeting.Title} on {meeting.StartAt:g}",
                    $"/meetings/{meeting.Id}",
                    cancellationToken);
            }
        }

        var created = await _unitOfWork.Meetings.GetWithDetailsAsync(meeting.Id, cancellationToken)
            ?? meeting;
        return GetMeetingByIdQueryHandler.MapToDetailDto(created);
    }
}

public class UpdateMeetingCommandHandler : IRequestHandler<UpdateMeetingCommand, MeetingDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService _notifications;
    private readonly IAuditService _audit;

    public UpdateMeetingCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        INotificationService notifications, IAuditService audit)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notifications = notifications;
        _audit = audit;
    }

    public async Task<MeetingDetailDto> Handle(UpdateMeetingCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var meeting = await _unitOfWork.Meetings.GetWithDetailsAsync(req.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Meeting not found.");

        var previousAttendeeIds = meeting.Attendees.Select(a => a.UserId).ToHashSet();

        meeting.Title = req.Title;
        meeting.Description = req.Description;
        meeting.Location = req.Location;
        meeting.StartAt = req.StartAt;
        meeting.EndAt = req.EndAt;
        meeting.Status = req.Status;
        meeting.ProjectId = req.ProjectId;
        meeting.UpdatedAt = DateTime.UtcNow;
        meeting.UpdatedBy = _currentUser.UserName;

        meeting.Attendees.Clear();
        foreach (var attendee in req.Attendees)
        {
            meeting.Attendees.Add(new MeetingAttendee
            {
                MeetingId = meeting.Id,
                UserId = attendee.UserId,
                Role = attendee.Role
            });
        }

        _unitOfWork.Meetings.Update(meeting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(nameof(Meeting), meeting.Id.ToString(), AuditAction.Update, cancellationToken: cancellationToken);

        var newAttendeeIds = req.Attendees.Select(a => a.UserId).Where(id => !previousAttendeeIds.Contains(id)).Distinct().ToList();
        foreach (var userId in newAttendeeIds)
        {
            await _notifications.SendToUserAsync(
                userId,
                "Meeting Invitation",
                $"You are invited to: {meeting.Title} on {meeting.StartAt:g}",
                $"/meetings/{meeting.Id}",
                cancellationToken);
        }

        var updated = await _unitOfWork.Meetings.GetWithDetailsAsync(meeting.Id, cancellationToken)
            ?? meeting;
        return GetMeetingByIdQueryHandler.MapToDetailDto(updated);
    }
}

public class DeleteMeetingCommandHandler : IRequestHandler<DeleteMeetingCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;

    public DeleteMeetingCommandHandler(IUnitOfWork unitOfWork, IAuditService audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Unit> Handle(DeleteMeetingCommand request, CancellationToken cancellationToken)
    {
        var meeting = await _unitOfWork.Meetings.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Meeting not found.");

        meeting.IsDeleted = true;
        meeting.Status = MeetingStatus.Cancelled;
        _unitOfWork.Meetings.Update(meeting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(nameof(Meeting), meeting.Id.ToString(), AuditAction.Delete, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}
