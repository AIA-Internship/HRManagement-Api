using FluentValidation;
using HRManagement.Api.Application.Commands;
using HRManagement.Api.Application.Interfaces;

namespace HRManagement.Api.Application.Validators;

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeValidator(IEmployeeRepository employeeRepository, ICurrentUserService currentUserService)
    {
        RuleFor(x => x.RequestDto.FullName)
            .MaximumLength(150).WithMessage("Full name cannot exceed 150 characters.")
            .MustAsync(async (command, name, _) => 
            {
                var employee = await employeeRepository.GetByEmailAsync(currentUserService.Email!);
                return await employeeRepository.IsFullNameUniqueAsync(name, employee?.Id);
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
                var employee = await employeeRepository.GetByEmailAsync(currentUserService.Email!);
                return await employeeRepository.IsPersonalEmailUniqueAsync(email, employee?.Id);
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
        
        RuleFor(x => x.RequestDto.StreetAddress)
            .MaximumLength(150).WithMessage("Street address cannot exceed 150 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.StreetAddress));
        
        RuleFor(x => x.RequestDto.City)
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.City));
        
        RuleFor(x => x.RequestDto.Province)
            .MaximumLength(50).WithMessage("Province cannot exceed 50 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.Province));
        
        RuleFor(x => x.RequestDto.PostalCode)
            .MaximumLength(15).WithMessage("Postal code cannot exceed 15 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.RequestDto.PostalCode));
        
        RuleFor(x => x.RequestDto.PhoneNumber)
            .MaximumLength(25).WithMessage("Phone number cannot exceed 25 characters.")
            .Matches(@"^\+?[0-9\s\-]+$").WithMessage("Invalid phone number format.")
            .MustAsync(async (command, phone, _) => 
            {
                var employee = await employeeRepository.GetByEmailAsync(currentUserService.Email!);
                return await employeeRepository.IsPhoneNumberUniqueAsync(phone, employee?.Id);
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
