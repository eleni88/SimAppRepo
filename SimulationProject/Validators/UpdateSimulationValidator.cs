using FluentValidation;
using SimulationProject.DTO.SimulationDTOs;
using SimulationProject.Services;

namespace SimulationProject.Validators
{
    public class UpdateSimulationValidator: AbstractValidator<UpdateSimulationDTO>
    {
        private readonly ISimulationService _simulationService;
        public UpdateSimulationValidator(ISimulationService simulationService)
        {
            _simulationService = simulationService;

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must be fewer than 200 characters.");

            RuleFor(p => p.Description)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must be fewer than 200 characters.");

            RuleFor(p => p.Codeurl)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(100).WithMessage("{PropertyName} must be fewer than 500 characters.")
                .Must(_simulationService.IsValidUrl).WithMessage("Invalid {PropertyName}");

            RuleFor(p => p.Simparams)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .Must(_simulationService.IsValidJson).WithMessage("{PropertyName} is not a valid JSON");

        }
    }
}
