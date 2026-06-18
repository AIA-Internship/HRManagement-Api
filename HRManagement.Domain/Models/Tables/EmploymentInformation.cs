namespace HRManagement.Domain.Models.Tables;

public class EmploymentInformation : BaseTable
{
    public int Id { get; private set; }
    public int EmployeeId { get; private set; }
    public int StatusCode { get; private set; }
    public DateTime StartDate { get; private set; }
    public int TypeCode { get; private set; }
    public string? DepartmentName { get; private set; }
    public string? PositionName { get; private set; }
    public string? DisplayId { get; private set; }
    public int? SupervisorId { get; private set; }
    public string? SupervisorName { get; private set; }

    public Employee Employee { get; private set; } = null!;

    protected EmploymentInformation() { }
    
    public EmploymentInformation(
        int employeeId,
        int statusCode, 
        DateTime startDate, 
        int type, 
        string? department, 
        string? position,
        string? displayId,
        int? supervisorId,
        string? supervisorName,
        int actionerId)
    {
        EmployeeId = employeeId;
        StatusCode = statusCode;
        StartDate = startDate;
        TypeCode = type;
        DepartmentName = department;
        PositionName = position;
        DisplayId = displayId;
        SupervisorId = supervisorId;
        SupervisorName = supervisorName;

        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void UpdateDetails(int? status, DateTime? startDate, int? type, string? department, string? position, 
        int? supervisorId, string? employeeDisplayId, int actionerId)
    {
        StatusCode = status ?? StatusCode;
        StartDate = startDate ?? StartDate;
        TypeCode = type ?? TypeCode;
        DepartmentName = UseIfProvided(department, DepartmentName ?? string.Empty);
        PositionName = UseIfProvided(position, PositionName ?? string.Empty);
        SupervisorId = supervisorId ?? SupervisorId;
        DisplayId = UseIfProvided(employeeDisplayId, DisplayId ?? string.Empty);

        MarkAsModified(actionerId); 
    }
}