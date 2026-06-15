namespace HRManagement.Domain.Models.Response;

public record EmergencyContactDto(
    string Name,
    string Relationship,
    string PhoneNumber
);

public record EmploymentInformationDto(
    DateTime StartDate,
    string DisplayId,
    string? TypeName,
    string? DepartmentName,
    string? PositionName,
    int? SupervisorId,
    string? SupervisorName
);

public record EmployeeProfileResponseDto
(
    //Personal Information & Address
    string FullName,
    string Gender,
    string PersonalEmail,
    string EmployeeEmail,

    // Current Address
    string CurrentStreetAddress,
    string CurrentCity,
    string CurrentProvince,
    string CurrentPostalCode,

    // Residential Address
    string ResidentialStreetAddress,
    string ResidentialCity,
    string ResidentialProvince,
    string ResidentialPostalCode,

    string PhoneNumber,
    string Nik,
    string PlaceOfBirth,
    DateTime DateOfBirth,
    string MaritalStatus,
    bool IsActive,

    EmploymentInformationDto? EmploymentInformation,
    List<EmergencyContactDto> EmergencyContacts
);
