using FluentValidation;
using SimulationProject.DTO.UserDTOs;

namespace SimulationProject.Validators
{
    public class QuestionsUpadateValidator: AbstractValidator<QuestionsUpdateDTO>
    {
        public QuestionsUpadateValidator()
        {
            RuleFor(p => p.Securityquestion)
                .NotEqual(p => p.Securityquestion1).WithMessage("The selected {PropertyName} is allready used.")
                .When(p => !String.IsNullOrEmpty(p.Securityquestion) && !String.IsNullOrEmpty(p.Securityquestion1));

            RuleFor(p => p.Securityquestion)
                .NotEqual(p => p.Securityquestion2).WithMessage("The selected {PropertyName} is allready used.")
                .When(p => !String.IsNullOrEmpty(p.Securityquestion) && !String.IsNullOrEmpty(p.Securityquestion2));

            // ----------------------------------------------------------
            RuleFor(p => p.Securityquestion1)
                .NotEqual(p => p.Securityquestion).WithMessage("The selected {PropertyName} is allready used.")
                .When(p => !String.IsNullOrEmpty(p.Securityquestion1) && !String.IsNullOrEmpty(p.Securityquestion));

            RuleFor(p => p.Securityquestion1)
                .NotEqual(p => p.Securityquestion2).WithMessage("The selected {PropertyName} is allready used.")
                .When(p => !String.IsNullOrEmpty(p.Securityquestion1) && !String.IsNullOrEmpty(p.Securityquestion2));

            // ----------------------------------------------------------
            RuleFor(p => p.Securityquestion2)
                .NotEqual(p => p.Securityquestion).WithMessage("The selected {PropertyName} is allready used.")
                .When(p => !String.IsNullOrEmpty(p.Securityquestion2) && !String.IsNullOrEmpty(p.Securityquestion));

            RuleFor(p => p.Securityquestion2)
                .NotEqual(p => p.Securityquestion1).WithMessage("The selected {PropertyName} is allready used.")
                .When(p => !String.IsNullOrEmpty(p.Securityquestion2) && !String.IsNullOrEmpty(p.Securityquestion1));

            // ----------------------------------------------------------
            RuleFor(p => p.Securityanswer)
                .NotEqual(p => p.Securityanswer1).WithMessage("The selected {PropertyName} is allready used.")
                .When(p => !String.IsNullOrEmpty(p.Securityanswer) && !String.IsNullOrEmpty(p.Securityanswer1));

            RuleFor(p => p.Securityanswer)
                .NotEqual(p => p.Securityanswer2).WithMessage("The selected {PropertyName} is allready used.")
                .When(p => !String.IsNullOrEmpty(p.Securityanswer) && !String.IsNullOrEmpty(p.Securityanswer2));
            // ----------------------------------------------------------
            RuleFor(p => p.Securityanswer1)
                .NotEqual(p => p.Securityanswer).WithMessage("The selected {PropertyName} is allready used.")
                .When(p => !String.IsNullOrEmpty(p.Securityanswer1) && !String.IsNullOrEmpty(p.Securityanswer));

            RuleFor(p => p.Securityanswer1)
                .NotEqual(p => p.Securityanswer2).WithMessage("The selected {PropertyName} is allready used.")
                .When(p => !String.IsNullOrEmpty(p.Securityanswer1) && !String.IsNullOrEmpty(p.Securityanswer2));
            // ----------------------------------------------------------
            RuleFor(p => p.Securityanswer2)
                .NotEqual(p => p.Securityanswer).WithMessage("The selected {PropertyName} is already used.")
                .When(p => !String.IsNullOrEmpty(p.Securityanswer2) && !String.IsNullOrEmpty(p.Securityanswer));

            RuleFor(p => p.Securityanswer2)
                .NotEqual(p => p.Securityanswer1).WithMessage("The selected {PropertyName} is already used.")
                .When(p => !String.IsNullOrEmpty(p.Securityanswer2) && !String.IsNullOrEmpty(p.Securityanswer1));
        }
    }
}
