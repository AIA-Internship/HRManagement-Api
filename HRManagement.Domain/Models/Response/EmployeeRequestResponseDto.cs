namespace HRManagement.Domain.Models.Response;

public record EmployeeRequestResponseDto
(
    int RequestId,
    string EmployeeDisplayId,
    string RequesterName,

    string? NewNIK,
    string? NewFullName,
    string? NewGender,
    string? NewPersonalEmail,
    string? NewBirthPlace,
    DateTime? NewBirthDate,
    int? NewMaritalStatus,
    
    string? NewCurrentAddress,
    string? NewCurrentCity,
    string? NewCurrentProvince,
    string? NewCurrentPostalCode,
    
    string? NewResidentialAddress,
    string? NewResidentialCity,
    string? NewResidentialProvince,
    string? NewResidentialPostalCode,

    string? NewMobilePhone,
    
    string? NewEmergencyContactName,
    string? NewEmergencyContactPhone,
    string? NewEmergencyContactRelationship,

    int RequestStatus,
    string? HRReason,
    DateTime CreatedAt
);
