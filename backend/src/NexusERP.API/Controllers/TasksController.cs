using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.API.Authorization;
using NexusERP.Application.Common;
using NexusERP.Application.DTOs.Tasks;
using NexusERP.Application.Features.Tasks.Commands;
using NexusERP.Application.Features.Tasks.Queries;
using NexusERP.Domain.Enums;
using TaskStatus = NexusERP.Domain.Enums.TaskStatus;

namespace NexusERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [RequirePermission("tasks.view")]
    public async Task<IActionResult> GetAll([FromQuery] Guid? projectId, [FromQuery] int page = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize, [FromQuery] TaskStatus? status = null)
    {
        var (p, ps) = Pagination.Normalize(page, pageSize);
        var result = await _mediator.Send(new GetTasksQuery(projectId, p, ps, status));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("tasks.view")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetTaskByIdQuery(id));
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("kanban/{projectId:guid}")]
    [RequirePermission("tasks.view")]
    public async Task<IActionResult> GetKanban(Guid projectId)
    {
        var result = await _mediator.Send(new GetKanbanBoardQuery(projectId));
        return Ok(result);
    }

    [HttpGet("calendar")]
    [RequirePermission("tasks.view")]
    public async Task<IActionResult> GetCalendar([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] Guid? projectId)
    {
        var result = await _mediator.Send(new GetCalendarTasksQuery(from, to, projectId));
        return Ok(result);
    }

    [HttpPost]
    [RequirePermission("tasks.create")]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var result = await _mediator.Send(new CreateTaskCommand(request));
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("tasks.edit")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request)
    {
        if (id != request.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(new UpdateTaskCommand(request));
        return Ok(result);
    }

    [HttpPatch("move")]
    [RequirePermission("tasks.edit")]
    public async Task<IActionResult> Move([FromBody] MoveTaskRequest request)
    {
        var result = await _mediator.Send(new MoveTaskCommand(request));
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("tasks.delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteTaskCommand(id));
        return NoContent();
    }
}
