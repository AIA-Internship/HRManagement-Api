using HRManagement.Domain.Interfaces;
using FluentValidation;
using HRManagement.Application.Commands;
using HRManagement.Application.Interfaces;

namespace HRManagement.Application.Validators;

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeValidator(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService)
    {
        RuleFor(x => x.RequestDto.FullName)
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.")
            .MustAsync(async (command, name, _) => 
            {
                var employee = await employeeRepository.GetProfileByEmailAsync(currentUserService.Email!);
                return await employeeRepository.IsUniqueAsync(e => e.FullName, name, employee?.Id);
            })
            .WithMessage(x => $"The full name '{x.RequestDto.FullName}' is already in use.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.FullName));

        RuleFor(x => x.RequestDto.Gender)
            .NotNull().WithMessage("Invalid gender value.")
            .When(x => x.RequestDto.Gender.HasValue);
            
        RuleFor(x => x.RequestDto.PersonalEmail)
            .MaximumLength(150).WithMessage("Personal email cannot exceed 150 characters.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MustAsync(async (command, email, _) => 
            {
                var employee = await employeeRepository.GetProfileByEmailAsync(currentUserService.Email!);
                return await employeeRepository.IsUniqueAsync(e => e.PersonalEmail, email, employee?.Id);
            })
            .WithMessage(x => $"The personal email '{x.RequestDto.PersonalEmail}' is already in use.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.PersonalEmail));

        RuleFor(x => x.RequestDto.PlaceOfBirth)
            .MaximumLength(100).WithMessage("Place of birth cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.PlaceOfBirth));

        RuleFor(x => x.RequestDto.DateOfBirth)
            .LessThan(DateTime.Today).WithMessage("Date of birth cannot be in the future.")
            .When(x => x.RequestDto.DateOfBirth.HasValue);

        RuleFor(x => x.RequestDto.MaritalStatus)
            .NotNull().WithMessage("Invalid marital status value.")
            .When(x => x.RequestDto.MaritalStatus.HasValue);
        
        // Current Address Validation
        RuleFor(x => x.RequestDto.CurrentStreetAddress)
            .MaximumLength(150).WithMessage("Current street address cannot exceed 150 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.CurrentStreetAddress));
        
        RuleFor(x => x.RequestDto.CurrentCity)
            .MaximumLength(100).WithMessage("Current city cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.CurrentCity));
        
        RuleFor(x => x.RequestDto.CurrentProvince)
            .MaximumLength(50).WithMessage("Current province cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.CurrentProvince));
        
        RuleFor(x => x.RequestDto.CurrentPostalCode)
            .MaximumLength(15).WithMessage("Current postal code cannot exceed 15 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.CurrentPostalCode));

        // Residential Address Validation
        RuleFor(x => x.RequestDto.ResidentialStreetAddress)
            .MaximumLength(150).WithMessage("Residential street address cannot exceed 150 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.ResidentialStreetAddress));
        
        RuleFor(x => x.RequestDto.ResidentialCity)
            .MaximumLength(100).WithMessage("Residential city cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.ResidentialCity));
        
        RuleFor(x => x.RequestDto.ResidentialProvince)
            .MaximumLength(50).WithMessage("Residential province cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.ResidentialProvince));
        
        RuleFor(x => x.RequestDto.ResidentialPostalCode)
            .MaximumLength(15).WithMessage("Residential postal code cannot exceed 15 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.ResidentialPostalCode));
        
        RuleFor(x => x.RequestDto.PhoneNumber)
            .MaximumLength(25).WithMessage("Phone number cannot exceed 25 characters.")
            .Matches(@"^\+?[0-9\s\-]+$").WithMessage("Invalid phone number format.")
            .MustAsync(async (command, phone, _) => 
            {
                var employee = await employeeRepository.GetProfileByEmailAsync(currentUserService.Email!);
                return await employeeRepository.IsUniqueAsync(e => e.MobilePhone, phone, employee?.Id);
            })
            .WithMessage(x => $"The phone number '{x.RequestDto.PhoneNumber}' is already in use.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.PhoneNumber)); 
        
        RuleFor(x => x.RequestDto.EmergencyContactName)
            .MaximumLength(150).WithMessage("Emergency contact name cannot exceed 150 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.EmergencyContactName));

        RuleFor(x => x.RequestDto.EmergencyContactPhone)
            .MaximumLength(25).WithMessage("Emergency contact phone cannot exceed 25 characters.")
            .Matches(@"^\+?[0-9\s\-]+$").WithMessage("Invalid emergency contact phone format.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.EmergencyContactPhone));

        RuleFor(x => x.RequestDto.EmergencyContactRelationship)
            .MaximumLength(50).WithMessage("Emergency contact relationship cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.EmergencyContactRelationship));
    }
}





