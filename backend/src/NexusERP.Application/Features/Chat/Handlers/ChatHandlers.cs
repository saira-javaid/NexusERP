using MediatR;
using NexusERP.Application.Common.Interfaces;
using NexusERP.Application.DTOs.Chat;
using NexusERP.Application.Features.Chat.Commands;

namespace NexusERP.Application.Features.Chat.Handlers;

public class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, ChatMessageResponse>
{
    private readonly IAiChatService _chatService;

    public SendChatMessageCommandHandler(IAiChatService chatService) => _chatService = chatService;

    public Task<ChatMessageResponse> Handle(SendChatMessageCommand request, CancellationToken cancellationToken) =>
        _chatService.SendMessageAsync(request.Request, cancellationToken);
}
