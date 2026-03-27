using FluentValidation;
using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;

namespace HRManagement.Api.Application.Validators;

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
            .NotEmpty().WithMessage("Employment department is required.")
            .MaximumLength(100).WithMessage("Employment department must not exceed 100 characters.");
        
        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Employment position is required.")
            .MaximumLength(50).WithMessage("Employment position must not exceed 50 characters.");
        
<<<<<<< HEAD
        RuleFor(x => x.SupervisorName)
            .NotEmpty().WithMessage("Supervisor name is required.")
            .MaximumLength(100).WithMessage("Supervisor name must not exceed 100 characters.");

        RuleFor(x => x.EmployeeDisplayId)
            .NotEmpty().WithMessage("Employee ID is required.")
            .Matches(@"^E\d{6}$").WithMessage("Employee ID must be in the format EXXXXXX (e.g., E150529).");
=======
        RuleFor(x => x.EmployeeDisplayId)
            .Matches(@"^E\d+$").WithMessage("Employee ID must be in the format EXXX (e.g., E001, E0001).")
            .When(x => !string.IsNullOrEmpty(x.EmployeeDisplayId));

        RuleFor(x => x.SupervisorDisplayId)
            .Matches(@"^E\d+$").WithMessage("Supervisor ID must be in the format EXXX (e.g., E001, E0001).")
            .When(x => !string.IsNullOrEmpty(x.SupervisorDisplayId));
>>>>>>> 395b5fe2d1c34e45da356467deda1ee05746ab6a
    }
}
