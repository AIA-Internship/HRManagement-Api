using Microsoft.AspNetCore.Authorization;

namespace HRManagement.Api.Application.Auth.Permissions;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
