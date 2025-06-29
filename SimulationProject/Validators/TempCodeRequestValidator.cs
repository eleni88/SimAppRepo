using FluentValidation;
using SimulationProject.DTO.UserDTOs;

namespace SimulationProject.Validators
{
    public class TempCodeRequestValidator: AbstractValidator<TempcodeRequestDTO>
    {
        public TempCodeRequestValidator()
        {
            RuleFor(p => p.username)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull();
        }
    }
}
