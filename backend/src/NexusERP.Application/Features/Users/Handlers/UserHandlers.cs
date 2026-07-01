using MediatR;
using Microsoft.AspNetCore.Identity;
using NexusERP.Application.Common;
using NexusERP.Application.Common.Interfaces;
using NexusERP.Application.DTOs.Users;
using NexusERP.Application.Features.Users.Commands;
using NexusERP.Application.Features.Users.Queries;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Enums;
using NexusERP.Domain.Interfaces;

namespace NexusERP.Application.Features.Users.Handlers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserListDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUsersQueryHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<PagedResult<UserListDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(u =>
                (u.Email != null && u.Email.Contains(search)) ||
                u.FirstName.Contains(search) ||
                u.LastName.Contains(search));
        }

        if (request.IsActive.HasValue)
            query = query.Where(u => u.IsActive == request.IsActive.Value);

        var total = query.Count();
        var users = query.OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var items = new List<UserListDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new UserListDto(
                user.Id, user.Email!, user.FirstName, user.LastName, user.FullName,
                user.IsActive, user.LastLoginAt, roles.ToList(), user.CreatedAt));
        }

        return new PagedResult<UserListDto>
        {
            Items = items,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDetailDto?>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserDetailDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Id.ToString());
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _unitOfWork.Permissions.GetUserPermissionsAsync(user.Id, cancellationToken);

        return new UserDetailDto(
            user.Id, user.Email!, user.FirstName, user.LastName, user.FullName,
            user.AvatarUrl, user.IsActive, user.LastLoginAt,
            roles.ToList(), permissions, user.CreatedAt);
    }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDetailDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;

    public CreateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUnitOfWork unitOfWork,
        IAuditService audit)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<UserDetailDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var user = new ApplicationUser
        {
            Email = req.Email,
            UserName = req.Email,
            FirstName = req.FirstName,
            LastName = req.LastName,
            IsActive = req.IsActive,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var roles = req.Roles?.Count > 0 ? req.Roles : ["Member"];
        await AssignRolesAsync(user, roles);

        await _audit.LogAsync(nameof(ApplicationUser), user.Id.ToString(), AuditAction.Create,
            newValues: new { user.Email, user.FirstName, user.LastName, Roles = roles },
            cancellationToken: cancellationToken);

        var permissions = await _unitOfWork.Permissions.GetUserPermissionsAsync(user.Id, cancellationToken);
        return new UserDetailDto(
            user.Id, user.Email!, user.FirstName, user.LastName, user.FullName,
            user.AvatarUrl, user.IsActive, user.LastLoginAt,
            roles.ToList(), permissions, user.CreatedAt);
    }

    private async Task AssignRolesAsync(ApplicationUser user, IReadOnlyList<string> roles)
    {
        foreach (var roleName in roles)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                await _userManager.AddToRoleAsync(user, roleName);
        }
    }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDetailDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;

    public UpdateUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUnitOfWork unitOfWork,
        IAuditService audit)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<UserDetailDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        var user = await _userManager.FindByIdAsync(req.Id.ToString())
            ?? throw new KeyNotFoundException($"User {req.Id} not found.");

        var oldValues = new { user.Email, user.FirstName, user.LastName, user.IsActive };
        user.Email = req.Email;
        user.UserName = req.Email;
        user.FirstName = req.FirstName;
        user.LastName = req.LastName;
        user.IsActive = req.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new InvalidOperationException(string.Join(", ", updateResult.Errors.Select(e => e.Description)));

        if (req.Roles != null)
            await SyncRolesAsync(user, req.Roles);

        await _audit.LogAsync(nameof(ApplicationUser), user.Id.ToString(), AuditAction.Update,
            oldValues: oldValues, newValues: req, cancellationToken: cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _unitOfWork.Permissions.GetUserPermissionsAsync(user.Id, cancellationToken);

        return new UserDetailDto(
            user.Id, user.Email!, user.FirstName, user.LastName, user.FullName,
            user.AvatarUrl, user.IsActive, user.LastLoginAt,
            roles.ToList(), permissions, user.CreatedAt);
    }

    private async Task SyncRolesAsync(ApplicationUser user, IReadOnlyList<string> newRoles)
    {
        var currentRoles = await _userManager.GetRolesAsync(user);
        var toRemove = currentRoles.Except(newRoles).ToList();
        var toAdd = newRoles.Except(currentRoles).ToList();

        if (toRemove.Count > 0)
            await _userManager.RemoveFromRolesAsync(user, toRemove);

        foreach (var role in toAdd)
        {
            if (await _roleManager.RoleExistsAsync(role))
                await _userManager.AddToRoleAsync(user, role);
        }
    }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditService _audit;

    public DeleteUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        ICurrentUserService currentUser,
        IAuditService audit)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _audit = audit;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId == request.Id)
            throw new InvalidOperationException("You cannot deactivate your own account.");

        var user = await _userManager.FindByIdAsync(request.Id.ToString())
            ?? throw new KeyNotFoundException($"User {request.Id} not found.");

        user.IsActive = false;
        await _userManager.UpdateAsync(user);

        await _audit.LogAsync(nameof(ApplicationUser), user.Id.ToString(), AuditAction.Delete,
            oldValues: new { user.Email, IsActive = true },
            cancellationToken: cancellationToken);

        return Unit.Value;
    }
}
