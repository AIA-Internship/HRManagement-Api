using Microsoft.AspNetCore.Authorization;

namespace HRManagement.Api.Application.Auth.Permissions;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
    }
}
