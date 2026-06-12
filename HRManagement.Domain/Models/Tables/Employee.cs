namespace HRManagement.Domain.Models.Tables;

public class Employee : BaseTable
{
    public int Id { get; private set; }
    public string NIK { get; private set; }
    public string FullName { get; private set; }
    public int Gender { get; private set; }
    public string PersonalEmail { get; private set; }
    public string EmployeeEmail { get; private set; }
    
    public string PlaceOfBirth { get; private set; }
    public DateTime DateOfBirth { get; private set; }
    public int MaritalStatus { get; private set; }
    public Address CurrentAddress { get; private set; } = new Address();
    public Address ResidentialAddress { get; private set; } = new Address();
    public string PhoneNumber { get; private set; }
    public bool IsActive { get; private set; }
    public int RoleId { get;  private set; }
    public Roles SystemRole { get;  private set; }
    // public int Role { get; private set; }
    public EmploymentInformation? EmploymentInformation { get; private set; }
    public ICollection<EmergencyContact> EmergencyContacts { get; private set; } = new List<EmergencyContact>();
    
    protected Employee() { }
    
    public Employee(
        string fullName,
        int gender,
        string personalEmail,
        string employeeEmail,
        string phoneNumber,
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
        PhoneNumber = phoneNumber;
        Nik = nik;
        PlaceOfBirth = placeOfBirth;
        DateOfBirth = dateOfBirth;
        MaritalStatus = maritalStatus;
        CurrentAddress = currentAddress;
        ResidentialAddress = residentialAddress;
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
        
        CurrentAddress.Street = UseIfProvided(request.NewCurrentStreetAddress, CurrentAddress.Street);
        CurrentAddress.City = UseIfProvided(request.NewCurrentCity, CurrentAddress.City);
        CurrentAddress.Province = UseIfProvided(request.NewCurrentProvince, CurrentAddress.Province);
        CurrentAddress.ZipCode = UseIfProvided(request.NewCurrentZipCode, CurrentAddress.ZipCode);

        ResidentialAddress.Street = UseIfProvided(request.NewResidentialStreetAddress, ResidentialAddress.Street);
        ResidentialAddress.City = UseIfProvided(request.NewResidentialCity, ResidentialAddress.City);
        ResidentialAddress.Province = UseIfProvided(request.NewResidentialProvince, ResidentialAddress.Province);
        ResidentialAddress.ZipCode = UseIfProvided(request.NewResidentialZipCode, ResidentialAddress.ZipCode);

        PhoneNumber = UseIfProvided(request.NewPhoneNumber, PhoneNumber);
        PersonalEmail = UseIfProvided(request.NewPersonalEmail, PersonalEmail);
        PlaceOfBirth = UseIfProvided(request.NewPlaceOfBirth, PlaceOfBirth);
        DateOfBirth = request.NewDateOfBirth ?? DateOfBirth;
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

    private static string UseIfProvided(string? newValue, string currentValue) =>
        string.IsNullOrWhiteSpace(newValue) ? currentValue : newValue;

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
