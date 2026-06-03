namespace HRManagement.Api.Domain.Models.Tables.MasterRole;

public class Permission : BaseTableModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}