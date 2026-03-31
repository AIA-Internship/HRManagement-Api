namespace HRManagement.Api.Domain.Models.Tables;

public class User : BaseTableModel
{
    public int Id { get; private set; }
    public string EmployeeEmail { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public int Role { get; private set; }
    
    protected User() { }
    
    public User(string email, string passwordHash, int role, long actionerId)
    {
        EmployeeEmail = email;
        PasswordHash = passwordHash;
        Role = role;
        
        CreatedBy = actionerId;
        ModifiedBy = actionerId;
    }

    public void ChangePassword(string passwordHash, long actionerId)
    {
        if(string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password cannot be empty");
        this.PasswordHash = passwordHash;
        MarkAsModified(actionerId);
    }

    public void ChangeRole(int role, long actionerId)
    {
        Role = role;
        MarkAsModified(actionerId);
    }
}