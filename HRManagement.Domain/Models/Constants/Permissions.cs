namespace HRManagement.Domain.Models.Constants;

public static class Permission
{
    public static class Employee
    {
        public const string View = "Permissions.Employee.View";
        public const string Create = "Permissions.Employee.Create";
        public const string Edit = "Permissions.Employee.Edit";
        public const string Delete = "Permissions.Employee.Delete";
    }

    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Edit = "Permissions.Users.Edit";
    }
}


