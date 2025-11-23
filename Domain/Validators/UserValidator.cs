using FluentValidation;
using online_courses.Domain;

namespace online_courses.Domain.Validators
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("Логин не может быть пустым")
                .Length(3, 20).WithMessage("Имя должно иметь длину от 3 до 20 символов");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Почта обязательна")
                .EmailAddress().WithMessage("Некорректный адрес электронной почты");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Пароль не может быть пустым")
                .MinimumLength(6).WithMessage("Пароль должен иметь длину более 6 символов");
        }
    }
}