using System.Xml.Linq;

namespace HRManagement.Domain.Models.Tables;

public class EmergencyContact : BaseTable
{
    public int Id { get; private set; }
    public int EmployeeId { get; private set; }
    public string ContactName { get; private set; } =  string.Empty;
    public string ContactPhone { get; private set; } = string.Empty;
    public string ContactRelationship { get; private set; } = string.Empty;

    public Employee Employee { get; private set; } = null!;

    protected EmergencyContact() { }

    public EmergencyContact(int employeeId, 
        string contactName, 
        string contactPhone, 
        string contactRelation, 
        int actionerId)
    {
        EmployeeId = employeeId;
        ContactName = contactName;
        ContactPhone = contactPhone;
        ContactRelationship = contactRelation;

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate(string name, string phoneNumber, string relationship, int actionerId)
    {
        if (!string.IsNullOrWhiteSpace(name))
            ContactName = name;

        if (!string.IsNullOrWhiteSpace(phoneNumber))
            ContactPhone = phoneNumber;

        if (!string.IsNullOrWhiteSpace(relationship))
            ContactRelationship = relationship;

        MarkAsModified(actionerId);
    }
}