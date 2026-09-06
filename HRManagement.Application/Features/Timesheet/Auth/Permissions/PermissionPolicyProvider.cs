using HRManagement.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HRManagement.Application.Auth.Permission;

public class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = await base.GetPolicyAsync(policyName);

        if (policy == null)
        {
            policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();
        }

        return policy;
    }
}




