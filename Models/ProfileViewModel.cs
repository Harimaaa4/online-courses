using Microsoft.AspNetCore.Http; // <--- ОБЯЗАТЕЛЬНО
using System.ComponentModel.DataAnnotations;

namespace online_courses.Models
{
    public class ProfileViewModel
    {
        public string Login { get; set; }

        public string Email { get; set; }

        [Display(Name = "Ссылка на аватарку")]
        public string AvatarUrl { get; set; }

        // Поле для загрузки файла
        [Display(Name = "Загрузить фото с устройства")]
        public IFormFile AvatarFile { get; set; }
    }
}