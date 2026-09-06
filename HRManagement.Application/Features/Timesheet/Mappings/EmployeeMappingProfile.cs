using HRManagement.Domain.Interfaces;
using AutoMapper;
using HRManagement.Domain.Models.Payload.EmployeeDtos.Queries.Dto;
using HRManagement.Domain.Models.Tables;
using System;
using System.Linq;

namespace HRManagement.Application.Mappings;

public class EmployeeMappingProfile : Profile
{
    public EmployeeMappingProfile()
    {
        CreateMap<Employee, EmployeeProfileResponseDto>()
            .ForMember(dest => dest.EmployeeId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Gender, opt => opt.Ignore())
            .ForMember(dest => dest.MaritalStatus, opt => opt.Ignore())
            .ForMember(dest => dest.PersonalEmail, opt => opt.MapFrom(src => src.PersonalEmail))
            .ForMember(dest => dest.EmployeeEmail, opt => opt.MapFrom(src => src.EmployeeEmail))
            .ForMember(dest => dest.CurrentStreetAddress, opt => opt.MapFrom(src => src.CurrentAddress))
            .ForMember(dest => dest.CurrentCity, opt => opt.MapFrom(src => src.CurrentCity))
            .ForMember(dest => dest.CurrentProvince, opt => opt.MapFrom(src => src.CurrentProvince))
            .ForMember(dest => dest.CurrentPostalCode, opt => opt.MapFrom(src => src.CurrentPostalCode))
            .ForMember(dest => dest.ResidentialStreetAddress, opt => opt.MapFrom(src => src.ResidentialAddress))
            .ForMember(dest => dest.ResidentialCity, opt => opt.MapFrom(src => src.ResidentialCity))
            .ForMember(dest => dest.ResidentialProvince, opt => opt.MapFrom(src => src.ResidentialProvince))
            .ForMember(dest => dest.ResidentialPostalCode, opt => opt.MapFrom(src => src.ResidentialPostalCode))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.MobilePhone))
            .ForMember(dest => dest.Nik, opt => opt.MapFrom(src => src.NIK))
            .ForMember(dest => dest.PlaceOfBirth, opt => opt.MapFrom(src => src.BirthPlace))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.BirthDate))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
            .ForMember(dest => dest.EmployeeStatus, opt => opt.MapFrom(src => src.EmploymentInformation != null ? src.EmploymentInformation.StatusCode : 0))
            .ForMember(dest => dest.EmploymentType, opt => opt.MapFrom(src => src.EmploymentInformation != null ? src.EmploymentInformation.TypeCode : 0))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.EmploymentInformation != null ? src.EmploymentInformation.StartDate : DateTime.MinValue))
            .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.EmploymentInformation != null ? src.EmploymentInformation.DepartmentName : string.Empty))
            .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.EmploymentInformation != null ? src.EmploymentInformation.PositionName : string.Empty))
            .ForMember(dest => dest.SupervisorName, opt => opt.MapFrom(src => src.EmploymentInformation != null ? src.EmploymentInformation.SupervisorName : string.Empty))
            .ForMember(dest => dest.EmployeeDisplayId, opt => opt.MapFrom(src => src.EmploymentInformation != null ? src.EmploymentInformation.DisplayId : string.Empty))
            .ForMember(dest => dest.EmergencyContactName, opt => opt.MapFrom(src => src.EmergencyContact != null ? src.EmergencyContact.ContactName : string.Empty))
            .ForMember(dest => dest.EmergencyContactPhone, opt => opt.MapFrom(src => src.EmergencyContact != null ? src.EmergencyContact.ContactPhone : string.Empty))
            .ForMember(dest => dest.Relationship, opt => opt.MapFrom(src => src.EmergencyContact != null ? src.EmergencyContact.ContactRelationship : string.Empty));

        CreateMap<EmployeeUpdateRequest, EmployeeProfileResponseDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.NewFullName))
            .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.NewGender))
            .ForMember(dest => dest.CurrentStreetAddress, opt => opt.MapFrom(src => src.NewCurrentStreetAddress))
            .ForMember(dest => dest.CurrentCity, opt => opt.MapFrom(src => src.NewCurrentCity))
            .ForMember(dest => dest.CurrentProvince, opt => opt.MapFrom(src => src.NewCurrentProvince))
            .ForMember(dest => dest.CurrentPostalCode, opt => opt.MapFrom(src => src.NewCurrentZipCode))
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

        CreateMap<Employee, EmployeeListItemDto>()
            .ForMember(dest => dest.EmployeeDisplayId, opt => opt.MapFrom(src => src.EmploymentInformation != null ? src.EmploymentInformation.DisplayId : string.Empty))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.EmploymentInformation != null ? src.EmploymentInformation.DepartmentName : string.Empty))
            .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.EmploymentInformation != null ? src.EmploymentInformation.PositionName : string.Empty))
            .ForMember(dest => dest.EmployeeStatus, opt => opt.MapFrom(src => src.EmploymentInformation != null ? src.EmploymentInformation.StatusCode : 0));
    }
}
