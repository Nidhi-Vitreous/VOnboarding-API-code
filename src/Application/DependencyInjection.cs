using Microsoft.Extensions.DependencyInjection;
using Vitreous.Onboarding.Application.Auth;
using Vitreous.Onboarding.Application.Interfaces;
using Vitreous.Onboarding.Application.Roles;
using Vitreous.Onboarding.Application.Users;

namespace Vitreous.Onboarding.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        return services;
    }
}
