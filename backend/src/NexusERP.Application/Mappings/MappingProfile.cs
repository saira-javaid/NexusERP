using AutoMapper;
using NexusERP.Application.DTOs.Auth;
using NexusERP.Application.DTOs.Projects;
using NexusERP.Application.DTOs.Tasks;
using NexusERP.Domain.Entities;

namespace NexusERP.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ApplicationUser, UserDto>()
            .ForCtorParam(nameof(UserDto.FullName), opt => opt.MapFrom(s => s.FullName))
            .ForCtorParam(nameof(UserDto.Roles), opt => opt.MapFrom(_ => Array.Empty<string>()))
            .ForCtorParam(nameof(UserDto.Permissions), opt => opt.MapFrom(_ => Array.Empty<string>()));

        CreateMap<Project, ProjectDto>()
            .ForCtorParam(nameof(ProjectDto.ManagerName), opt => opt.MapFrom(s => s.Manager != null ? s.Manager.FullName : null))
            .ForCtorParam(nameof(ProjectDto.TaskCount), opt => opt.MapFrom(s => s.Tasks.Count))
            .ForCtorParam(nameof(ProjectDto.MemberCount), opt => opt.MapFrom(s => s.Members.Count));

        CreateMap<TaskItem, TaskDto>()
            .ForCtorParam(nameof(TaskDto.ProjectName), opt => opt.MapFrom(s => s.Project != null ? s.Project.Name : ""))
            .ForCtorParam(nameof(TaskDto.AssigneeName), opt => opt.MapFrom(s => s.Assignee != null ? s.Assignee.FullName : null));
    }
}
