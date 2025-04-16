using AutoMapper;
using WebApi.Models.DTOs.User;
using WebApi.Models.Entities;

namespace WebApi.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, 
                opt => opt.MapFrom(src => src.Role.Name));

        CreateMap<CreateUserDto, User>();

        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}