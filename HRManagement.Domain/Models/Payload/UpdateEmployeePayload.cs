namespace HRManagement.Domain.Models.Payload;

public record UpdateEmployeePayload
(
    string? FullName,
    string? Gender,
    string? PersonalEmail,
    string? PlaceOfBirth,
    string? Nik,
    DateTime? DateOfBirth,
    int? MaritalStatus,
    
    string? CurrentStreetAddress,
    string? CurrentCity,
    string? CurrentProvince,
    string? CurrentPostalCode,

    string? ResidentialStreetAddress,
    string? ResidentialCity,
    string? ResidentialProvince,
    string? ResidentialPostalCode,

    string? PhoneNumber,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? EmergencyContactRelationship
);
