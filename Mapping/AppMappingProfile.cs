using System;
using AutoMapper;
using online_courses.Domain;
using online_courses.Entities;
using online_courses.Models;

namespace online_courses
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            // --- ПОЛЬЗОВАТЕЛИ ---
            CreateMap<RegisterViewModel, User>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "User"))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<LoginViewModel, User>();

            // Связь: Домен <-> База (User)
            CreateMap<User, UserDb>().ReverseMap();

            CreateMap<RegisterViewModel, ConfirmEmailViewModel>().ReverseMap();
            CreateMap<User, ConfirmEmailViewModel>().ReverseMap();

            // --- КАТЕГОРИИ ---
            // Связь: Домен <-> База (Category)
            CreateMap<CategoryDb, Category>()
                .ForMember(dest => dest.CourseCount, opt => opt.MapFrom(src => src.Courses != null ? src.Courses.Count : 0))
                .ReverseMap();
            // Связь: Домен <-> Представление (CategoryViewModel)
            CreateMap<Category, CategoryViewModel>().ReverseMap();

            // --- КУРСЫ (ИСПРАВЛЕНИЕ) ---

            // 1. Связь: Домен <-> База (Course). ЭТОЙ СТРОКИ НЕ ХВАТАЛО!
            CreateMap<Course, CourseDb>().ReverseMap();

            // 2. Связь: Домен <-> Представление (CourseViewModel)
            CreateMap<online_courses.Domain.Course, CourseViewModel>();
            CreateMap<online_courses.Domain.Category, CategoryViewModel>();
        }
    }
}