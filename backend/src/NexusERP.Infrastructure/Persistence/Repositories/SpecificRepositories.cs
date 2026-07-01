using Microsoft.EntityFrameworkCore;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Interfaces;
using NexusERP.Infrastructure.Persistence;
using TaskStatus = NexusERP.Domain.Enums.TaskStatus;

namespace NexusERP.Infrastructure.Persistence.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<Project> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, ProjectStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(p => p.Manager).Include(p => p.Tasks).Include(p => p.Members).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.Code.Contains(search));

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Project?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet.Include(p => p.Manager)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}

public class TaskRepository : Repository<TaskItem>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> GetPagedAsync(
        Guid? projectId, int page, int pageSize, TaskStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(t => t.Project).Include(t => t.Assignee).AsQueryable();

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);
        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(t => t.Order)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IReadOnlyList<TaskItem>> GetByProjectForKanbanAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        await DbSet.Include(t => t.Assignee).Include(t => t.Project)
            .Where(t => t.ProjectId == projectId).OrderBy(t => t.Order).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TaskItem>> GetCalendarTasksAsync(DateTime from, DateTime to, Guid? projectId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Include(t => t.Project).Include(t => t.Assignee)
            .Where(t => (t.DueDate >= from && t.DueDate <= to) || (t.StartDate >= from && t.StartDate <= to));

        if (projectId.HasValue)
            query = query.Where(t => t.ProjectId == projectId.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<TaskItem?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet.Include(t => t.Project).Include(t => t.Assignee)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
}

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Comment>> GetByTaskAsync(Guid taskId, CancellationToken cancellationToken = default) =>
        await DbSet.Include(c => c.Author).Where(c => c.TaskId == taskId).OrderBy(c => c.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Comment>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        await DbSet.Include(c => c.Author).Where(c => c.ProjectId == projectId).OrderBy(c => c.CreatedAt).ToListAsync(cancellationToken);
}

public class FileRepository : Repository<ProjectFile>, IFileRepository
{
    public FileRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ProjectFile>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken = default) =>
        await DbSet.Include(f => f.UploadedBy).Where(f => f.ProjectId == projectId).ToListAsync(cancellationToken);
}

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetByUserPagedAsync(
        Guid userId, int page, int pageSize, bool? unreadOnly = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(n => n.UserId == userId);
        if (unreadOnly == true)
            query = query.Where(n => !n.IsRead);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await DbSet.CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);

    public async Task<Notification?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, cancellationToken);

    public async Task MarkAsReadAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await GetByIdForUserAsync(id, userId, cancellationToken);
        if (notification == null || notification.IsRead) return;
        notification.IsRead = true;
        Update(notification);
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unread = await DbSet.Where(n => n.UserId == userId && !n.IsRead).ToListAsync(cancellationToken);
        foreach (var notification in unread)
            notification.IsRead = true;
    }
}

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? entityType = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, total);
    }
}

public class AppSettingRepository : Repository<AppSetting>, IAppSettingRepository
{
    public AppSettingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
}

public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Permission>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default) =>
        await Context.RolePermissions.Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.Permission).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await Context.UserRoles.Where(ur => ur.UserId == userId)
            .Join(Context.RolePermissions, ur => ur.RoleId, rp => rp.RoleId, (_, rp) => rp.PermissionId)
            .Join(Context.Permissions, pid => pid, p => p.Id, (_, p) => p.Name)
            .Distinct().ToListAsync(cancellationToken);

    public async Task SetRolePermissionsAsync(Guid roleId, IReadOnlyList<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        var existing = await Context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(cancellationToken);
        Context.RolePermissions.RemoveRange(existing);

        var validIds = await Context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        foreach (var permissionId in validIds)
            Context.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permissionId });
    }

    public async Task RemoveRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var existing = await Context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(cancellationToken);
        Context.RolePermissions.RemoveRange(existing);
    }
}

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context) => _context = context;

    public async Task<Dictionary<Guid, int>> GetUserCountsAsync(CancellationToken cancellationToken = default) =>
        await _context.UserRoles
            .GroupBy(ur => ur.RoleId)
            .Select(g => new { RoleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoleId, x => x.Count, cancellationToken);

    public async Task<Dictionary<Guid, int>> GetPermissionCountsAsync(CancellationToken cancellationToken = default) =>
        await _context.RolePermissions
            .GroupBy(rp => rp.RoleId)
            .Select(g => new { RoleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoleId, x => x.Count, cancellationToken);

    public async Task<bool> HasUsersAsync(Guid roleId, CancellationToken cancellationToken = default) =>
        await _context.UserRoles.AnyAsync(ur => ur.RoleId == roleId, cancellationToken);
}

public class MeetingRepository : Repository<Meeting>, IMeetingRepository
{
    public MeetingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<Meeting> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, MeetingStatus? status = null,
        DateTime? from = null, DateTime? to = null, Guid? organizerId = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(m => m.Organizer)
            .Include(m => m.Project)
            .Include(m => m.Attendees)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Title.Contains(search) || (m.Description != null && m.Description.Contains(search)));

        if (status.HasValue)
            query = query.Where(m => m.Status == status.Value);

        if (from.HasValue)
            query = query.Where(m => m.StartAt >= from.Value);

        if (to.HasValue)
            query = query.Where(m => m.StartAt <= to.Value);

        if (organizerId.HasValue)
            query = query.Where(m => m.OrganizerId == organizerId.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(m => m.StartAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Meeting?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(m => m.Organizer)
            .Include(m => m.Project)
            .Include(m => m.Attendees).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
}
