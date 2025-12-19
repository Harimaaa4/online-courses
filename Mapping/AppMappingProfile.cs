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

            // Связь: Домен <-> База (User)
            CreateMap<User, UserDb>().ReverseMap();

            CreateMap<RegisterViewModel, ConfirmEmailViewModel>().ReverseMap();
            CreateMap<User, ConfirmEmailViewModel>().ReverseMap();

            // =========================
            //       КАТЕГОРИИ
            // =========================
            // Связь: База (CategoryDb) <-> Домен (Category)
            // При чтении из базы считаем количество курсов
            CreateMap<CategoryDb, Category>()
                .ForMember(dest => dest.CourseCount, opt => opt.MapFrom(src => src.Courses != null ? src.Courses.Count : 0))
                .ReverseMap();

            // Связь: Домен (Category) <-> Представление (CategoryViewModel)
            CreateMap<Category, CategoryViewModel>().ReverseMap();

            // =========================
            //         КУРСЫ
            // =========================
            // 1. Связь: Домен <-> База (Course)
            CreateMap<Course, CourseDb>().ReverseMap();

            // 2. Связь: Домен <-> Представление (CourseViewModel)
            CreateMap<Course, CourseViewModel>().ReverseMap();

            // =========================
            //        КОРЗИНА
            // =========================
            // 1. Связь: База <-> Домен
            CreateMap<CartDb, Cart>().ReverseMap();

            // 2. Связь: Домен (Cart) -> Представление (CartViewModel)
            // Здесь мы "вытаскиваем" данные из вложенного объекта Course в плоскую модель
            CreateMap<Cart, CartViewModel>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Course.Description)) // Добавили Description
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Course.Price))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Course.Image));
        }
    }
}