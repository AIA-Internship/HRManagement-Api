namespace HRManagement.Api.Domain.Models.Tables.MasterRole;

public class Role : BaseTableModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>(); 
}