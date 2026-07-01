namespace NexusERP.Application.DTOs.Users;

public record UserListDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    bool IsActive,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt);

public record UserDetailDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? AvatarUrl,
    bool IsActive,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions,
    DateTime CreatedAt);

public record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    IReadOnlyList<string>? Roles = null,
    bool IsActive = true);

public record UpdateUserRequest(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    IReadOnlyList<string>? Roles = null);
