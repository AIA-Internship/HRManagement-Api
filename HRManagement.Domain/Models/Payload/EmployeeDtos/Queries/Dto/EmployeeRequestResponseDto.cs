namespace HRManagement.Domain.Models.Payload.EmployeeDtos.Queries.Dto;

public class EmployeeRequestResponseDto
{
    public int RequestId { get; set; }
    public string EmployeeDisplayId { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public string NewFullName { get; set; } =  string.Empty;
    public string NewGender { get; set; } = string.Empty;
    public string NewPersonalEmail { get; set; } = string.Empty;
    public string NewPlaceOfBirth { get; set; } = string.Empty;
    public DateTime NewDateOfBirth { get; set; }
    public string NewMaritalStatus { get; set; } = string.Empty;
    
    public string NewCurrentStreetAddress { get; set; } = string.Empty;
    public string NewCurrentCity { get; set; } = string.Empty;
    public string NewCurrentProvince { get; set; } = string.Empty;
    public string NewCurrentPostalCode { get; set; } = string.Empty;
    
    public string NewResidentialStreetAddress { get; set; } = string.Empty;
    public string NewResidentialCity { get; set; } = string.Empty;
    public string NewResidentialProvince { get; set; } = string.Empty;
    public string NewResidentialPostalCode { get; set; } = string.Empty;

    public string NewPhoneNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? HrReason { get; set; }
    public string NewEmergencyContactName { get; set; } = string.Empty;
    public string NewEmergencyContactPhone { get; set; } = string.Empty;
    public string NewEmergencyContactRelationship { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}


