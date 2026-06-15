using HRManagement.Api.Domain.Models.Tables;
using HRManagement.Application.EmployeeDtos.Commands.Dto;
using HRManagement.Domain.Models.Response;

namespace HRManagement.Application.Mappings;

public static class EmployeeMappings
{
    public static EmployeeProfileResponseDto ToProfileResponse(this Employee employee, IReadOnlyCollection<SystemLookup> lookups)
    {
        var employment = employee.EmploymentInformation;
        var emergencyContact = employee.EmergencyContacts.FirstOrDefault();

        return new EmployeeProfileResponseDto
        {
            FullName = employee.FullName,
            Gender = GetLookupDisplayName(lookups, "GENDER", employee.Gender),
            PersonalEmail = employee.PersonalEmail,
            EmployeeEmail = employee.EmployeeEmail,
            
            CurrentStreetAddress = employee.CurrentAddress.Street,
            CurrentCity = employee.CurrentAddress.City,
            CurrentProvince = employee.CurrentAddress.Province,
            CurrentPostalCode = employee.CurrentAddress.ZipCode,

            ResidentialStreetAddress = employee.ResidentialAddress.Street,
            ResidentialCity = employee.ResidentialAddress.City,
            ResidentialProvince = employee.ResidentialAddress.Province,
            ResidentialPostalCode = employee.ResidentialAddress.ZipCode,

            PhoneNumber = employee.PhoneNumber,
            Nik = employee.Nik,
            PlaceOfBirth = employee.PlaceOfBirth,
            DateOfBirth = employee.DateOfBirth,
            MaritalStatus = GetLookupDisplayName(lookups, "MARITAL_STATUS", employee.MaritalStatus),
            IsActive = employee.IsActive,
            EmployeeStatus = employment == null
                ? "Unknown"
                : GetLookupDisplayName(lookups, "EMPLOYMENT_STATUS", employment.EmploymentStatus),
            StartDate = employment?.StartDate ?? DateTime.MinValue,
            EmploymentType = employment == null
                ? "Unknown"
                : GetLookupDisplayName(lookups, "EMPLOYMENT_TYPE", employment.EmploymentType),
            Department = employment?.Department ?? string.Empty,
            Position = employment?.Position ?? string.Empty,
            // SupervisorDisplayId = employment?.Supervisor?.EmploymentInformation?.EmployeeDisplayId ?? string.Empty,
            SupervisorName = employment?.Supervisor?.FullName ?? string.Empty,
            EmployeeDisplayId = employment?.EmployeeDisplayId ?? string.Empty,
            EmergencyContactName = emergencyContact?.Name ?? string.Empty,
            EmergencyContactPhone = emergencyContact?.PhoneNumber ?? string.Empty,
            Relationship = emergencyContact?.Relationship ?? string.Empty
        };
    }

    public static EmployeeListResponseDto ToEmployeeListResponse(this Employee employee, IReadOnlyCollection<SystemLookup> lookups)
    {
        var employment = employee.EmploymentInformation;
        
        return new EmployeeListResponseDto
        {
            EmployeeDisplayId = employment?.EmployeeDisplayId ?? string.Empty,
            FullName = employee.FullName, 
            Department = employment?.Department ?? string.Empty, 
            Position = employment?.Position ?? string.Empty, 
            EmployeeStatus = employment == null ? "Unknown" : GetLookupDisplayName(lookups, "EMPLOYMENT_STATUS", employment.EmploymentStatus)
        };
    }

    public static EmployeeRequestResponseDto ToEmployeeRequestResponse(this EmployeeUpdateRequest request, IReadOnlyCollection<SystemLookup> lookups)
    {
        return new EmployeeRequestResponseDto
        {
            RequestId = request.Id,
            EmployeeDisplayId = request.Employee?.EmploymentInformation?.EmployeeDisplayId ?? string.Empty,
            RequesterName = request.Employee?.FullName ?? string.Empty,
            NewFullName = request.NewFullName,
            NewGender = request.NewGender.HasValue ? GetLookupDisplayName(lookups, "GENDER", request.NewGender.Value) : null,
            NewPersonalEmail = request.NewPersonalEmail,
            NewPlaceOfBirth = request.NewPlaceOfBirth,
            NewNik = request.NewNik,
            NewDateOfBirth = request.NewDateOfBirth,
            NewMaritalStatus = request.NewMaritalStatus.HasValue ? GetLookupDisplayName(lookups, "MARITAL_STATUS", request.NewMaritalStatus.Value) : null,
            
            NewCurrentStreetAddress = request.NewCurrentStreetAddress,
            NewCurrentCity = request.NewCurrentCity,
            NewCurrentProvince = request.NewCurrentProvince,
            NewCurrentPostalCode = request.NewCurrentZipCode,

            NewResidentialStreetAddress = request.NewResidentialStreetAddress,
            NewResidentialCity = request.NewResidentialCity,
            NewResidentialProvince = request.NewResidentialProvince,
            NewResidentialPostalCode = request.NewResidentialZipCode,

            NewPhoneNumber = request.NewPhoneNumber,
            Status = GetLookupDisplayName(lookups, "REQUEST_STATUS", request.Status),
            HrReason = request.HrReason,
            NewEmergencyContactName = request.NewEmergencyContactName,
            NewEmergencyContactPhone = request.NewEmergencyContactPhone,
            NewEmergencyContactRelationship = request.NewEmergencyContactRelationship,
            CreatedAt = request.CreatedAt
        };
    }

    public static Employee ToEntity(this CreateEmployeeRequestDto dto, long actionerId, EmploymentInformation? employmentInfo, List<EmergencyContact> emergencyContacts)
    {
        return new Employee(
            fullName: dto.FullName,
            gender: dto.Gender,
            personalEmail: dto.PersonalEmail,
            employeeEmail: dto.EmployeeEmail,
            phoneNumber: dto.PhoneNumber,
            nik: dto.Nik,
            placeOfBirth: dto.PlaceOfBirth,
            dateOfBirth: dto.DateOfBirth,
            maritalStatus: dto.MaritalStatus,
            currentAddress: new Address(dto.CurrentStreetAddress, dto.CurrentCity, dto.CurrentProvince, dto.CurrentPostalCode),
            residentialAddress: new Address(dto.ResidentialStreetAddress, dto.ResidentialCity, dto.ResidentialProvince, dto.ResidentialPostalCode),
            roleId: dto.Role,
            actionerId: actionerId,
            employmentInformation: employmentInfo,
            emergencyContacts: emergencyContacts
        );
    }

    public static EmploymentInformation ToEntity(this CreateEmploymentInfoDto infoDto, string displayId, int? supervisorId, long actionerId)
    {
        var employmentInfo = new EmploymentInformation(actionerId);
        
        employmentInfo.UpdateDetails(
            infoDto.EmploymentStatus,
            infoDto.StartDate,
            infoDto.EmploymentType,
            infoDto.Department,
            infoDto.Position,
            supervisorId,
            displayId,
            actionerId
        );

        return employmentInfo;
    }

    public static List<EmergencyContact> ToEntityList(this IEnumerable<CreateEmergencyContactDto>? contactDtos, long actionerId)
    {
        if (contactDtos == null || !contactDtos.Any())
            return new List<EmergencyContact>();

        return contactDtos.Select(dto => new EmergencyContact
        {
            Name = dto.Name,
            Relationship = dto.Relationship,
            PhoneNumber = dto.PhoneNumber,
            CreatedBy = actionerId,
            ModifiedBy = actionerId
        }).ToList();
    }

    private static string GetLookupDisplayName(
        IReadOnlyCollection<SystemLookup> lookups,
        string category,
        int value)
    {
        return lookups.FirstOrDefault(x => x.Category == category && x.Value == value)?.DisplayName ?? "Unknown";
    }
}
