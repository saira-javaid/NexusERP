using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.API.Authorization;
using NexusERP.Application.Common;
using NexusERP.Application.DTOs.Users;
using NexusERP.Application.Features.Users.Commands;
using NexusERP.Application.Features.Users.Queries;

namespace NexusERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission("users.view")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var (p, ps) = Pagination.Normalize(page, pageSize);
        var result = await _mediator.Send(new GetUsersQuery(p, ps, search, isActive));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("users.view")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id));
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission("users.manage")]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await _mediator.Send(new CreateUserCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("users.manage")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        if (id != request.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(new UpdateUserCommand(request));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("users.manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteUserCommand(id));
        return NoContent();
    }
}
