namespace HRManagement.Domain.Models.Payload.EmployeeDtos.Commands.Dto;

public class UpdateEmployeeRequestDto
{
    /// <example>John Doe</example>
    public string FullName { get; set; } = string.Empty;
    /// <example>1</example>
    public int? Gender { get; set; }
    /// <example>john.doe.personal@gmail.com</example>
    public string PersonalEmail { get; set; } = string.Empty;
    /// <example>Jakarta</example>
    public string PlaceOfBirth { get; set; } = string.Empty;
    /// <example>1990-01-01</example>
    public DateTime? DateOfBirth { get; set; }
    /// <example>1</example>
    public int? MaritalStatus { get; set; }
    
    // Current Address
    /// <example>Jl. Sudirman No. 1</example>
    public string CurrentStreetAddress { get; set; } = string.Empty;
    /// <example>Jakarta Selatan</example>
    public string CurrentCity { get; set; } = string.Empty;
    /// <example>DKI Jakarta</example>
    public string CurrentProvince { get; set; } = string.Empty;
    /// <example>12345</example>
    public string CurrentPostalCode { get; set; } = string.Empty;

    // Residential Address
    /// <example>Jl. Thamrin No. 10</example>
    public string ResidentialStreetAddress { get; set; } = string.Empty;
    /// <example>Bandung</example>
    public string ResidentialCity { get; set; } = string.Empty;
    /// <example>Jawa Barat</example>
    public string ResidentialProvince { get; set; } = string.Empty;
    /// <example>40123</example>
    public string ResidentialPostalCode { get; set; } = string.Empty;

    /// <example>08123456789</example>
    public string PhoneNumber { get; set; } = string.Empty;
    /// <example>Jane Doe</example>
    public string EmergencyContactName { get; set; } = string.Empty;
    /// <example>08987654321</example>
    public string EmergencyContactPhone { get; set; } = string.Empty;
    /// <example>Wife</example>
    public string EmergencyContactRelationship { get; set; } = string.Empty;
}


