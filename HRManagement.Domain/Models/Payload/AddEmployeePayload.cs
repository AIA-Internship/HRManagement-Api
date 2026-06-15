namespace HRManagement.Domain.Models.Payload;

public record AddEmployeePayload
(
    string EmployeeEmail,
    string DefaultPassword,
    string FullName,
    string Gender,
    string PersonalEmail,
    string PhoneNumber,
    string Nik,
    string PlaceOfBirth,
    DateTime DateOfBirth,
    int MaritalStatus,
    
    string CurrentAddress,
    string CurrentCity,
    string CurrentProvince,
    string CurrentPostalCode,

    string ResidentialAddress,
    string ResidentialCity,
    string ResidentialProvince,
    string ResidentialPostalCode,

    int RoleId,
    AddEmploymentInfoPayload? EmploymentInformation,
    AddEmergencyContactPayload? EmergencyContact
);

public record AddEmploymentInfoPayload
(
    int EmploymentStatus,
    DateTime StartDate,
    int EmploymentType,
    string Department,
    string Position,
    int? SupervisorId,
    string SupervisorName,
    string EmployeeDisplayId
);

public record AddEmergencyContactPayload
(
    string Name,
    string PhoneNumber,
    string Relationship
);
