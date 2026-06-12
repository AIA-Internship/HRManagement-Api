namespace HRManagement.Domain.Models.Tables;

public class Users
{
    public int Id { get; private set; }
    public string EmployeeEmail { get; private set; }
    public string PasswordHash { get; private set; }
    public int RoleId { get; private set; }
    public DateTime LastLoginTime { get; private set; }


    public bool IsDeleted { get; private set; }
    public int CreatedBy { get; private set; }
    public DateTime CreatedUtcDate { get; private set; }
    public int ModifiedBy { get; private set; }
    public DateTime ModifiedUtcDate { get; private set; }

    public Roles Role { get; private set; } = null!;
    public virtual Users? Actioner { get; set; } = null!;

    protected Users() { }
    
    public Users(string email, string passwordHash, int roleId, int actionerId)
    {
        EmployeeEmail = email;
        PasswordHash = passwordHash;
        RoleId = roleId;

        IsDeleted = false;
        CreatedBy = actionerId;
        CreatedUtcDate = DateTime.UtcNow;
        ModifiedBy = actionerId;
        ModifiedUtcDate = DateTime.UtcNow;
    }

    public void ChangePassword(string passwordHash, int actionerId)
    {
        if(string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password cannot be empty");

        PasswordHash = passwordHash;

        ModifiedBy = actionerId;
        ModifiedUtcDate = DateTime.UtcNow;
    }

    public void ChangeRole(int roleId, int actionerId)
    {
        RoleId = roleId;

        ModifiedBy = actionerId;
        ModifiedUtcDate = DateTime.UtcNow;
    }

    public void SetLastLogin()
    {
        LastLoginTime = DateTime.UtcNow;
    }
}