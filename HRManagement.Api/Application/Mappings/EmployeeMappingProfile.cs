using AutoMapper;
using HRManagement.Api.Application.EmployeeDtos.Queries.Dto;
using HRManagement.Api.Domain.Models.Tables;

namespace HRManagement.Api.Application.Mappings;

public class EmployeeMappingProfile : Profile
{
    public EmployeeMappingProfile()
    {
        // Maps employee entity into full profile response fields.
        CreateMap<Employee, EmployeeProfileResponseDto>()
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Gender, opt => opt.Ignore())
            .ForMember(dest => dest.MaritalStatus, opt => opt.Ignore())
            .ForMember(dest => dest.PersonalEmail, opt => opt.MapFrom(src => src.PersonalEmail))
            .ForMember(dest => dest.EmployeeEmail, opt => opt.MapFrom(src => src.EmployeeEmail))
            
            // Current Address
            .ForMember(dest => dest.CurrentStreetAddress, opt => opt.MapFrom(src => src.CurrentAddress.Street))
            .ForMember(dest => dest.CurrentCity, opt => opt.MapFrom(src => src.CurrentAddress.City))
            .ForMember(dest => dest.CurrentProvince, opt => opt.MapFrom(src => src.CurrentAddress.Province))
            .ForMember(dest => dest.CurrentPostalCode, opt => opt.MapFrom(src => src.CurrentAddress.ZipCode))
            
            // Residential Address
            .ForMember(dest => dest.ResidentialStreetAddress, opt => opt.MapFrom(src => src.ResidentialAddress.Street))
            .ForMember(dest => dest.ResidentialCity, opt => opt.MapFrom(src => src.ResidentialAddress.City))
            .ForMember(dest => dest.ResidentialProvince, opt => opt.MapFrom(src => src.ResidentialAddress.Province))
            .ForMember(dest => dest.ResidentialPostalCode, opt => opt.MapFrom(src => src.ResidentialAddress.ZipCode))
            
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.Nik, opt => opt.MapFrom(src => src.Nik))
            .ForMember(dest => dest.PlaceOfBirth, opt => opt.MapFrom(src => src.PlaceOfBirth))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.EmployeeStatus, opt => opt.Ignore())
            .ForMember(dest => dest.EmploymentType, opt => opt.Ignore())
            .ForMember(dest => dest.StartDate,
                opt => opt.MapFrom(src =>
                    src.EmploymentInformation != null ? src.EmploymentInformation.StartDate : DateTime.MinValue))
            .ForMember(dest => dest.Department,
                opt => opt.MapFrom(src =>
                    src.EmploymentInformation != null ? src.EmploymentInformation.Department : string.Empty))
            .ForMember(dest => dest.Position,
                opt => opt.MapFrom(src =>
                    src.EmploymentInformation != null ? src.EmploymentInformation.Position : string.Empty))
            .ForMember(dest => dest.SupervisorName,
                opt => opt.MapFrom(src =>
                    src.EmploymentInformation != null ? src.EmploymentInformation.SupervisorName : string.Empty))
            .ForMember(dest => dest.EmployeeDisplayId,
                opt => opt.MapFrom(src =>
                    src.EmploymentInformation != null ? src.EmploymentInformation.EmployeeDisplayId : string.Empty))
            .ForMember(dest => dest.EmergencyContactName,
                opt => opt.MapFrom(src =>
                    src.EmergencyContacts.FirstOrDefault() != null
                        ? src.EmergencyContacts.FirstOrDefault()!.Name
                        : string.Empty))
            .ForMember(dest => dest.EmergencyContactPhone,
                opt => opt.MapFrom(src =>
                    src.EmergencyContacts.FirstOrDefault() != null
                        ? src.EmergencyContacts.FirstOrDefault()!.PhoneNumber
                        : string.Empty))
            .ForMember(dest => dest.Relationship,
                opt => opt.MapFrom(src =>
                    src.EmergencyContacts.FirstOrDefault() != null
                        ? src.EmergencyContacts.FirstOrDefault()!.Relationship
                        : string.Empty));

        // Maps approved/pending update request values into profile response fields.
        CreateMap<EmployeeUpdateRequest, EmployeeProfileResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.NewFullName))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.NewGender))
            
            // Current Address
            .ForMember(dest => dest.CurrentStreetAddress, opt => opt.MapFrom(src => src.NewCurrentStreetAddress))
            .ForMember(dest => dest.CurrentCity, opt => opt.MapFrom(src => src.NewCurrentCity))
            .ForMember(dest => dest.CurrentProvince, opt => opt.MapFrom(src => src.NewCurrentProvince))
            .ForMember(dest => dest.CurrentPostalCode, opt => opt.MapFrom(src => src.NewCurrentZipCode))
            
            // Residential Address
            .ForMember(dest => dest.ResidentialStreetAddress, opt => opt.MapFrom(src => src.NewResidentialStreetAddress))
            .ForMember(dest => dest.ResidentialCity, opt => opt.MapFrom(src => src.NewResidentialCity))
            .ForMember(dest => dest.ResidentialProvince, opt => opt.MapFrom(src => src.NewResidentialProvince))
            .ForMember(dest => dest.ResidentialPostalCode, opt => opt.MapFrom(src => src.NewResidentialZipCode))
            
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.NewPhoneNumber))
            .ForMember(dest => dest.PersonalEmail, opt => opt.MapFrom(src => src.NewPersonalEmail))
            .ForMember(dest => dest.PlaceOfBirth, opt => opt.MapFrom(src => src.NewPlaceOfBirth))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.NewDateOfBirth))
            .ForMember(dest => dest.MaritalStatus, opt => opt.MapFrom(src => src.NewMaritalStatus))
            .ForMember(dest => dest.EmergencyContactName, opt => opt.MapFrom(src => src.NewEmergencyContactName))
            .ForMember(dest => dest.EmergencyContactPhone, opt => opt.MapFrom(src => src.NewEmergencyContactPhone))
            .ForMember(dest => dest.Relationship, opt => opt.MapFrom(src => src.NewEmergencyContactRelationship))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));


        // Maps update request entity to HR review response.
        CreateMap<EmployeeUpdateRequest, EmployeeRequestResponseDto>()
            .ForMember(dest => dest.RequestId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.RequesterName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : "Unknown"))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedUtcDate))
            .ForMember(dest => dest.NewFullName, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewPersonalEmail, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewPlaceOfBirth, opt => opt.NullSubstitute(string.Empty))
            
            .ForMember(dest => dest.NewCurrentStreetAddress, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewCurrentCity, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewCurrentProvince, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewCurrentPostalCode, opt => opt.MapFrom(src => src.NewCurrentZipCode))
            
            .ForMember(dest => dest.NewResidentialStreetAddress, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewResidentialCity, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewResidentialProvince, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewResidentialPostalCode, opt => opt.MapFrom(src => src.NewResidentialZipCode))
            
            .ForMember(dest => dest.NewPhoneNumber, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewEmergencyContactName, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewEmergencyContactPhone, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewEmergencyContactRelationship, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewGender, opt => opt.Ignore())
            .ForMember(dest => dest.NewMaritalStatus, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        // Maps employee entity to compact list item response.
        CreateMap<Employee, EmployeeListItemDto>()
            .ForMember(dest => dest.EmployeeDisplayId,
                opt => opt.MapFrom(src =>
                    src.EmploymentInformation != null ? src.EmploymentInformation.EmployeeDisplayId : string.Empty))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Department,
                opt => opt.MapFrom(src =>
                    src.EmploymentInformation != null ? src.EmploymentInformation.Department : string.Empty))
            .ForMember(dest => dest.Position,
                opt => opt.MapFrom(src =>
                    src.EmploymentInformation != null ? src.EmploymentInformation.Position : string.Empty))
            .ForMember(dest => dest.EmployeeStatus,
                opt => opt.MapFrom(src =>
                    src.EmploymentInformation != null ? src.EmploymentInformation.EmploymentStatus : 0));
    }
}
