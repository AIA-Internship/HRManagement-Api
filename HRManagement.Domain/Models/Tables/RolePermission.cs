namespace HRManagement.Domain.Models.Tables;

public class RolePermission : BaseTable
{
    public int Id { get; private set; }
    public int RoleId { get; private set; }
    public int PermissionId { get; private set; }

    public virtual Roles Role { get; private set; } = null!;
    public virtual Permission Permission { get; private set; } = null!;

    protected RolePermission() { }
}