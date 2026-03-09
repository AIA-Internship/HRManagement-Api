using FluentValidation;
using HRManagement.Api.Application.Commands;
using HRManagement.Api.Application.Interfaces;

namespace HRManagement.Api.Application.Validators;

public class UpdateEmploymentInfoValidator : AbstractValidator<UpdateEmployeeInfoCommand>
{
    public UpdateEmploymentInfoValidator()
    {
        RuleFor(x => x.RequestDto.EmploymentStatus)
            .NotNull().WithMessage("Please select a valid employment status.")
            .When(x => x.RequestDto.EmploymentStatus.HasValue);

        RuleFor(x => x.RequestDto.StartDate)
            .NotEmpty().WithMessage("Start date is required.")
            .When(x => x.RequestDto.StartDate.HasValue);

        RuleFor(x => x.RequestDto.EmploymentType)
            .NotNull().WithMessage("Please select a valid employment type.")
            .When(x => x.RequestDto.EmploymentType.HasValue);

        RuleFor(x => x.RequestDto.Department)
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters.")
            .When(x => x.RequestDto.Department != null);

        RuleFor(x => x.RequestDto.Position)
            .MaximumLength(50).WithMessage("Position cannot exceed 50 characters.")
            .When(x => x.RequestDto.Position != null);

        RuleFor(x => x.RequestDto.SupervisorName)
            .MaximumLength(100).WithMessage("Supervisor name cannot exceed 100 characters.")
            .When(x => x.RequestDto.SupervisorName != null);
    }
}
