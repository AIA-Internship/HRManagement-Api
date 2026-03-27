using FluentValidation;
using HRManagement.Api.Application.Commands;
using HRManagement.Api.Application.Interfaces;

namespace HRManagement.Api.Application.Validators;

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator(IEmployeeRepository employeeRepository)
    {
        RuleFor(x => x.RequestDto.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters.")
            .MustAsync(async (name, _) => await employeeRepository.IsFullNameUniqueAsync(name))
            .WithMessage(x => $"The full name '{x.RequestDto.FullName}' is already in use.");

        RuleFor(x => x.RequestDto.Gender)
            .NotNull().WithMessage("Please select a valid gender option.");
        
        RuleFor(x => x.RequestDto.PersonalEmail)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email is required.")
            .NotEqual(x => x.RequestDto.EmployeeEmail).WithMessage("Personal email cannot be the same as the employee email.")
            .MustAsync(async (email, _) => await employeeRepository.IsPersonalEmailUniqueAsync(email))
            .WithMessage(x => $"The personal email '{x.RequestDto.PersonalEmail}' is already in use.");

        RuleFor(x => x.RequestDto.EmployeeEmail)
            .NotEmpty().WithMessage("Employee email is required.")
            .EmailAddress().WithMessage("A valid email is required.")
            .Must(email => email.EndsWith("@aia.com", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Email must be a valid aia.com email")
            .MustAsync(async (email, _) => await employeeRepository.IsEmailUniqueAsync(email))
            .WithMessage(x => $"The work email '{x.RequestDto.EmployeeEmail}' is already in use.");
        
        RuleFor(x => x.RequestDto.Nik)
            .NotEmpty().WithMessage("Nik is required.")
            .Length(16).WithMessage("Nik must be exactly 16 digits.") 
            .Matches("^[0-9]+$").WithMessage("Nik can only contain numbers.")
            .MustAsync(async (nik, _) => await employeeRepository.IsNikUniqueAsync(nik))
            .WithMessage(x => $"The NIK '{x.RequestDto.Nik}' is already in use.");
        
        RuleFor(x => x.RequestDto.PlaceOfBirth)
            .NotEmpty().WithMessage("Birth Place is required.")
            .MaximumLength(100).WithMessage("Birth Place cannot exceed 100 characters.");

        RuleFor(x => x.RequestDto.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required.")
            .LessThanOrEqualTo(DateTime.Today.AddYears(-18))
            .WithMessage("Employee must be at least 18 years old.");
        
        RuleFor(x => x.RequestDto.MaritalStatus)
            .NotNull().WithMessage("Please select a valid marital status option.");
        
        RuleFor(x => x.RequestDto.StreetAddress)
            .NotEmpty().WithMessage("Street address is required.")
            .MaximumLength(150).WithMessage("Street address cannot exceed 150 characters.");
        
        RuleFor(x => x.RequestDto.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");
        
        RuleFor(x => x.RequestDto.Province)
            .NotEmpty().WithMessage("Province is required.")
            .MaximumLength(50).WithMessage("Province cannot exceed 50 characters.");
        
        RuleFor(x => x.RequestDto.PostalCode)
            .NotEmpty().WithMessage("Postal code is required.")
            .MaximumLength(15).WithMessage("Postal code cannot exceed 15 characters.");
        
        RuleFor(x => x.RequestDto.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(25).WithMessage("Phone number cannot exceed 25 characters.")
            .Matches(@"^\+?[0-9\s\-]+$").WithMessage("Invalid phone number format.")
            .MustAsync(async (phone, _) => await employeeRepository.IsPhoneNumberUniqueAsync(phone))
            .WithMessage(x => $"The phone number '{x.RequestDto.PhoneNumber}' is already in use.");
        
        RuleFor(x => x.RequestDto.EmploymentInformation)
            .SetValidator(new CreateEmploymentInfoValidator()!)
            .When(x => x.RequestDto.EmploymentInformation != null);

        RuleForEach(x => x.RequestDto.EmergencyContacts)
            .SetValidator(new CreateEmergencyContactValidator());

        RuleFor(x => x.RequestDto.DefaultPassword)
            .NotEmpty().WithMessage("Default password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Must((cmd, pwd) => !string.Equals(pwd, cmd.RequestDto.EmployeeEmail, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Password cannot match the employee email.")
            .Must((cmd, pwd) => !string.Equals(pwd, cmd.RequestDto.PersonalEmail, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Password cannot match the personal email.")
            .Must(pwd => !string.Equals(pwd, "CompanyPass123", StringComparison.Ordinal))
            .WithMessage("Default password cannot be the insecure factory value.");
    }
}
