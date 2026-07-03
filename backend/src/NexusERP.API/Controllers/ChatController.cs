using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Application.DTOs.Chat;
using NexusERP.Application.Features.Chat.Commands;

namespace NexusERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator) => _mediator = mediator;

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] SendChatMessageRequest request)
    {
        var result = await _mediator.Send(new SendChatMessageCommand(request));
        return Ok(result);
    }
}
