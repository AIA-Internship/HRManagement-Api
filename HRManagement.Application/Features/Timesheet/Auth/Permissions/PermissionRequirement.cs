using HRManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace HRManagement.Application.Auth.Permission;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}




