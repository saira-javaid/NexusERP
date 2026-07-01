using MediatR;
using Microsoft.AspNetCore.Identity;
using NexusERP.Application.Common;
using NexusERP.Application.Common.Interfaces;
using NexusERP.Application.DTOs.Roles;
using NexusERP.Application.Features.Roles.Commands;
using NexusERP.Application.Features.Roles.Queries;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Interfaces;

namespace NexusERP.Application.Features.Roles.Handlers;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, PagedResult<RoleDto>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;

    public GetRolesQueryHandler(RoleManager<ApplicationRole> roleManager, IUnitOfWork unitOfWork)
    {
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var query = _roleManager.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(r => r.Name!.Contains(search) || (r.Description != null && r.Description.Contains(search)));
        }

        var total = query.Count();
        var roles = query.OrderBy(r => r.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var userCounts = await _unitOfWork.Roles.GetUserCountsAsync(cancellationToken);
        var permCounts = await _unitOfWork.Roles.GetPermissionCountsAsync(cancellationToken);

        var items = roles.Select(r => new RoleDto(
            r.Id, r.Name!, r.Description,
            userCounts.GetValueOrDefault(r.Id),
            permCounts.GetValueOrDefault(r.Id),
            r.CreatedAt)).ToList();

        return new PagedResult<RoleDto>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDetailDto?>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;

    public GetRoleByIdQueryHandler(RoleManager<ApplicationRole> roleManager, IUnitOfWork unitOfWork)
    {
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<RoleDetailDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id.ToString());
        if (role == null) return null;

        var permissions = await _unitOfWork.Permissions.GetByRoleAsync(role.Id, cancellationToken);
        return new RoleDetailDto(
            role.Id, role.Name!, role.Description,
            permissions.Select(p => p.Name).ToList(),
            permissions.Select(p => p.Id).ToList(),
            role.CreatedAt);
    }
}

public class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPermissionsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IReadOnlyList<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await _unitOfWork.Permissions.GetAllAsync(cancellationToken);
        return permissions
            .OrderBy(p => p.Module).ThenBy(p => p.Name)
            .Select(p => new PermissionDto(p.Id, p.Name, p.Module, p.Description))
            .ToList();
    }
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleDetailDto>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IAuditService _audit;

    public CreateRoleCommandHandler(RoleManager<ApplicationRole> roleManager, IAuditService audit)
    {
        _roleManager = roleManager;
        _audit = audit;
    }

    public async Task<RoleDetailDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        if (await _roleManager.RoleExistsAsync(req.Name))
            throw new InvalidOperationException($"Role '{req.Name}' already exists.");

        var role = new ApplicationRole { Name = req.Name, Description = req.Description };
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _audit.LogAsync(nameof(ApplicationRole), role.Id.ToString(), AuditAction.Create,
            newValues: req, cancellationToken: cancellationToken);

        return new RoleDetailDto(role.Id, role.Name!, role.Description, [], [], role.CreatedAt);
    }
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDetailDto>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;

    private static readonly HashSet<string> SystemRoles = ["Admin", "Manager", "Member", "Viewer"];

    public UpdateRoleCommandHandler(
        RoleManager<ApplicationRole> roleManager,
        IUnitOfWork unitOfWork,
        IAuditService audit)
    {
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<RoleDetailDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var role = await _roleManager.FindByIdAsync(req.Id.ToString())
            ?? throw new KeyNotFoundException($"Role {req.Id} not found.");

        if (SystemRoles.Contains(role.Name!) && role.Name != req.Name)
            throw new InvalidOperationException("System role names cannot be changed.");

        var oldValues = new { role.Name, role.Description };
        role.Name = req.Name;
        role.Description = req.Description;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _audit.LogAsync(nameof(ApplicationRole), role.Id.ToString(), AuditAction.Update,
            oldValues: oldValues, newValues: req, cancellationToken: cancellationToken);

        var permissions = await _unitOfWork.Permissions.GetByRoleAsync(role.Id, cancellationToken);
        return new RoleDetailDto(
            role.Id, role.Name!, role.Description,
            permissions.Select(p => p.Name).ToList(),
            permissions.Select(p => p.Id).ToList(),
            role.CreatedAt);
    }
}

public class UpdateRolePermissionsCommandHandler : IRequestHandler<UpdateRolePermissionsCommand, RoleDetailDto>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;

    public UpdateRolePermissionsCommandHandler(
        RoleManager<ApplicationRole> roleManager,
        IUnitOfWork unitOfWork,
        IAuditService audit)
    {
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<RoleDetailDto> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.RoleId.ToString())
            ?? throw new KeyNotFoundException($"Role {request.RoleId} not found.");

        await _unitOfWork.Permissions.SetRolePermissionsAsync(role.Id, request.Request.PermissionIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(nameof(ApplicationRole), role.Id.ToString(), AuditAction.Update,
            newValues: new { Permissions = request.Request.PermissionIds }, cancellationToken: cancellationToken);

        var permissions = await _unitOfWork.Permissions.GetByRoleAsync(role.Id, cancellationToken);
        return new RoleDetailDto(
            role.Id, role.Name!, role.Description,
            permissions.Select(p => p.Name).ToList(),
            permissions.Select(p => p.Id).ToList(),
            role.CreatedAt);
    }
}

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Unit>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;

    private static readonly HashSet<string> SystemRoles = ["Admin", "Manager", "Member", "Viewer"];

    public DeleteRoleCommandHandler(
        RoleManager<ApplicationRole> roleManager,
        IUnitOfWork unitOfWork,
        IAuditService audit)
    {
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleManager.FindByIdAsync(request.Id.ToString())
            ?? throw new KeyNotFoundException($"Role {request.Id} not found.");

        if (SystemRoles.Contains(role.Name!))
            throw new InvalidOperationException("System roles cannot be deleted.");

        if (await _unitOfWork.Roles.HasUsersAsync(role.Id, cancellationToken))
            throw new InvalidOperationException("Cannot delete a role that is assigned to users.");

        await _unitOfWork.Permissions.RemoveRolePermissionsAsync(role.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await _audit.LogAsync(nameof(ApplicationRole), role.Id.ToString(), AuditAction.Delete,
            oldValues: new { role.Name }, cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
