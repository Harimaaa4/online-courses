using System.ComponentModel.DataAnnotations;

namespace online_courses.Models
{
    public class ConfirmEmailViewModel
    {
        [Required(ErrorMessage = "Введите код")]
        public string CodeConfirm { get; set; } // Тот код, что введет пользователь

        public string GeneratedCode { get; set; } // Правильный код (придет с сервера)

        // Данные пользователя, чтобы сохранить их после проверки
        public string Login { get; set; }
        [Required]
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
    }
}