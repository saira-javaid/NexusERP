using MediatR;
using NexusERP.Application.DTOs.Auth;

namespace NexusERP.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password, string? IpAddress) : IRequest<LoginResponse>;
public record RefreshTokenCommand(string AccessToken, string RefreshToken, string? IpAddress) : IRequest<LoginResponse>;
public record RevokeTokenCommand(string RefreshToken, string? IpAddress) : IRequest<Unit>;
public record RegisterCommand(string Email, string Password, string FirstName, string LastName) : IRequest<UserDto>;
public record SignupCommand(string Email, string Password, string FirstName, string LastName, string? IpAddress) : IRequest<LoginResponse>;
