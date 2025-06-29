using FluentValidation;
using SimulationProject.DTO.UserDTOs;
using SimulationProject.Services;

namespace SimulationProject.Validators
{
    public class PasswordResetValidator : AbstractValidator<PasswordReset>
    {
        private readonly IUsersService _usersService;
        public PasswordResetValidator(IUsersService usersService)
        {
            _usersService = usersService;

            RuleFor(p => p.UserName)
                .NotEmpty()
                .NotEmpty().WithMessage("{PropertyName} is required.");

            RuleFor(p => p.TempPassword)
                .NotEmpty()
                .NotEmpty().WithMessage("{PropertyName} is required.");


            RuleFor(p => p.NewPassword)
                .NotEmpty()
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .Must(_usersService.PasswordValid).WithMessage("Invalid {PropertyName}. {PropertyName} must be at least 10 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

            RuleFor(p => p.ConfirmPassword)
                .NotEmpty()
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .Equal(p => p.NewPassword).WithMessage("Confirm password and new password do not match")
                .When(p => !String.IsNullOrEmpty(p.NewPassword) && !String.IsNullOrEmpty(p.ConfirmPassword));

        }
    }
}
