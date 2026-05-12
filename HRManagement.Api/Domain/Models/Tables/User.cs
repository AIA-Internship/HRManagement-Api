using HRManagement.Api.Domain.Models.Tables.MasterRole;

namespace HRManagement.Api.Domain.Models.Tables;

public class User : BaseTableModel
{
    public int Id { get; private set; }
    public string EmployeeEmail { get; private set; }
    public string PasswordHash { get; private set; }
    public int RoleId { get; private set; }
    public Role SystemRole { get; private set; }
    
    protected User() { }
    
    public User(string email, string passwordHash, int roleId, long actionerId)
    {
        EmployeeEmail = email;
        PasswordHash = passwordHash;
        RoleId = roleId;
        
        CreatedBy = actionerId;
        ModifiedBy = actionerId;
    }

    public void ChangePassword(string passwordHash, long actionerId)
    {
        if(string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password cannot be empty");
        this.PasswordHash = passwordHash;
        MarkAsModified(actionerId);
    }

    public void ChangeRole(int roleId, long actionerId)
    {
        RoleId = roleId;
        MarkAsModified(actionerId);
    }
}