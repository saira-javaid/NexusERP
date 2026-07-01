using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.API.Authorization;
using NexusERP.Application.Common;
using NexusERP.Application.DTOs.Roles;
using NexusERP.Application.Features.Roles.Commands;
using NexusERP.Application.Features.Roles.Queries;

namespace NexusERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission("roles.view")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        [FromQuery] string? search = null)
    {
        var (p, ps) = Pagination.Normalize(page, pageSize);
        var result = await _mediator.Send(new GetRolesQuery(p, ps, search));
        return Ok(result);
    }

    [HttpGet("permissions")]
    [RequirePermission("roles.view")]
    public async Task<IActionResult> GetPermissions()
    {
        var result = await _mediator.Send(new GetPermissionsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("roles.view")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetRoleByIdQuery(id));
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission("roles.manage")]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var result = await _mediator.Send(new CreateRoleCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("roles.manage")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request)
    {
        if (id != request.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(new UpdateRoleCommand(request));
        return Ok(result);
    }

    [HttpPut("{id:guid}/permissions")]
    [RequirePermission("roles.manage")]
    public async Task<IActionResult> UpdatePermissions(Guid id, [FromBody] UpdateRolePermissionsRequest request)
    {
        var result = await _mediator.Send(new UpdateRolePermissionsCommand(id, request));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("roles.manage")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteRoleCommand(id));
        return NoContent();
    }
}
