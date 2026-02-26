namespace HRManagement.Api.Application.EmployeeDtos.Queries.Dto;

public class EmployeeRequestResponseDto
{
    public int RequestId { get; set; }
    public int EmployeeId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string NewFullName { get; set; } =  string.Empty;
    public string NewGender { get; set; } = string.Empty;
    public string NewPersonalEmail { get; set; } = string.Empty;
    public string NewPlaceOfBirth { get; set; } = string.Empty;
    public DateTime NewDateOfBirth { get; set; }
    public string NewMaritalStatus { get; set; } = string.Empty;
    public string NewStreetAddress { get; set; } = string.Empty;
    public string NewCity { get; set; } = string.Empty;
    public string NewProvince { get; set; } = string.Empty;
    public string NewPostalCode { get; set; } = string.Empty;
    public string NewPhoneNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? HrReason { get; set; }
    public string NewEmergencyContactName { get; set; } = string.Empty;
    public string NewEmergencyContactPhone { get; set; } = string.Empty;
    public string NewEmergencyContactRelationship { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
