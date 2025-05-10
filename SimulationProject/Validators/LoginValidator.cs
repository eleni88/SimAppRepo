using FluentValidation;
using SimulationProject.DTO.UserDTOs;

namespace SimulationProject.Validators
{
    public class LoginValidator : AbstractValidator<LoginForm>
    {
        public LoginValidator()
        {
            RuleFor(p => p.UserName)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull();

            RuleFor(p => p.Password)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull();
        }
    }
}
