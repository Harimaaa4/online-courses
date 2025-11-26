using FluentValidation;
using online_courses.Domain;

namespace online_courses.Domain.Validators
{
    public class CourseValidator : AbstractValidator<Course>
    {
        public CourseValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Название курса обязательно")
                .MaximumLength(100).WithMessage("Название слишком длинное");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Укажите автора курса");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Цена не может быть отрицательной");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Описание не может быть пустым")
                .MinimumLength(10).WithMessage("Описание должно содержать хотя бы 10 символов");

            // Можно добавить проверку рейтинга, если нужно
            RuleFor(x => x.Rating)
                .InclusiveBetween(0, 5).WithMessage("Рейтинг должен быть от 0 до 5");
        }
    }
}