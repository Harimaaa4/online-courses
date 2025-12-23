using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace online_courses.Models
{
    public class CourseViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Выберите категорию")]
        [Display(Name = "Категория")]
        public Guid CategoryId { get; set; }

        [Required(ErrorMessage = "Введите название")]
        [Display(Name = "Название курса")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Укажите автора")]
        [Display(Name = "Автор")]
        public string Author { get; set; }

        [Required(ErrorMessage = "Добавьте описание")]
        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Укажите цену")]
        [Display(Name = "Цена (руб.)")]
        public decimal Price { get; set; }

        // Картинка (файл для загрузки)
        [Display(Name = "Обложка курса")]
        public IFormFile ImageFile { get; set; }

        // Путь к картинке (для отображения)
        public string ImagePath { get; set; }

        public string Level { get; set; } // Уровень (Beginner, Middle...)
        public double Rating { get; set; } // Рейтинг (5.0, 4.5...)
    }
}