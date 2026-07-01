using NexusERP.Domain.Enums;

namespace NexusERP.Application.DTOs.Meetings;

public record MeetingDto(
    Guid Id,
    string Title,
    string? Description,
    string? Location,
    DateTime StartAt,
    DateTime EndAt,
    MeetingStatus Status,
    Guid OrganizerId,
    string OrganizerName,
    Guid? ProjectId,
    string? ProjectName,
    int AttendeeCount,
    DateTime CreatedAt);

public record MeetingAttendeeDto(
    Guid UserId,
    string FullName,
    string Email,
    MeetingAttendeeRole Role);

public record MeetingDetailDto(
    Guid Id,
    string Title,
    string? Description,
    string? Location,
    DateTime StartAt,
    DateTime EndAt,
    MeetingStatus Status,
    Guid OrganizerId,
    string OrganizerName,
    Guid? ProjectId,
    string? ProjectName,
    IReadOnlyList<MeetingAttendeeDto> Attendees,
    DateTime CreatedAt);

public record MeetingAttendeeInput(Guid UserId, MeetingAttendeeRole Role = MeetingAttendeeRole.Required);

public record CreateMeetingRequest(
    string Title,
    string? Description,
    string? Location,
    DateTime StartAt,
    DateTime EndAt,
    MeetingStatus Status,
    Guid? ProjectId,
    IReadOnlyList<MeetingAttendeeInput> Attendees);

public record UpdateMeetingRequest(
    Guid Id,
    string Title,
    string? Description,
    string? Location,
    DateTime StartAt,
    DateTime EndAt,
    MeetingStatus Status,
    Guid? ProjectId,
    IReadOnlyList<MeetingAttendeeInput> Attendees);
