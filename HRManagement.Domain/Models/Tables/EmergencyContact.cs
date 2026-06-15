namespace HRManagement.Domain.Models.Tables;

public class EmergencyContact : BaseTable
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string ContactName { get; set; } =  string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactRelationship { get; set; } = string.Empty;

    public Employee Employee { get; set; } = null!;

    protected EmergencyContact() { }
}