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
            .ForMember(dest => dest.StreetAddress, opt => opt.MapFrom(src => src.StreetAddress))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.Province))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
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
                        ? src.EmergencyContacts.FirstOrDefault().Name
                        : string.Empty))
            .ForMember(dest => dest.EmergencyContactPhone,
                opt => opt.MapFrom(src =>
                    src.EmergencyContacts.FirstOrDefault() != null
                        ? src.EmergencyContacts.FirstOrDefault().PhoneNumber
                        : string.Empty))
            .ForMember(dest => dest.Relationship,
                opt => opt.MapFrom(src =>
                    src.EmergencyContacts.FirstOrDefault() != null
                        ? src.EmergencyContacts.FirstOrDefault().Relationship
                        : string.Empty));

        // Maps approved/pending update request values into profile response fields.
        CreateMap<EmployeeUpdateRequest, EmployeeProfileResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.NewFullName))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.NewGender))
            .ForMember(dest => dest.StreetAddress, opt => opt.MapFrom(src => src.NewStreetAddress))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.NewCity))
            .ForMember(dest => dest.Province, opt => opt.MapFrom(src => src.NewProvince))
            .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.NewPostalCode))
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
            .ForMember(dest => dest.RequesterName, opt => opt.MapFrom(src => src.Employee.FullName))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedUtcDate))
            .ForMember(dest => dest.NewFullName, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewPersonalEmail, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewPlaceOfBirth, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewStreetAddress, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewCity, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewProvince, opt => opt.NullSubstitute(string.Empty))
            .ForMember(dest => dest.NewPostalCode, opt => opt.NullSubstitute(string.Empty))
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
