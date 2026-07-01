using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.API.Authorization;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Interfaces;
using TaskStatus = NexusERP.Domain.Enums.TaskStatus;

namespace NexusERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet("overview")]
    [RequirePermission("reports.view")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
    {
        var (projects, totalProjects) = await _unitOfWork.Projects.GetPagedAsync(1, 100, cancellationToken: cancellationToken);

        var projectsByStatus = Enum.GetValues<ProjectStatus>()
            .Select(status => new
            {
                status = (int)status,
                label = status.ToString(),
                count = projects.Count(p => p.Status == status)
            })
            .Where(x => x.count > 0)
            .ToList();

        var tasksByStatus = new
        {
            todo = await _unitOfWork.Tasks.CountAsync(t => t.Status == TaskStatus.Todo, cancellationToken),
            inProgress = await _unitOfWork.Tasks.CountAsync(t => t.Status == TaskStatus.InProgress, cancellationToken),
            inReview = await _unitOfWork.Tasks.CountAsync(t => t.Status == TaskStatus.InReview, cancellationToken),
            done = await _unitOfWork.Tasks.CountAsync(t => t.Status == TaskStatus.Done, cancellationToken),
            backlog = await _unitOfWork.Tasks.CountAsync(t => t.Status == TaskStatus.Backlog, cancellationToken),
        };

        var projectRows = projects
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.Code,
                p.Name,
                status = (int)p.Status,
                p.Budget,
                taskCount = p.Tasks.Count,
                managerName = p.Manager?.FullName
            })
            .ToList();

        return Ok(new
        {
            summary = new
            {
                totalProjects,
                totalTasks = await _unitOfWork.Tasks.CountAsync(cancellationToken: cancellationToken),
                activeProjects = projects.Count(p => p.Status == ProjectStatus.Active),
                completedProjects = projects.Count(p => p.Status == ProjectStatus.Completed),
                totalBudget = projects.Sum(p => p.Budget)
            },
            projectsByStatus,
            tasksByStatus,
            projects = projectRows
        });
    }
}
