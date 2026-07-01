using Microsoft.AspNetCore.Authorization;
using Vitreous.Onboarding.Api.Authorization;

namespace Vitreous.Onboarding.Api.Configuration;

public static class AuthorizationConfiguration
{
    public static IServiceCollection AddOnboardingAuthorization(this IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, OnboardingPermissionAuthorizationHandler>();
        services.AddScoped<IAuthorizationHandler, SystemPermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            OnboardingAuthorizationPolicies.Register(options);
            SystemPermissionPolicies.Register(options);
        });

        return services;
    }
}
