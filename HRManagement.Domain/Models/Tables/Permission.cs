namespace HRManagement.Domain.Models.Tables;

public class Permission : BaseTable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermission { get; set; } = new List<RolePermission>();

    protected Permission() { }
}
