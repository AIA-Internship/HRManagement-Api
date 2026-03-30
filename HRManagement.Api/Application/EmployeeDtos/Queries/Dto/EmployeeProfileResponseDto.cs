namespace HRManagement.Api.Application.EmployeeDtos.Queries.Dto;

public class EmployeeProfileResponseDto
{
    //Personal Information & Address
    public string FullName { get; set; } = string.Empty;
    public string Gender { get; set; }
    public string PersonalEmail { get; set; } = string.Empty;
    public string EmployeeEmail { get; set; } =  string.Empty;

    // Current Address
    public string CurrentStreetAddress { get; set; } =  string.Empty;
    public string CurrentCity { get; set; } = string.Empty;
    public string CurrentProvince { get; set; } = string.Empty;
    public string CurrentPostalCode { get; set; } = string.Empty;

    // Residential Address
    public string ResidentialStreetAddress { get; set; } =  string.Empty;
    public string ResidentialCity { get; set; } = string.Empty;
    public string ResidentialProvince { get; set; } = string.Empty;
    public string ResidentialPostalCode { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } =   string.Empty;
    public string Nik { get; set; } =  string.Empty;
    public string PlaceOfBirth { get; set; } =   string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string MaritalStatus { get; set; }
    public bool IsActive { get; set; }
    
    //Employment Information
    public string EmployeeStatus { get; set; }
    public DateTime StartDate { get; set; }
    public string EmploymentType { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    // public string SupervisorDisplayId { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    public string EmployeeDisplayId { get; set; } = string.Empty;
    
    //Emergency Contact
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public string Relationship { get; set; } = string.Empty;
    
}
