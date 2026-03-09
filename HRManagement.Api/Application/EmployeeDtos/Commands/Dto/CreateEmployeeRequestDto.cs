namespace HRManagement.Api.Application.EmployeeDtos.Commands.Dto;

public class CreateEmployeeRequestDto
{
    /// <example>john.doe@aia.com</example>
    public string EmployeeEmail { get; set; } = string.Empty;
    /// <example>StrongPass123!</example>
    public string DefaultPassword { get; set; } = string.Empty;
    /// <example>John Doe</example>
    public string FullName { get; set; } = string.Empty;
    /// <example>1</example>
    public int Gender { get; set; }
    /// <example>john.doe.personal@gmail.com</example>
    public string PersonalEmail { get; set; } = string.Empty;
    /// <example>08123456789</example>
    public string PhoneNumber { get; set; } = string.Empty;
    /// <example>1234567890123456</example>
    public string Nik { get; set; } = string.Empty;
    /// <example>Jakarta</example>
    public string PlaceOfBirth { get; set; } = string.Empty;
    /// <example>1990-01-01</example>
    public DateTime DateOfBirth { get; set; }
    /// <example>1</example>
    public int MaritalStatus { get; set; }
    /// <example>Jl. Sudirman No. 1</example>
    public string StreetAddress { get; set; } = string.Empty;
    /// <example>Jakarta Selatan</example>
    public string City { get; set; } = string.Empty;
    /// <example>DKI Jakarta</example>
    public string Province { get; set; } = string.Empty;
    /// <example>12345</example>
    public string PostalCode { get; set; } = string.Empty;
    /// <example>1</example>
    public int Role { get; set; }
    public CreateEmploymentInfoDto? EmploymentInformation { get; set; }
    public List<CreateEmergencyContactDto> EmergencyContacts { get; set; } = new();
}

public class CreateEmploymentInfoDto
{
    /// <example>1</example>
    public int EmploymentStatus { get; set; }
    /// <example>2024-01-01</example>
    public DateTime StartDate { get; set; } = DateTime.Now;
    /// <example>1</example>
    public int EmploymentType { get; set; }
    /// <example>Information Technology</example>
    public string Department { get; set; } = string.Empty;
    /// <example>Software Engineer</example>
    public string Position { get; set; } =  string.Empty;
    /// <example>Jane Smith</example>
    public string SupervisorName { get; set; } = string.Empty;
}

public class CreateEmergencyContactDto
{
    /// <example>Jane Doe</example>
    public string Name { get; set; } = string.Empty;
    /// <example>Wife</example>
    public string Relationship { get; set; } = string.Empty;
    /// <example>08987654321</example>
    public string PhoneNumber { get; set; } = string.Empty;
}
