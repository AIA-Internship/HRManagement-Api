using System.ComponentModel.DataAnnotations;

namespace HRManagement.Api.Domain.Models.Table
{
    public class UserModel : BaseTableModel
    {
        [Key]
        public int UserId { get; set; }
        public Guid? MemberId { get; set; }
        public string? UserMobile { get; set; }
        public string? UserEmail { get; set; }
        public string? PasswordHash { get; set; }
        public int? RoleId { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public virtual RoleModel Role { get; set; }

        public UserModel() { }

        public UserModel(string? userEmail, string? passwordHash, long actioner)
        {
            UserEmail = userEmail;
            PasswordHash = passwordHash;
            IsDeleted = false;
            CreatedBy = actioner;
            CreatedUtcDate = DateTime.UtcNow;
            ModifiedBy = actioner;
            ModifiedUtcDate = DateTime.UtcNow;
        }

        public UserModel(string? userEmail, string? passwordHash, int? roleId, long actioner)
        {
            UserEmail = userEmail;
            PasswordHash = passwordHash;
            RoleId = roleId;
            IsDeleted = false;
            CreatedBy = actioner;
            CreatedUtcDate = DateTime.UtcNow;
            ModifiedBy = actioner;
            ModifiedUtcDate = DateTime.UtcNow;
        }

        public UserModel(EmployeeModel payload)
        {
            RoleId = 1;
            IsDeleted = false;
            CreatedUtcDate = DateTime.UtcNow;
            ModifiedUtcDate = DateTime.UtcNow;
        }

        public void UpdateData(string? userEmail, int? roleId, long actioner)
        {
            UserEmail = userEmail;
            RoleId = roleId;
            ModifiedBy = actioner;
            ModifiedUtcDate = DateTime.UtcNow;
        }

        public void UpdatePhone(string? userMobile)
        {
            UserMobile = userMobile;
            ModifiedUtcDate = DateTime.UtcNow;
        }

        public void ChangePassword(string? passwordHash, long actioner)
        {
            PasswordHash = passwordHash;
            IsDeleted = false;
            ModifiedBy = actioner;
            ModifiedUtcDate = DateTime.UtcNow;
        }

        public void SetLastLogin(long actioner)
        {
            LastLoginTime = DateTime.UtcNow.AddHours(7);
            //ModifiedBy = actioner;
            //ModifiedUtcDate = DateTime.UtcNow;
        }

        public void SetDelete(long actioner)
        {
            IsDeleted = true;
            ModifiedBy = actioner;
            ModifiedUtcDate = DateTime.UtcNow;
        }
    }
}
