namespace NexusERP.Domain.Enums;

public enum ProjectStatus
{
    Planning = 0,
    Active = 1,
    OnHold = 2,
    Completed = 3,
    Cancelled = 4
}

public enum TaskStatus
{
    Backlog = 0,
    Todo = 1,
    InProgress = 2,
    InReview = 3,
    Done = 4,
    Cancelled = 5
}

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum NotificationType
{
    Info = 0,
    Success = 1,
    Warning = 2,
    Error = 3,
    TaskAssigned = 10,
    TaskUpdated = 11,
    CommentAdded = 12,
    ProjectUpdated = 13,
    MeetingScheduled = 14
}

public enum AuditAction
{
    Create = 0,
    Update = 1,
    Delete = 2,
    Login = 3,
    Logout = 4,
    Export = 5
}

public enum FileCategory
{
    Document = 0,
    Image = 1,
    Archive = 2,
    Other = 3
}

public enum MeetingStatus
{
    Scheduled = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

public enum MeetingAttendeeRole
{
    Organizer = 0,
    Required = 1,
    Optional = 2
}
