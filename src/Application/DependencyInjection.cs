using Microsoft.Extensions.DependencyInjection;
using Vitreous.Onboarding.Application.Auth;
using Vitreous.Onboarding.Application.Authorization;
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
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IDepartmentResolver, DepartmentResolver>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<IPermissionAuthorizationService, PermissionAuthorizationService>();
        return services;
    }
}
