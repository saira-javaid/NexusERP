using MediatR;
using NexusERP.Application.Common;
using NexusERP.Application.Common.Interfaces;
using NexusERP.Application.DTOs.Projects;
using NexusERP.Application.Features.Projects.Commands;
using NexusERP.Application.Features.Projects.Queries;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Interfaces;

namespace NexusERP.Application.Features.Projects.Handlers;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, PagedResult<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedResult<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _unitOfWork.Projects.GetPagedAsync(
            request.Page, request.PageSize, request.Search, request.Status, cancellationToken);

        var dtos = items.Select(p => new ProjectDto(
            p.Id, p.Name, p.Description, p.Code, p.Status,
            p.StartDate, p.EndDate, p.Budget, p.ManagerId,
            p.Manager?.FullName, p.Tasks.Count, p.Members.Count, p.CreatedAt)).ToList();

        return new PagedResult<ProjectDto>
        {
            Items = dtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDetailDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ProjectDetailDto?> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetWithDetailsAsync(request.Id, cancellationToken);
        if (project == null) return null;

        return new ProjectDetailDto(
            project.Id, project.Name, project.Description, project.Code, project.Status,
            project.StartDate, project.EndDate, project.Budget, project.ManagerId, project.Manager?.FullName,
            project.Members.Select(m => new ProjectMemberDto(m.UserId, m.User.FullName, m.User.Email!, m.Role, m.JoinedAt)).ToList(),
            project.CreatedAt);
    }
}

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public CreateProjectCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IAuditService audit)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var project = new Project
        {
            Name = req.Name,
            Description = req.Description,
            Code = req.Code,
            Status = req.Status,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            Budget = req.Budget,
            ManagerId = req.ManagerId,
            CreatedBy = _currentUser.UserName
        };

        await _unitOfWork.Projects.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(nameof(Project), project.Id.ToString(), AuditAction.Create, newValues: project, cancellationToken: cancellationToken);

        return new ProjectDto(project.Id, project.Name, project.Description, project.Code, project.Status,
            project.StartDate, project.EndDate, project.Budget, project.ManagerId, null, 0, 0, project.CreatedAt);
    }
}

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public UpdateProjectCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IAuditService audit)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var project = await _unitOfWork.Projects.GetByIdAsync(req.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {req.Id} not found.");

        var oldValues = new { project.Name, project.Status, project.Budget };
        project.Name = req.Name;
        project.Description = req.Description;
        project.Code = req.Code;
        project.Status = req.Status;
        project.StartDate = req.StartDate;
        project.EndDate = req.EndDate;
        project.Budget = req.Budget;
        project.ManagerId = req.ManagerId;
        project.UpdatedAt = DateTime.UtcNow;
        project.UpdatedBy = _currentUser.UserName;

        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(nameof(Project), project.Id.ToString(), AuditAction.Update, oldValues, project, cancellationToken);

        return new ProjectDto(project.Id, project.Name, project.Description, project.Code, project.Status,
            project.StartDate, project.EndDate, project.Budget, project.ManagerId, null, 0, 0, project.CreatedAt);
    }
}

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;

    public DeleteProjectCommandHandler(IUnitOfWork unitOfWork, IAuditService audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Unit> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Project {request.Id} not found.");

        project.IsDeleted = true;
        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(nameof(Project), project.Id.ToString(), AuditAction.Delete, cancellationToken: cancellationToken);
        return Unit.Value;
    }
}
