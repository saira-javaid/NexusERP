using NexusERP.Domain.Enums;

namespace NexusERP.Application.DTOs.Projects;

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    string Code,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal Budget,
    Guid? ManagerId,
    string? ManagerName,
    int TaskCount,
    int MemberCount,
    DateTime CreatedAt);

public record ProjectDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string Code,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal Budget,
    Guid? ManagerId,
    string? ManagerName,
    IReadOnlyList<ProjectMemberDto> Members,
    DateTime CreatedAt);

public record ProjectMemberDto(Guid UserId, string FullName, string Email, string Role, DateTime JoinedAt);

public record CreateProjectRequest(
    string Name,
    string? Description,
    string Code,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal Budget,
    Guid? ManagerId);

public record UpdateProjectRequest(
    Guid Id,
    string Name,
    string? Description,
    string Code,
    ProjectStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    decimal Budget,
    Guid? ManagerId);
