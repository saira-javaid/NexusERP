using Microsoft.AspNetCore.SignalR;
using NexusERP.Domain.Entities;

namespace NexusERP.Infrastructure.Hubs;

public interface INotificationHubService
{
    Task SendToUserAsync(Guid userId, Notification notification);
}

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }
}

public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubService(IHubContext<NotificationHub> hubContext) => _hubContext = hubContext;

    public async Task SendToUserAsync(Guid userId, Notification notification) =>
        await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", new
        {
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Type,
            notification.ActionUrl,
            notification.CreatedAt
        });
}
