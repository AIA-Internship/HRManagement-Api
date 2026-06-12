namespace HRManagement.Application.EmployeeDtos.Commands.Dto;

public class UpdateEmployeeRequestDto
{
    /// <example>John Doe</example>
    public string? FullName { get; set; }
    /// <example>1</example>
    public int? Gender { get; set; }
    /// <example>john.doe.personal@gmail.com</example>
    public string? PersonalEmail { get; set; }
    /// <example>Jakarta</example>
    public string? PlaceOfBirth { get; set; }
    /// <summary>
    /// 1234567890123456
    /// </summary>
    public string? Nik { get; set; }
    /// <example>1990-01-01</example>
    public DateTime? DateOfBirth { get; set; }
    /// <example>1</example>
    public int? MaritalStatus { get; set; }
    
    // Current Address
    /// <example>Jl. Sudirman No. 1</example>
    public string? CurrentStreetAddress { get; set; }
    /// <example>Jakarta Selatan</example>
    public string? CurrentCity { get; set; }
    /// <example>DKI Jakarta</example>
    public string? CurrentProvince { get; set; }
    /// <example>12345</example>
    public string? CurrentPostalCode { get; set; }

    // Residential Address
    /// <example>Jl. Thamrin No. 10</example>
    public string? ResidentialStreetAddress { get; set; }
    /// <example>Bandung</example>
    public string? ResidentialCity { get; set; }
    /// <example>Jawa Barat</example>
    public string? ResidentialProvince { get; set; }
    /// <example>40123</example>
    public string? ResidentialPostalCode { get; set; }

    /// <example>08123456789</example>
    public string? PhoneNumber { get; set; }
    /// <example>Jane Doe</example>
    public string? EmergencyContactName { get; set; }
    /// <example>08987654321</example>
    public string? EmergencyContactPhone { get; set; }
    /// <example>Wife</example>
    public string? EmergencyContactRelationship { get; set; }
}
