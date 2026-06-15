namespace HRManagement.Domain.Models.Tables;

public class Employee : BaseTable
{
    public int Id { get; private set; }
    public string NIK { get; private set; }
    public string FullName { get; private set; }
    public string Gender { get; private set; }
    public string PersonalEmail { get; private set; }
    public string EmployeeEmail { get; private set; }
    
    public string BirthPlace { get; private set; }
    public DateTime BirthDate { get; private set; }
    public int MaritalStatus { get; private set; }

    public string CurrentAddress { get; private set; }
    public string CurrentCity { get; private set; }
    public string CurrentProvince { get; private set; }
    public string CurrentPostalCode { get; private set; }

    public string ResidentialAddress { get; private set; }
    public string ResidentialCity { get; private set; }
    public string ResidentialProvince { get; private set; }
    public string ResidentialPostalCode { get; private set; }

    public string MobilePhone { get; private set; }
    public int RoleId { get; private set; }

    public bool IsActive { get; private set; }

    public Roles Role { get;  private set; }

    public ICollection<EmploymentInformation> EmploymentInformations { get; set; } = new List<EmploymentInformation>();
    public ICollection<EmergencyContact?> EmergencyContacts { get; private set; } = new List<EmergencyContact?>();
    
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
        long actionerId,
        EmploymentInformation? employmentInformation = null,
        IEnumerable<EmergencyContact>? emergencyContacts = null)
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
        {
            EmploymentInformation = employmentInformation;
        }

        if (emergencyContacts != null)
        {
            foreach (var contact in emergencyContacts)
            {
                EmergencyContacts.Add(new EmergencyContact
                {
                    Name = contact.Name,
                    Relationship = contact.Relationship,
                    PhoneNumber = contact.PhoneNumber,
                    CreatedBy = actionerId,
                    ModifiedBy = actionerId
                });
            }
        }

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate(EmployeeUpdateRequest request, long actionerId)
    {
        FullName = UseIfProvided(request.NewFullName, FullName);
        Gender = request.NewGender ?? Gender;

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
            var contact = EmergencyContacts.FirstOrDefault();
            if (contact == null)
            {
                contact = new EmergencyContact { EmployeeId = Id };
                EmergencyContacts.Add(contact);
            }

            contact.Name = request.NewEmergencyContactName;
            contact.PhoneNumber = UseIfProvided(request.NewEmergencyContactPhone, contact.PhoneNumber);
            contact.Relationship = UseIfProvided(request.NewEmergencyContactRelationship, contact.Relationship);
        }

        MarkAsModified(actionerId);
    }

    private static string UseIfProvided(string? newValue, string currentValue) => string.IsNullOrWhiteSpace(newValue) ? currentValue : newValue;

    public void UpdateEmploymentInfo(int? status, DateTime? startDate, int? type, string? department, string? position, int? supervisorId, string? employeeDisplayId, long actionerId)
    {
        if (EmploymentInformation == null)
        {
            EmploymentInformation = new EmploymentInformation(actionerId);
        }
        
        EmploymentInformation.UpdateDetails(status, startDate, type, department, position, supervisorId, employeeDisplayId, actionerId);
    
        MarkAsModified(actionerId);
    }
}
