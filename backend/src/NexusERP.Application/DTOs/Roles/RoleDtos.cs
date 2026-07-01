namespace NexusERP.Application.DTOs.Roles;

public record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    int UserCount,
    int PermissionCount,
    DateTime CreatedAt);

public record RoleDetailDto(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<Guid> PermissionIds,
    DateTime CreatedAt);

public record PermissionDto(
    Guid Id,
    string Name,
    string Module,
    string? Description);

public record CreateRoleRequest(string Name, string? Description);

public record UpdateRoleRequest(Guid Id, string Name, string? Description);

public record UpdateRolePermissionsRequest(IReadOnlyList<Guid> PermissionIds);
