using System.ComponentModel.DataAnnotations;

namespace online_courses.Models
{
    public class RegisterViewModel
    {
        // В методичке это "Login", что логично
        [Required(ErrorMessage = "Укажите имя 3-20 символов")]
        [MaxLength(20, ErrorMessage = "Имя должно иметь длину менье 20 символов")]
        [MinLength(3, ErrorMessage = "Имя должно иметь длину более 3 символов")]
        public string Login { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный адрес электронной почты")]
        [Required(ErrorMessage = "Укажите почту")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Укажите пароль")]
        [MinLength(6, ErrorMessage = "Пароль должен иметь длину больше 6 символов")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string PasswordConfirm { get; set; }
    }
}