using MediatR;
using NexusERP.Application.DTOs.Projects;

namespace NexusERP.Application.Features.Projects.Commands;

public record CreateProjectCommand(CreateProjectRequest Request) : IRequest<ProjectDto>;
public record UpdateProjectCommand(UpdateProjectRequest Request) : IRequest<ProjectDto>;
public record DeleteProjectCommand(Guid Id) : IRequest<Unit>;
