using FluentValidation;
using SimulationProject.DTO.UserDTOs;
using SimulationProject.Services;

namespace SimulationProject.Validators
{
    public class PasswordUpdateValidator : AbstractValidator<PasswordUpdate>
    {
        private readonly IUsersService _usersService;
        public PasswordUpdateValidator(IUsersService usersService)
        {
            _usersService = usersService;

            RuleFor(p => p.UserName)
                .NotEmpty()
                .NotEmpty().WithMessage("{PropertyName} is required.");

            RuleFor(p => p.OldPassword)
                .NotEmpty()
                .NotEmpty().WithMessage("{PropertyName} is required.");


            RuleFor(p => p.NewPassword)
                .NotEmpty()
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .Must(_usersService.PasswordValid).WithMessage("Invalid {PropertyName}. {PropertyName} must be at least 10 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.");

            RuleFor(p => p.ConfirmPassword)
                .NotEmpty()
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .Matches(p => p.ConfirmPassword).WithMessage("Confirm password and new pawwsord do not match");
        }
    }
}
