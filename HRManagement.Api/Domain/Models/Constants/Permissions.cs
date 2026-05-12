namespace HRManagement.Api.Domain.Models.Constants;

public static class Permissions
{
    public static class Employees
    {
        public const string View = "Permissions.Employees.View";
        public const string Create = "Permissions.Employees.Create";
        public const string Edit = "Permissions.Employees.Edit";
        public const string Delete = "Permissions.Employees.Delete";
    }

    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Edit = "Permissions.Users.Edit";
    }
}
