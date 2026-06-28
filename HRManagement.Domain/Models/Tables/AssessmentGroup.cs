namespace HRManagement.Domain.Models.Tables;

public class AssessmentGroup : BaseTable
{
    public int Id { get; private set; }
    public int AssessmentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public Assessment Assessment { get; private set; } = null!;
    public virtual ICollection<AssessmentGroupMember> Members { get; private set; } = new List<AssessmentGroupMember>();

    protected AssessmentGroup() { }

    public AssessmentGroup(
        int assessmentId,
        string name,
        string? description,
        int actionerId)
    {
        AssessmentId = assessmentId;
        Name = name;
        Description = description;
        MarkAsCreated(actionerId);
        MarkAsModified(actionerId);
    }

    public void ApplyUpdate(string? name, string? description, int actionerId)
    {
        Name = UseIfProvided(name, Name);
        Description = UseIfProvided(description, Description ?? "");
        MarkAsModified(actionerId);
    }
}