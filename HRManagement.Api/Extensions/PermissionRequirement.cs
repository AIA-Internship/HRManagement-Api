using Microsoft.AspNetCore.Authorization;

namespace HRManagement.Application.Auth.Permissions;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
