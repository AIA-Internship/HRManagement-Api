using HRManagement.Domain.Interfaces;
using FluentValidation;
using HRManagement.Domain.Models.Payload.EmployeeDtos.Commands.Dto;

namespace HRManagement.Application.Validators;

public class CreateEmploymentInfoValidator : AbstractValidator<CreateEmploymentInfoDto>
{
    public CreateEmploymentInfoValidator()
    {
        RuleFor(x => x.EmploymentStatus)
            .NotNull().WithMessage("Please select a valid Employment status option.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Employment start date is required.");
        
        RuleFor(x => x.EmploymentType)
            .NotNull().WithMessage("Please select a valid Employment type option.");
        
        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Employment Department is required.")
            .MaximumLength(100).WithMessage("Employment Department must not exceed 100 characters.");
        
        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Employment Position is required.")
            .MaximumLength(50).WithMessage("Employment Position must not exceed 50 characters.");
        
        RuleFor(x => x.EmployeeDisplayId)
            .Matches(@"^E\d+$").WithMessage("Employee ID must be in the format EXXX (e.g., E001, E0001).")
            .When(x => !string.IsNullOrEmpty(x.EmployeeDisplayId));

        RuleFor(x => x.SupervisorDisplayId)
            .Matches(@"^E\d+$").WithMessage("Supervisor ID must be in the format EXXX (e.g., E001, E0001).")
            .When(x => !string.IsNullOrEmpty(x.SupervisorDisplayId));
    }
}







