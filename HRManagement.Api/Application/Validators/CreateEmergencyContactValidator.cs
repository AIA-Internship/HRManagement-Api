using FluentValidation;
using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;

namespace HRManagement.Api.Application.Validators;

public class CreateEmergencyContactValidator : AbstractValidator<CreateEmergencyContactDto>
{
    public CreateEmergencyContactValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Emergency contact name is required.")
            .MaximumLength(100).WithMessage("Emergency contact name cannot exceed 100 characters.");
        
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(25).WithMessage("Phone number cannot exceed 25 characters.")
            .Matches(@"^\+?[0-9\s\-]+$").WithMessage("Invalid phone number format.");
        
        RuleFor(x => x.Relationship)
            .NotEmpty().WithMessage("Employee relationship is required.")
            .MaximumLength(25).WithMessage("Employee relationship cannot exceed 25 characters.");
            
    }
}
