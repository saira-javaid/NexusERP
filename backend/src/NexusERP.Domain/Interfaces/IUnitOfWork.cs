namespace NexusERP.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }
    ITaskRepository Tasks { get; }
    ICommentRepository Comments { get; }
    IFileRepository Files { get; }
    INotificationRepository Notifications { get; }
    IAuditLogRepository AuditLogs { get; }
    IAppSettingRepository Settings { get; }
    IPermissionRepository Permissions { get; }
    IRoleRepository Roles { get; }
    IMeetingRepository Meetings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
