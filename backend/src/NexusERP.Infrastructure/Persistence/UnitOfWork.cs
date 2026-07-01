using NexusERP.Domain.Interfaces;
using NexusERP.Infrastructure.Persistence.Repositories;

namespace NexusERP.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Projects = new ProjectRepository(context);
        Tasks = new TaskRepository(context);
        Comments = new CommentRepository(context);
        Files = new FileRepository(context);
        Notifications = new NotificationRepository(context);
        AuditLogs = new AuditLogRepository(context);
        Settings = new AppSettingRepository(context);
        Permissions = new PermissionRepository(context);
        Roles = new RoleRepository(context);
        Meetings = new MeetingRepository(context);
    }

    public IProjectRepository Projects { get; }
    public ITaskRepository Tasks { get; }
    public ICommentRepository Comments { get; }
    public IFileRepository Files { get; }
    public INotificationRepository Notifications { get; }
    public IAuditLogRepository AuditLogs { get; }
    public IAppSettingRepository Settings { get; }
    public IPermissionRepository Permissions { get; }
    public IRoleRepository Roles { get; }
    public IMeetingRepository Meetings { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
