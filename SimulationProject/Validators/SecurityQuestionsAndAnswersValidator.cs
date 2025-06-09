using FluentValidation;
using SimulationProject.DTO.UserDTOs;

namespace SimulationProject.Validators
{
    public class SecurityQuestionsAndAnswersValidator : AbstractValidator<SecurityQuestionsAndAnswersDTO>
    {
        public SecurityQuestionsAndAnswersValidator()
        {
            RuleFor(p => p.Securityanswer)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull();

            RuleFor(p => p.Securityquestion)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull();
        }
    }
}
