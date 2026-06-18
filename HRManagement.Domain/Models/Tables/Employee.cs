namespace HRManagement.Domain.Models.Tables;

public class Employee : BaseTable
{
    public int Id { get; private set; }
    public string NIK { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string Gender { get; private set; } = string.Empty;
    public string PersonalEmail { get; private set; } = string.Empty;
    public string EmployeeEmail { get; private set; } = string.Empty;

    public string BirthPlace { get; private set; } = string.Empty;
    public DateTime BirthDate { get; private set; }
    public int MaritalStatus { get; private set; }

    public string CurrentAddress { get; private set; } = string.Empty;
    public string CurrentCity { get; private set; } = string.Empty;
    public string CurrentProvince { get; private set; } = string.Empty;
    public string CurrentPostalCode { get; private set; } = string.Empty;

    public string ResidentialAddress { get; private set; } = string.Empty;
    public string ResidentialCity { get; private set; } = string.Empty;
    public string ResidentialProvince { get; private set; } = string.Empty;
    public string ResidentialPostalCode { get; private set; } = string.Empty;

    public string MobilePhone { get; private set; } = string.Empty;
    public int RoleId { get; private set; }

    public bool IsActive { get; private set; }

    public Roles Role { get;  private set; } = null!;

    public EmploymentInformation? EmploymentInformation { get; private set; } = null!;
    public EmergencyContact? EmergencyContact { get; private set; } = null!;

    protected Employee() { }
    
    public Employee(
        string fullName,
        string gender,
        string personalEmail,
        string employeeEmail,
        string mobilePhone,
        string nik,
        string placeOfBirth,
        DateTime dateOfBirth,
        int maritalStatus,
        Address currentAddress,
        Address residentialAddress,
        int roleId,
        int actionerId,
        EmploymentInformation? employmentInformation,
        EmergencyContact? emergencyContact)
    {
        FullName = fullName;
        Gender = gender;
        PersonalEmail = personalEmail;
        EmployeeEmail = employeeEmail;
        MobilePhone = mobilePhone;
        NIK = nik;
        BirthPlace = placeOfBirth;
        BirthDate = dateOfBirth;
        MaritalStatus = maritalStatus;
        CurrentAddress = currentAddress.Street;
        CurrentCity = currentAddress.City;
        CurrentProvince = currentAddress.Province;
        CurrentPostalCode = currentAddress.ZipCode;

        ResidentialAddress = residentialAddress.Street;
        ResidentialCity = residentialAddress.City;
        ResidentialProvince = residentialAddress.Province;
        ResidentialPostalCode = residentialAddress.ZipCode;

        RoleId = roleId;
        IsActive = true;

        if (employmentInformation != null)
            EmploymentInformation = employmentInformation;

        if (emergencyContact != null)
            EmergencyContact = emergencyContact;

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate(EmployeeUpdateRequest request, int actionerId)
    {
        FullName = UseIfProvided(request.NewFullName, FullName);
        Gender = request.NewGender.HasValue ? (request.NewGender.Value == 1 ? "F" : "M") : Gender;

        CurrentAddress = UseIfProvided(request.NewCurrentStreetAddress, CurrentAddress);
        CurrentCity = UseIfProvided(request.NewCurrentCity, CurrentCity);
        CurrentProvince = UseIfProvided(request.NewCurrentProvince, CurrentProvince);
        CurrentPostalCode = UseIfProvided(request.NewCurrentZipCode, CurrentPostalCode);

        ResidentialAddress = UseIfProvided(request.NewResidentialStreetAddress, ResidentialAddress);
        ResidentialCity = UseIfProvided(request.NewResidentialCity, ResidentialCity);
        ResidentialProvince = UseIfProvided(request.NewResidentialProvince, ResidentialProvince);
        ResidentialPostalCode = UseIfProvided(request.NewResidentialZipCode, ResidentialPostalCode);

        MobilePhone = UseIfProvided(request.NewPhoneNumber, MobilePhone);
        PersonalEmail = UseIfProvided(request.NewPersonalEmail, PersonalEmail);
        BirthPlace = UseIfProvided(request.NewPlaceOfBirth, BirthPlace);
        BirthDate = request.NewDateOfBirth ?? BirthDate;
        MaritalStatus = request.NewMaritalStatus ?? MaritalStatus;
        
        if (!string.IsNullOrWhiteSpace(request.NewEmergencyContactName))
        {
            if (EmergencyContact != null)
            {
                EmergencyContact.ApplyUpdate(
                    request.NewEmergencyContactName,
                    request.NewEmergencyContactPhone ?? string.Empty,
                    request.NewEmergencyContactRelationship ?? string.Empty,
                    actionerId
                );
            }
            else
            {
                this.EmergencyContact = new EmergencyContact(
                    employeeId: this.Id,
                    request.NewEmergencyContactName,
                    request.NewEmergencyContactPhone ?? string.Empty,
                    request.NewEmergencyContactRelationship ?? string.Empty,
                    actionerId
                );
            }
        }

        /* EMPLOYMENT INFORMATION tidak sekalian di update ?? */

        MarkAsModified(actionerId);
    }
}
