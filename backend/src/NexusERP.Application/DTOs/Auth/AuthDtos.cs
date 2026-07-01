namespace NexusERP.Application.DTOs.Auth;

public record LoginRequest(string Email, string Password);
public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserDto User);
public record RefreshTokenRequest(string AccessToken, string RefreshToken);
public record RegisterRequest(string Email, string Password, string FirstName, string LastName);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string? AvatarUrl,
    bool IsActive,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
