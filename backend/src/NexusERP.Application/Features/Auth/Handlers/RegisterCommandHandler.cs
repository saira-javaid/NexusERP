using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using NexusERP.Application.Common.Interfaces;
using NexusERP.Application.DTOs.Auth;
using NexusERP.Application.Features.Auth.Commands;
using NexusERP.Domain.Entities;
using NexusERP.Domain.Interfaces;

namespace NexusERP.Application.Features.Auth.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, UserDto>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterCommandHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<UserDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = await CreateMemberUserAsync(request.Email, request.Password, request.FirstName, request.LastName);
        return new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, user.FullName,
            user.AvatarUrl, user.IsActive, ["Member"], []);
    }

    internal static async Task<ApplicationUser> CreateMemberUserAsync(
        UserManager<ApplicationUser> userManager,
        string email, string password, string firstName, string lastName)
    {
        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, "Member");
        return user;
    }

    private Task<ApplicationUser> CreateMemberUserAsync(string email, string password, string firstName, string lastName) =>
        CreateMemberUserAsync(_userManager, email, password, firstName, lastName);
}

public class SignupCommandHandler : IRequestHandler<SignupCommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IDateTimeService _dateTime;

    public SignupCommandHandler(
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

    public async Task<LoginResponse> Handle(SignupCommand request, CancellationToken cancellationToken)
    {
        var user = await RegisterCommandHandler.CreateMemberUserAsync(
            _userManager, request.Email, request.Password, request.FirstName, request.LastName);

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

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Unit>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDateTimeService _dateTime;

    public RevokeTokenCommandHandler(UserManager<ApplicationUser> userManager, IDateTimeService dateTime)
    {
        _userManager = userManager;
        _dateTime = dateTime;
    }

    public async Task<Unit> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var user = _userManager.Users.FirstOrDefault(u => u.RefreshTokens.Any(t => t.Token == request.RefreshToken));
        if (user != null)
        {
            var token = user.RefreshTokens.First(t => t.Token == request.RefreshToken);
            token.RevokedAt = _dateTime.UtcNow;
            token.RevokedByIp = request.IpAddress;
            await _userManager.UpdateAsync(user);
        }
        return Unit.Value;
    }
}
