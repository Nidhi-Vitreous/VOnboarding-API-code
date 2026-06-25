namespace Vitreous.Onboarding.Api.Configuration;

/// <summary>
/// API-layer dependency injection registration.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register API services, auth, CORS, etc.
        return services;
    }
}
