using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.API.Authorization;
using NexusERP.Application.Common;
using NexusERP.Application.DTOs.Projects;
using NexusERP.Application.Features.Projects.Commands;
using NexusERP.Application.Features.Projects.Queries;
using NexusERP.Domain.Enums;

namespace NexusERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission("projects.view")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = Pagination.DefaultPageSize,
        [FromQuery] string? search = null, [FromQuery] ProjectStatus? status = null)
    {
        var (p, ps) = Pagination.Normalize(page, pageSize);
        var result = await _mediator.Send(new GetProjectsQuery(p, ps, search, status));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("projects.view")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProjectByIdQuery(id));
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [RequirePermission("projects.create")]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var result = await _mediator.Send(new CreateProjectCommand(request));
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("projects.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request)
    {
        if (id != request.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(new UpdateProjectCommand(request));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("projects.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteProjectCommand(id));
        return NoContent();
    }
}
