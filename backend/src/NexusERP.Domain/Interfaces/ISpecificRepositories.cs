using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using TaskStatus = NexusERP.Domain.Enums.TaskStatus;

namespace NexusERP.Domain.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, ProjectStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<Project?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ITaskRepository : IRepository<TaskItem>
{
    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPagedAsync(
        Guid? projectId, int page, int pageSize, TaskStatus? status = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetByProjectForKanbanAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaskItem>> GetCalendarTasksAsync(DateTime from, DateTime to, Guid? projectId = null, CancellationToken cancellationToken = default);
    Task<TaskItem?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ICommentRepository : IRepository<Comment>
{
    Task<IReadOnlyList<Comment>> GetByTaskAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comment>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}

public interface IFileRepository : IRepository<ProjectFile>
{
    Task<IReadOnlyList<ProjectFile>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken = default);
}

public interface INotificationRepository : IRepository<Notification>
{
    Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetByUserPagedAsync(
        Guid userId, int page, int pageSize, bool? unreadOnly = null, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Notification?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? entityType = null, CancellationToken cancellationToken = default);
}

public interface IAppSettingRepository : IRepository<AppSetting>
{
    Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
}

public interface IPermissionRepository : IRepository<Permission>
{
    Task<IReadOnlyList<Permission>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SetRolePermissionsAsync(Guid roleId, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default);
    Task RemoveRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
}

public interface IRoleRepository
{
    Task<Dictionary<Guid, int>> GetUserCountsAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetPermissionCountsAsync(CancellationToken cancellationToken = default);
    Task<bool> HasUsersAsync(Guid roleId, CancellationToken cancellationToken = default);
}

public interface IMeetingRepository : IRepository<Meeting>
{
    Task<(IReadOnlyList<Meeting> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, MeetingStatus? status = null,
        DateTime? from = null, DateTime? to = null, Guid? organizerId = null,
        CancellationToken cancellationToken = default);
    Task<Meeting?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
