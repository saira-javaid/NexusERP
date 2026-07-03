using MediatR;
using NexusERP.Application.DTOs.Chat;

namespace NexusERP.Application.Features.Chat.Commands;

public record SendChatMessageCommand(SendChatMessageRequest Request) : IRequest<ChatMessageResponse>;
