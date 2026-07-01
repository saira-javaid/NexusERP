using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.API.Authorization;
using NexusERP.Application.Common;
using NexusERP.Application.DTOs.Meetings;
using NexusERP.Application.Features.Meetings.Commands;
using NexusERP.Application.Features.Meetings.Queries;
using NexusERP.Domain.Enums;

namespace NexusERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeetingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public MeetingsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission("meetings.view")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        [FromQuery] string? search = null,
        [FromQuery] MeetingStatus? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] Guid? organizerId = null)
    {
        var (p, ps) = Pagination.Normalize(page, pageSize);
        var result = await _mediator.Send(new GetMeetingsQuery(p, ps, search, status, from, to, organizerId));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("meetings.view")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetMeetingByIdQuery(id));
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission("meetings.create")]
    public async Task<IActionResult> Create([FromBody] CreateMeetingRequest request)
    {
        var result = await _mediator.Send(new CreateMeetingCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("meetings.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMeetingRequest request)
    {
        if (id != request.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(new UpdateMeetingCommand(request));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("meetings.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteMeetingCommand(id));
        return NoContent();
    }
}
