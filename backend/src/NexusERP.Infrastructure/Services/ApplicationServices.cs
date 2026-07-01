using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NexusERP.Application.Common.Interfaces;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Interfaces;
using NexusERP.Infrastructure.Hubs;

namespace NexusERP.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogAsync(string entityType, string? entityId, AuditAction action, object? oldValues = null, object? newValues = null, CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            UserId = _currentUser.UserId?.ToString() ?? "system",
            UserName = _currentUser.UserName ?? "system",
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString()
        };

        await _unitOfWork.AuditLogs.AddAsync(log, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationHubService _hubService;

    public NotificationService(IUnitOfWork unitOfWork, INotificationHubService hubService)
    {
        _unitOfWork = unitOfWork;
        _hubService = hubService;
    }

    public async Task SendToUserAsync(Guid userId, string title, string message, string? actionUrl = null, CancellationToken cancellationToken = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = NotificationType.Info,
            ActionUrl = actionUrl
        };

        await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _hubService.SendToUserAsync(userId, notification);
    }

    public async Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string message, CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds)
            await SendToUserAsync(userId, title, message, cancellationToken: cancellationToken);
    }
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _basePath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
    {
        var uniqueName = $"{Guid.NewGuid()}_{fileName}";
        var path = Path.Combine(_basePath, uniqueName);
        await using var fs = File.Create(path);
        await fileStream.CopyToAsync(fs, cancellationToken);
        return uniqueName;
    }

    public Task<Stream> GetFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_basePath, storagePath);
        if (!File.Exists(path)) throw new FileNotFoundException();
        return Task.FromResult<Stream>(File.OpenRead(path));
    }

    public Task DeleteFileAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_basePath, storagePath);
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }
}
