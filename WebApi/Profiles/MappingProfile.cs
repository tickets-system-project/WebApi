using AutoMapper;
using WebApi.Models.DTOs.User;
using WebApi.Models.DTOs.Visit;
using WebApi.Models.Entities;

namespace WebApi.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();

        CreateMap<CreateUserDto, User>();

        CreateMap<UpdateUserDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<VisitRequest, Client>();
        
        CreateMap<VisitRequest, Reservation>()
            .ForMember(dest => dest.Date, 
                opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.Time, 
                opt => opt.MapFrom(src => src.Time))
            .ForMember(dest => dest.ConfirmationCode, 
                opt => opt.MapFrom(_ => GenerateConfirmationCode()));

        CreateMap<Reservation, VisitResponse>()
            .ForMember(dest => dest.CaseCategoryID, 
                opt => opt.MapFrom(src => src.Category.ID));
        
        CreateMap<Reservation, VisitDetailsDto>()
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Client.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Client.LastName))
            .ForMember(dest => dest.PESEL, opt => opt.MapFrom(src => src.Client.PESEL))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Client.Email))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Client.Phone))
            .ForMember(dest => dest.CaseCategoryID, opt => opt.MapFrom(src => src.Category.ID))
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.Value))
            .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.Time.Value));
    }
    
    private static string GenerateConfirmationCode()
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, 8).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}