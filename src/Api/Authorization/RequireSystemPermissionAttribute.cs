using Microsoft.AspNetCore.Authorization;

namespace Vitreous.Onboarding.Api.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequireSystemPermissionAttribute : AuthorizeAttribute
{
    public RequireSystemPermissionAttribute(string systemPermission)
        : base(SystemPermissionPolicies.For(systemPermission))
    {
    }
}
