using System;
using AutoMapper;
using online_courses.Domain;
using online_courses.Entities;
using online_courses.Models; // Тут лежат LoginViewModel и RegisterViewModel

namespace online_courses
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            // Превращение: RegisterViewModel <-> User (Domain)
            CreateMap<RegisterViewModel, User>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "User")) // По умолчанию роль User
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Превращение: LoginViewModel <-> User (Domain)
            CreateMap<LoginViewModel, User>();

            // Превращение: User (Domain) <-> UserDb (База данных)
            CreateMap<User, UserDb>().ReverseMap();
        }
    }
}