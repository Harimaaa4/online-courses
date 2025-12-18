using FluentValidation;
using online_courses.Domain;

namespace online_courses.Domain.Validators
{
    public class CategoryValidator : AbstractValidator<Category>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Название категории не может быть пустым")
                .Length(2, 50).WithMessage("Название должно быть от 2 до 50 символов");

        }
    }
}