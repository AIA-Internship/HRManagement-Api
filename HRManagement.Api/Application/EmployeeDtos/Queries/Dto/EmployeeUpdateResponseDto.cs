namespace HRManagement.Api.Application.EmployeeDtos.Queries.Dto;

public class EmployeeRequestResponseDto
{
    public int RequestId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public string? NewFullName { get; set; }
    public string? NewPersonalEmail { get; set; }
    public string? NewPlaceOfBirth { get; set; }
    public string? NewGender { get; set; }
    public DateTime? NewDateOfBirth { get; set; }
    public string? NewMaritalStatus { get; set; }

    public string? NewCurrentStreetAddress { get; set; }
    public string? NewCurrentCity { get; set; }
    public string? NewCurrentProvince { get; set; }
    public string? NewCurrentPostalCode { get; set; }

    public string? NewResidentialStreetAddress { get; set; }
    public string? NewResidentialCity { get; set; }
    public string? NewResidentialProvince { get; set; }
    public string? NewResidentialPostalCode { get; set; }

    public string? NewPhoneNumber { get; set; }
    
    public string? NewEmergencyContactName { get; set; }
    public string? NewEmergencyContactPhone { get; set; }
    public string? NewEmergencyContactRelationship { get; set; }

    public string Status { get; set; } = string.Empty;
    public string? HrReason { get; set; }
}
