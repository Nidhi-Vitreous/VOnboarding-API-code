using System.Net;
using System.Text.Json;
using Vitreous.Onboarding.Application.Common;

namespace Vitreous.Onboarding.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BusinessRuleException ex)
        {
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, ex.Message, ex.Details);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception processing {Method} {Path}.", context.Request.Method, context.Request.Path);
            await WriteErrorResponseAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message,
        string[]? details = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new ErrorResponse { Message = message, Details = details };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
