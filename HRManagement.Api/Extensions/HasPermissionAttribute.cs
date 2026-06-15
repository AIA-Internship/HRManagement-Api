using Microsoft.AspNetCore.Authorization;

namespace HRManagement.Application.Auth.Permissions;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
    }
}
