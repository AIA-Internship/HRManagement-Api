namespace HRManagement.Api.Domain.Models.Tables;

public class EmploymentInformation : BaseTableModel
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public int EmploymentStatus { get; set; }
    public DateTime StartDate { get; set; }
    public int EmploymentType { get; set; }
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string SupervisorName { get; set; } = string.Empty;
    
    protected EmploymentInformation() { }
    
    public EmploymentInformation(long actionerId)
    {
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void UpdateDetails(int? status, DateTime? startDate, int? type, string? department, string? position, string? supervisorName, long actionerId)
    {
        EmploymentStatus = status ?? EmploymentStatus;
        StartDate = startDate ?? StartDate;
        EmploymentType = type ?? EmploymentType;
        Department = UseIfProvided(department, Department);
        Position = UseIfProvided(position, Position);
        SupervisorName = UseIfProvided(supervisorName, SupervisorName);

        MarkAsModified(actionerId); 
    }
    
    private static string UseIfProvided(string? newValue, string currentValue) =>
        string.IsNullOrWhiteSpace(newValue) ? currentValue : newValue;
}