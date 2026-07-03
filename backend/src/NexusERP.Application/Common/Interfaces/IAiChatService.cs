using NexusERP.Application.DTOs.Chat;

namespace NexusERP.Application.Common.Interfaces;

public interface IAiChatService
{
    Task<ChatMessageResponse> SendMessageAsync(SendChatMessageRequest request, CancellationToken cancellationToken = default);
}
