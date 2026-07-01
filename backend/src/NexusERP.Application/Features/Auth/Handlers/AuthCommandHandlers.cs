using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NexusERP.Application.Common.Interfaces;
using NexusERP.Application.DTOs.Auth;
using NexusERP.Application.Features.Auth.Commands;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Interfaces;

namespace NexusERP.Application.Features.Auth.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IDateTimeService _dateTime;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IDateTimeService dateTime)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _dateTime = dateTime;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated.");

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedAccessException("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _unitOfWork.Permissions.GetUserPermissionsAsync(user.Id, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = _dateTime.UtcNow.AddDays(refreshDays),
            CreatedByIp = request.IpAddress
        });
        user.LastLoginAt = _dateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var expiresMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15");
        var userDto = new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, user.FullName,
            user.AvatarUrl, user.IsActive, roles.ToList(), permissions);

        return new LoginResponse(accessToken, refreshToken, _dateTime.UtcNow.AddMinutes(expiresMinutes), userDto);
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IDateTimeService _dateTime;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IDateTimeService dateTime)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _dateTime = dateTime;
    }

    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = _tokenService.ValidateAccessToken(request.AccessToken);
        if (principal == null)
            throw new UnauthorizedAccessException("Invalid access token.");

        var user = await _userManager.FindByIdAsync(principal.Value.UserId.ToString())
            ?? throw new UnauthorizedAccessException("User not found.");

        var storedToken = user.RefreshTokens.SingleOrDefault(t => t.Token == request.RefreshToken && t.IsActive)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        storedToken.RevokedAt = _dateTime.UtcNow;
        storedToken.RevokedByIp = request.IpAddress;

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await _unitOfWork.Permissions.GetUserPermissionsAsync(user.Id, cancellationToken);
        var newAccessToken = _tokenService.GenerateAccessToken(user.Id, user.Email!, roles, permissions);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var refreshDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpiresAt = _dateTime.UtcNow.AddDays(refreshDays),
            CreatedByIp = request.IpAddress,
        });
        storedToken.ReplacedByToken = newRefreshToken;
        await _userManager.UpdateAsync(user);

        var expiresMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15");
        var userDto = new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, user.FullName,
            user.AvatarUrl, user.IsActive, roles.ToList(), permissions);

        return new LoginResponse(newAccessToken, newRefreshToken, _dateTime.UtcNow.AddMinutes(expiresMinutes), userDto);
    }
}
