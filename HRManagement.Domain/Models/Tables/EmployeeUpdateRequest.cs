namespace HRManagement.Domain.Models.Tables;

public class EmployeeUpdateRequest : BaseTable
{
    public int Id { get; private set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; private set; }
    
    public string? NewFullName { get; private set; }
    public int? NewGender { get; private set; }
    public string? NewPersonalEmail { get; private set; }
    public string? NewPlaceOfBirth { get; private set; }
    public string? NewNik { get; private set; }
    public DateTime? NewDateOfBirth { get; private set; }
    public int? NewMaritalStatus { get; private set; }
    
    // New Current Address
    public string? NewCurrentStreetAddress { get; private set; }
    public string? NewCurrentCity { get; private set; }
    public string? NewCurrentProvince { get; private set; }
    public string? NewCurrentZipCode { get; private set; }

    // New Residential Address
    public string? NewResidentialStreetAddress { get; private set; }
    public string? NewResidentialCity { get; private set; }
    public string? NewResidentialProvince { get; private set; }
    public string? NewResidentialZipCode { get; private set; }

    public string? NewPhoneNumber { get; private set; }
    
    
    public string? NewEmergencyContactName { get; private set; }
    public string? NewEmergencyContactPhone { get; private set; }
    public string? NewEmergencyContactRelationship { get; private set; }

    public int Status { get; private set; }
    public string HrReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    protected EmployeeUpdateRequest() { }
    
    public EmployeeUpdateRequest(Employee employee, UpdateEmployeeRequestDto dto, long actionerId)
    {
        EmployeeId = employee.Id;
        NewFullName = IsChanged(dto.FullName, employee.FullName) ? dto.FullName : null;
        NewGender = IsChanged(dto.Gender, employee.Gender) ? dto.Gender : null;
        NewPersonalEmail = IsChanged(dto.PersonalEmail, employee.PersonalEmail) ? dto.PersonalEmail : null;
        NewPlaceOfBirth = IsChanged(dto.PlaceOfBirth, employee.PlaceOfBirth) ? dto.PlaceOfBirth : null;
        NewNik = IsChanged(dto.Nik, employee.NIK) ? dto.Nik : null;
        NewDateOfBirth = IsChanged(dto.DateOfBirth, employee.DateOfBirth) ? dto.DateOfBirth : null;
        NewMaritalStatus = IsChanged(dto.MaritalStatus, employee.MaritalStatus) ? dto.MaritalStatus : null;
        
        NewCurrentStreetAddress = IsChanged(dto.CurrentStreetAddress, employee.CurrentAddress.Street) ? dto.CurrentStreetAddress : null;
        NewCurrentCity = IsChanged(dto.CurrentCity, employee.CurrentAddress.City) ? dto.CurrentCity : null;
        NewCurrentProvince = IsChanged(dto.CurrentProvince, employee.CurrentAddress.Province) ? dto.CurrentProvince : null;
        NewCurrentZipCode = IsChanged(dto.CurrentPostalCode, employee.CurrentAddress.ZipCode) ? dto.CurrentPostalCode : null;

        NewResidentialStreetAddress = IsChanged(dto.ResidentialStreetAddress, employee.ResidentialAddress.Street) ? dto.ResidentialStreetAddress : null;
        NewResidentialCity = IsChanged(dto.ResidentialCity, employee.ResidentialAddress.City) ? dto.ResidentialCity : null;
        NewResidentialProvince = IsChanged(dto.ResidentialProvince, employee.ResidentialAddress.Province) ? dto.ResidentialProvince : null;
        NewResidentialZipCode = IsChanged(dto.ResidentialPostalCode, employee.ResidentialAddress.ZipCode) ? dto.ResidentialPostalCode : null;

        NewPhoneNumber = IsChanged(dto.PhoneNumber, employee.PhoneNumber) ? dto.PhoneNumber : null;

        var currentContact = employee.EmergencyContacts.FirstOrDefault();
        NewEmergencyContactName = IsChanged(dto.EmergencyContactName, currentContact?.Name) ? dto.EmergencyContactName : null;
        NewEmergencyContactPhone = IsChanged(dto.EmergencyContactPhone, currentContact?.PhoneNumber) ? dto.EmergencyContactPhone : null;
        NewEmergencyContactRelationship = IsChanged(dto.EmergencyContactRelationship, currentContact?.Relationship) ? dto.EmergencyContactRelationship : null;
    
        Status = 0; // Pending
        CreatedBy = actionerId;
        ModifiedBy = actionerId;
    }

    private static bool IsChanged(string? newValue, string? currentValue)
    {
        if (string.IsNullOrWhiteSpace(newValue)) return false;
        return !string.Equals(newValue.Trim(), currentValue?.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsChanged<T>(T? newValue, T currentValue) where T : struct
    {
        if (!newValue.HasValue) return false;
        return !newValue.Value.Equals(currentValue);
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