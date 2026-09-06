using HRManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace HRManagement.Application.Auth.Permission;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: permission)
    {
    }
}




