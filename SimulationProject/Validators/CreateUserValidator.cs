using FluentValidation;
using SimulationProject.DTO.UserDTOs;
using SimulationProject.Services;

namespace SimulationProject.Validators
{
    public class CreateUserValidator: AbstractValidator<CreateUserDTO>
    {
        private readonly IUsersService _usersService;
        public CreateUserValidator(IUsersService usersService)
        {
            _usersService = usersService;

            RuleFor(p => p.Firstname)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must be fewer than 100 characters.");

            RuleFor(p => p.Lastname)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must be fewer than 100 characters.");

            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .EmailAddress().WithMessage("{PropertyName} is not valid.");

            RuleFor(p => p.Username)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must be fewer than 50 characters.")
                .MinimumLength(5).WithMessage("{PropertyName} must be more than 5 characters.");

            RuleFor(p => p.Age)
                .InclusiveBetween(18, 99).WithMessage("{PropertyName} must be between 18 and 99.");

            RuleFor(p => p.Jobtitle)
                .MaximumLength(100).WithMessage("{PropertyName} must be fewer than 200 characters." );

            RuleFor(p => p.Organization)
                .MaximumLength(100).WithMessage("{PropertyName} must be fewer than 200 characters.");

            RuleFor(p => p.Password)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .Must(_usersService.PasswordValid).WithMessage("Invalid {PropertyName}. {PropertyName} must be at least 10 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
        }
    }
}
