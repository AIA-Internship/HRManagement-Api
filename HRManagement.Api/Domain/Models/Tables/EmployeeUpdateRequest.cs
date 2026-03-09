using HRManagement.Api.Application.EmployeeDtos.Commands.Dto;

namespace HRManagement.Api.Domain.Models.Tables;

public class EmployeeUpdateRequest : BaseTableModel
{
    public int Id { get; private set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; private set; }
    
    public string? NewFullName { get; private set; }
    public int? NewGender { get; private set; }
    public string? NewPersonalEmail { get; private set; }
    public string? NewPlaceOfBirth { get; private set; }
    public DateTime? NewDateOfBirth { get; private set; }
    public int? NewMaritalStatus { get; private set; }
    public string? NewStreetAddress { get; private set; }
    public string? NewCity { get; private set; }
    public string? NewProvince { get; private set; }
    public string? NewPostalCode { get; private set; }
    public string? NewPhoneNumber { get; private set; }
    
    
    public string? NewEmergencyContactName { get; private set; }
    public string? NewEmergencyContactPhone { get; private set; }
    public string? NewEmergencyContactRelationship { get; private set; }

    public int Status { get; private set; }
    public string HrReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    protected EmployeeUpdateRequest() { }
    
    public EmployeeUpdateRequest(int employeeId, UpdateEmployeeRequestDto dto, long actionerId)
    {
        EmployeeId = employeeId;
        NewFullName = dto.FullName;
        NewGender = dto.Gender;
        NewPersonalEmail = dto.PersonalEmail;
        NewPlaceOfBirth = dto.PlaceOfBirth;
        NewDateOfBirth = dto.DateOfBirth;
        NewMaritalStatus = dto.MaritalStatus;
        NewStreetAddress = dto.StreetAddress;
        NewCity = dto.City;
        NewProvince = dto.Province;
        NewPostalCode = dto.PostalCode;
        NewPhoneNumber = dto.PhoneNumber;
        NewEmergencyContactName = dto.EmergencyContactName;
        NewEmergencyContactPhone = dto.EmergencyContactPhone;
        NewEmergencyContactRelationship = dto.EmergencyContactRelationship;
    
        Status = 0; // Pending
        CreatedBy = actionerId;
        ModifiedBy = actionerId;
    }

    public void Reject(string reason, long hrActionerId)
    {
        Status = 2; // Rejected
        HrReason = reason ?? "Rejected by Supervisor";
        ModifiedBy = hrActionerId;
        ModifiedUtcDate = DateTime.UtcNow.AddHours(7);
        CreatedAt = DateTime.UtcNow.AddHours(7);
    }

    public void Approve(string reason, long hrActionerId)
    {
        Status = 1; // Approved
        HrReason = reason ?? "Approved by Supervisor";
        ModifiedBy = hrActionerId;
        ModifiedUtcDate = DateTime.UtcNow.AddHours(7);
    }
}