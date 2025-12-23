using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace online_courses.Models
{
    public class CategoryViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Название обязательно")]
        [Display(Name = "Название категории")]
        public string Name { get; set; }

        [Display(Name = "Картинка категории")]
        public IFormFile ImageFile { get; set; }

        public string ImagePath { get; set; }

        // Добавили это поле, чтобы таблица в админке не ругалась
        public int CourseCount { get; set; }
    }
}