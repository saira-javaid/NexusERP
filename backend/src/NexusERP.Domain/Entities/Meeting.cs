using NexusERP.Domain.Common;
using NexusERP.Domain.Enums;

namespace NexusERP.Domain.Entities;

public class Meeting : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public MeetingStatus Status { get; set; } = MeetingStatus.Scheduled;
    public Guid OrganizerId { get; set; }
    public Guid? ProjectId { get; set; }

    public ApplicationUser Organizer { get; set; } = null!;
    public Project? Project { get; set; }
    public ICollection<MeetingAttendee> Attendees { get; set; } = [];
}

public class MeetingAttendee
{
    public Guid MeetingId { get; set; }
    public Guid UserId { get; set; }
    public MeetingAttendeeRole Role { get; set; } = MeetingAttendeeRole.Required;
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;

    public Meeting Meeting { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}
