using FluentValidation;
using SimulationProject.DTO.UserDTOs;

namespace SimulationProject.Validators
{
    public class QuestionsUpadateValidator: AbstractValidator<QuestionsUpdateDTO>
    {
        public QuestionsUpadateValidator()
        {
            RuleFor(p => p.Securityquestion)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .NotEqual(p => p.Securityquestion1).WithMessage("The selected {PropertyName} is allready used.")
                .NotEqual(p => p.Securityquestion2).WithMessage("The selected {PropertyName} is allready used.");

            RuleFor(p => p.Securityquestion1)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .NotEqual(p => p.Securityquestion).WithMessage("The selected {PropertyName} is allready used.")
                .NotEqual(p => p.Securityquestion2).WithMessage("The selected {PropertyName} is allready used.");

            RuleFor(p => p.Securityquestion2)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .NotEqual(p => p.Securityquestion).WithMessage("The selected {PropertyName} is allready used.")
                .NotEqual(p => p.Securityquestion1).WithMessage("The selected {PropertyName} is allready used.");

            RuleFor(p => p.Securityanswer)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .NotEqual(p => p.Securityanswer1).WithMessage("The selected {PropertyName} is allready used.")
                .NotEqual(p => p.Securityanswer2).WithMessage("The selected {PropertyName} is allready used.");

            RuleFor(p => p.Securityanswer1)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .NotEqual(p => p.Securityanswer).WithMessage("The selected {PropertyName} is allready used.")
                .NotEqual(p => p.Securityanswer2).WithMessage("The selected {PropertyName} is allready used.");

            RuleFor(p => p.Securityanswer2)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .NotEqual(p => p.Securityanswer).WithMessage("The selected {PropertyName} is allready used.")
                .NotEqual(p => p.Securityanswer1).WithMessage("The selected {PropertyName} is allready used.");
        }
    }
}
