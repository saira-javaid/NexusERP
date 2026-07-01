using FluentAssertions;
using NexusERP.Application.DTOs.Projects;
using NexusERP.Application.Features.Auth.Commands;
using NexusERP.Application.Features.Auth.Validators;
using NexusERP.Application.Features.Projects.Commands;
using NexusERP.Application.Features.Projects.Validators;
using NexusERP.Domain.Enums;
using Xunit;

namespace NexusERP.Tests.Validators;

public class AuthValidatorTests
{
    [Fact]
    public void LoginCommand_WithValidData_ShouldPass()
    {
        var validator = new LoginCommandValidator();
        var result = validator.Validate(new LoginCommand("admin@nexuserp.com", "Admin@123", null));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LoginCommand_WithInvalidEmail_ShouldFail()
    {
        var validator = new LoginCommandValidator();
        var result = validator.Validate(new LoginCommand("invalid", "password", null));
        result.IsValid.Should().BeFalse();
    }
}

public class ProjectValidatorTests
{
    [Fact]
    public void CreateProject_WithEmptyName_ShouldFail()
    {
        var validator = new CreateProjectCommandValidator();
        var result = validator.Validate(new CreateProjectCommand(
            new CreateProjectRequest("", null, "PRJ-001", ProjectStatus.Active, null, null, 0, null)));
        result.IsValid.Should().BeFalse();
    }
}
