using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.Api.Domain.Models.Table
{
    public class RoleModel : BaseTableModel
    {
        [Key]
        public int RoleId { get; set; }
        public string? RoleName { get; set; }

        public virtual ICollection<UserModel> Users { get; set; }
    }
}
