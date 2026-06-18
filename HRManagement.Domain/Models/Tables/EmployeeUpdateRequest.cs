using System.Text.Json;

using HRManagement.Domain.Models.Payload;
using HRManagement.Domain.Models.Response;

namespace HRManagement.Domain.Models.Tables;

// One captured field change (old -> new) snapshotted at submission time.
public record ChangeSnapshotItem(string Field, string? Previous, string? Updated);

public class EmployeeUpdateRequest : BaseTable
{
    public int Id { get; private set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; private set; } = null!;
    
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
    public string? HrReason { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Snapshot of old->new values at submission, so history survives after approval. JSON array of ChangeSnapshotItem.
    public string? ChangesJson { get; private set; }

    protected EmployeeUpdateRequest() { }
    
    public EmployeeUpdateRequest(EmployeeProfileResponseDto employee, UpdateEmployeePayload dto, int actionerId, string? newMaritalStatusName = null)
    {
        EmployeeId = employee.Id;
        NewFullName = IsChanged(dto.FullName, employee.FullName) ? dto.FullName : null;
        NewGender = IsChanged(dto.Gender, employee.Gender) ? GenderToInt(dto.Gender) : null;
        NewPersonalEmail = IsChanged(dto.PersonalEmail, employee.PersonalEmail) ? dto.PersonalEmail : null;
        NewPlaceOfBirth = IsChanged(dto.PlaceOfBirth, employee.PlaceOfBirth) ? dto.PlaceOfBirth : null;
        NewNik = IsChanged(dto.Nik, employee.Nik) ? dto.Nik : null;
        NewDateOfBirth = IsChanged(dto.DateOfBirth, employee.DateOfBirth) ? dto.DateOfBirth : null;
        NewMaritalStatus = IsChanged(dto.MaritalStatus, employee.MaritalStatus) ? dto.MaritalStatus : null;
        
        NewCurrentStreetAddress = IsChanged(dto.CurrentStreetAddress, employee.CurrentStreetAddress) ? dto.CurrentStreetAddress : null;
        NewCurrentCity = IsChanged(dto.CurrentCity, employee.CurrentCity) ? dto.CurrentCity : null;
        NewCurrentProvince = IsChanged(dto.CurrentProvince, employee.CurrentProvince) ? dto.CurrentProvince : null;
        NewCurrentZipCode = IsChanged(dto.CurrentPostalCode, employee.CurrentPostalCode) ? dto.CurrentPostalCode : null;

        NewResidentialStreetAddress = IsChanged(dto.ResidentialStreetAddress, employee.ResidentialStreetAddress) ? dto.ResidentialStreetAddress : null;
        NewResidentialCity = IsChanged(dto.ResidentialCity, employee.ResidentialCity) ? dto.ResidentialCity : null;
        NewResidentialProvince = IsChanged(dto.ResidentialProvince, employee.ResidentialProvince) ? dto.ResidentialProvince : null;
        NewResidentialZipCode = IsChanged(dto.ResidentialPostalCode, employee.ResidentialPostalCode) ? dto.ResidentialPostalCode : null;

        NewPhoneNumber = IsChanged(dto.PhoneNumber, employee.PhoneNumber) ? dto.PhoneNumber : null;

        var currentContact = employee.EmergencyContact;
        NewEmergencyContactName = IsChanged(dto.EmergencyContactName, currentContact?.Name) ? dto.EmergencyContactName : null;
        NewEmergencyContactPhone = IsChanged(dto.EmergencyContactPhone, currentContact?.PhoneNumber) ? dto.EmergencyContactPhone : null;
        NewEmergencyContactRelationship = IsChanged(dto.EmergencyContactRelationship, currentContact?.Relationship) ? dto.EmergencyContactRelationship : null;
    
        Status = 0; // Pending
        CreatedAt = DateTime.UtcNow;
        CreatedBy = actionerId;
        ModifiedBy = actionerId;

        ChangesJson = BuildChangesSnapshot(employee, dto, newMaritalStatusName);
    }

    // Capture display old->new for each changed field, in the same order the UI lists them.
    private static string? BuildChangesSnapshot(EmployeeProfileResponseDto e, UpdateEmployeePayload dto, string? newMaritalStatusName)
    {
        static string FmtDate(DateTime? d) => d.HasValue ? d.Value.ToString("dd/MM/yyyy") : "";
        var items = new List<ChangeSnapshotItem>();

        void Add(string field, bool changed, string? oldVal, string? newVal)
        {
            if (changed) items.Add(new ChangeSnapshotItem(field, oldVal, newVal));
        }

        Add("newFullName", IsChanged(dto.FullName, e.FullName), e.FullName, dto.FullName);
        Add("newGender", IsChanged(dto.Gender, e.Gender), e.Gender, dto.Gender);
        Add("newPersonalEmail", IsChanged(dto.PersonalEmail, e.PersonalEmail), e.PersonalEmail, dto.PersonalEmail);
        Add("newPlaceOfBirth", IsChanged(dto.PlaceOfBirth, e.PlaceOfBirth), e.PlaceOfBirth, dto.PlaceOfBirth);
        Add("newNik", IsChanged(dto.Nik, e.Nik), e.Nik, dto.Nik);
        Add("newDateOfBirth", IsChanged(dto.DateOfBirth, e.DateOfBirth), FmtDate(e.DateOfBirth), FmtDate(dto.DateOfBirth));
        Add("newMaritalStatus", IsChanged(dto.MaritalStatus, e.MaritalStatus), e.MaritalStatusName, newMaritalStatusName ?? dto.MaritalStatus?.ToString());

        Add("newCurrentStreetAddress", IsChanged(dto.CurrentStreetAddress, e.CurrentStreetAddress), e.CurrentStreetAddress, dto.CurrentStreetAddress);
        Add("newCurrentCity", IsChanged(dto.CurrentCity, e.CurrentCity), e.CurrentCity, dto.CurrentCity);
        Add("newCurrentProvince", IsChanged(dto.CurrentProvince, e.CurrentProvince), e.CurrentProvince, dto.CurrentProvince);
        Add("newCurrentPostalCode", IsChanged(dto.CurrentPostalCode, e.CurrentPostalCode), e.CurrentPostalCode, dto.CurrentPostalCode);

        Add("newResidentialStreetAddress", IsChanged(dto.ResidentialStreetAddress, e.ResidentialStreetAddress), e.ResidentialStreetAddress, dto.ResidentialStreetAddress);
        Add("newResidentialCity", IsChanged(dto.ResidentialCity, e.ResidentialCity), e.ResidentialCity, dto.ResidentialCity);
        Add("newResidentialProvince", IsChanged(dto.ResidentialProvince, e.ResidentialProvince), e.ResidentialProvince, dto.ResidentialProvince);
        Add("newResidentialPostalCode", IsChanged(dto.ResidentialPostalCode, e.ResidentialPostalCode), e.ResidentialPostalCode, dto.ResidentialPostalCode);

        Add("newPhoneNumber", IsChanged(dto.PhoneNumber, e.PhoneNumber), e.PhoneNumber, dto.PhoneNumber);

        var c = e.EmergencyContact;
        Add("newEmergencyContactName", IsChanged(dto.EmergencyContactName, c?.Name), c?.Name, dto.EmergencyContactName);
        Add("newEmergencyContactPhone", IsChanged(dto.EmergencyContactPhone, c?.PhoneNumber), c?.PhoneNumber, dto.EmergencyContactPhone);
        Add("newEmergencyContactRelationship", IsChanged(dto.EmergencyContactRelationship, c?.Relationship), c?.Relationship, dto.EmergencyContactRelationship);

        return items.Count > 0
            ? JsonSerializer.Serialize(items, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
            : null;
    }

    private static int? GenderToInt(string? gender) =>
        string.Equals(gender?.Trim(), "Female", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

    public void Reject(string? reason, int hrActionerId)
    {
        Status = 2; // Rejected
        HrReason = reason ?? "Rejected by Supervisor";
        ModifiedBy = hrActionerId;
        ModifiedUtcDate = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public void Approve(string? reason, int hrActionerId)
    {
        Status = 1; // Approved
        HrReason = reason ?? "Approved by Supervisor";
        ModifiedBy = hrActionerId;
        ModifiedUtcDate = DateTime.UtcNow;
    }
}