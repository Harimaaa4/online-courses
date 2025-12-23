using AutoMapper;
using online_courses.Domain;
using online_courses.Entities;
using online_courses.Models;
using System;

namespace online_courses.Mapping
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            // =========================
            //      ПОЛЬЗОВАТЕЛИ
            // =========================
            CreateMap<RegisterViewModel, User>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "User"))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<LoginViewModel, User>();

            CreateMap<User, UserDb>().ReverseMap();
            CreateMap<RegisterViewModel, ConfirmEmailViewModel>().ReverseMap();
            CreateMap<User, ConfirmEmailViewModel>().ReverseMap();

            // =========================
            //        КАТЕГОРИИ
            // =========================
            CreateMap<CategoryDb, Category>()
                .ForMember(dest => dest.CourseCount, opt => opt.MapFrom(src => src.Courses != null ? src.Courses.Count : 0))
                .ReverseMap();

            CreateMap<Category, CategoryViewModel>().ReverseMap();

            // Маппинг для админки (напрямую из БД в ViewModel)
            CreateMap<CategoryDb, CategoryViewModel>()
                 .ForMember(dest => dest.CourseCount, opt => opt.MapFrom(src => src.CourseCount)) // или src.Courses.Count
                 .ReverseMap();

            // =========================
            //          КУРСЫ
            // =========================
            // 1. Связь: Домен <-> База
            CreateMap<Course, CourseDb>().ReverseMap();

            // 2. Связь: Домен <-> ViewModel
            CreateMap<Course, CourseViewModel>()
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.Image))
                .ReverseMap();

            // 3. !!! ВАЖНОЕ ИСПРАВЛЕНИЕ ДЛЯ АДМИНКИ !!!
            // Связь: База (CourseDb) <-> ViewModel (CourseViewModel)
            // Это нужно для AdminController/EditCourse
            CreateMap<CourseDb, CourseViewModel>()
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.Image))
                .ReverseMap();

            // =========================
            //         КОРЗИНА
            // =========================
            CreateMap<CartDb, Cart>().ReverseMap();

            CreateMap<Cart, CartViewModel>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Course.Description))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Course.Price))
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.Course.Image));
        }
    }
}