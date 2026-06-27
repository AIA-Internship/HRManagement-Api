namespace HRManagement.Domain.Models.Tables;

public class AssessmentGroupMember : BaseTable
{
    public int Id { get; private set; }
    public int GroupId { get; private set; }
    public int EmployeeId { get; private set; }

    public AssessmentGroup Group { get; private set; } = null!;

    public Employee Employee { get; private set; } = null!;

    protected AssessmentGroupMember() { }

    public AssessmentGroupMember(
        int groupId,
        int employeeId,
        int actionerId)
    {
        GroupId = groupId;
        EmployeeId = employeeId;
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }
}