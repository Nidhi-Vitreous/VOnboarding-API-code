namespace Vitreous.Onboarding.Api.Middleware;

/// <summary>
/// Global exception handling middleware (implement during API hardening).
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context) => await _next(context);
}
