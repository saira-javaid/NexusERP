using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusERP.Application.Common;
using NexusERP.Application.Common.Interfaces;
using NexusERP.Domain.Interfaces;

namespace NexusERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var projectCount = await _unitOfWork.Projects.CountAsync(cancellationToken: cancellationToken);
        var taskCount = await _unitOfWork.Tasks.CountAsync(cancellationToken: cancellationToken);
        var unreadNotifications = _currentUser.UserId.HasValue
            ? await _unitOfWork.Notifications.GetUnreadCountAsync(_currentUser.UserId.Value, cancellationToken)
            : 0;

        return Ok(new
        {
            totalProjects = projectCount,
            totalTasks = taskCount,
            unreadNotifications,
            tasksByStatus = new
            {
                todo = await _unitOfWork.Tasks.CountAsync(t => t.Status == Domain.Enums.TaskStatus.Todo, cancellationToken),
                inProgress = await _unitOfWork.Tasks.CountAsync(t => t.Status == Domain.Enums.TaskStatus.InProgress, cancellationToken),
                done = await _unitOfWork.Tasks.CountAsync(t => t.Status == Domain.Enums.TaskStatus.Done, cancellationToken)
            }
        });
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = Pagination.DefaultPageSize, [FromQuery] bool? unreadOnly = null)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        var (p, ps) = Pagination.Normalize(page, pageSize);
        var result = await _unitOfWork.Notifications.GetByUserPagedAsync(_currentUser.UserId.Value, p, ps, unreadOnly);
        return Ok(Pagination.ToPagedResult(result.Items, result.TotalCount, p, ps));
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        var count = await _unitOfWork.Notifications.GetUnreadCountAsync(_currentUser.UserId.Value);
        return Ok(new { count });
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        await _unitOfWork.Notifications.MarkAsReadAsync(id, _currentUser.UserId.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue) return Unauthorized();
        await _unitOfWork.Notifications.MarkAllAsReadAsync(_currentUser.UserId.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = Pagination.DefaultPageSize, [FromQuery] string? entityType = null)
    {
        var (p, ps) = Pagination.Normalize(page, pageSize);
        var result = await _unitOfWork.AuditLogs.GetPagedAsync(p, ps, entityType);
        return Ok(Pagination.ToPagedResult(result.Items, result.TotalCount, p, ps));
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly ICurrentUserService _currentUser;

    public FilesController(IUnitOfWork unitOfWork, IFileStorageService fileStorage, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _currentUser = currentUser;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(52_428_800)]
    public async Task<IActionResult> Upload(IFormFile file, [FromQuery] Guid? projectId, [FromQuery] Guid? taskId, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return BadRequest("No file provided");
        if (!_currentUser.UserId.HasValue) return Unauthorized();

        await using var stream = file.OpenReadStream();
        var path = await _fileStorage.SaveFileAsync(stream, file.FileName, file.ContentType, cancellationToken);

        var projectFile = new Domain.Entities.ProjectFile
        {
            FileName = path,
            OriginalFileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            StoragePath = path,
            ProjectId = projectId,
            TaskId = taskId,
            UploadedById = _currentUser.UserId.Value,
            CreatedBy = _currentUser.UserName
        };

        await _unitOfWork.Files.AddAsync(projectFile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new { projectFile.Id, projectFile.OriginalFileName, projectFile.FileSize });
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var file = await _unitOfWork.Files.GetByIdAsync(id, cancellationToken);
        if (file == null) return NotFound();

        var stream = await _fileStorage.GetFileAsync(file.StoragePath, cancellationToken);
        return File(stream, file.ContentType, file.OriginalFileName);
    }
}
