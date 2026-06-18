namespace HRManagement.Domain.Models.Tables;

public class Roles
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public bool IsDeleted { get; private set; }
    public int CreatedBy { get; private set; }
    public DateTime CreatedUtcDate { get; private set; }
    public int ModifiedBy { get; private set; }
    public DateTime ModifiedUtcDate { get; private set; }

    public virtual ICollection<Users> Users { get; set; } = new List<Users>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public virtual Users? Actioner { get; set; } = null!;

    protected Roles() { }
}