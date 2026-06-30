using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Vitreous.Onboarding.Api.Configuration;

public static class SwaggerConfiguration
{
    public const string DocumentName = "v1";

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(DocumentName, new OpenApiInfo
            {
                Title = "Vitreous Onboarding API",
                Version = "v1",
                Description =
                    "Development API documentation. Endpoints are discovered automatically from controllers.",
            });

            options.TagActionsBy(api =>
            {
                var controller = api.ActionDescriptor.RouteValues.TryGetValue("controller", out var name)
                    ? name
                    : "Default";

                return [controller];
            });

            options.OrderActionsBy(api => api.RelativePath);

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter a valid JWT access token. Example: eyJhbGciOiJIUzI1NiIs...",
            });

            options.OperationFilter<AuthorizeCheckOperationFilter>();
            options.SchemaFilter<SnakeCaseSchemaFilter>();
        });

        return services;
    }

    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{DocumentName}/swagger.json", "Vitreous Onboarding API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "Vitreous Onboarding API";
            options.DisplayRequestDuration();
        });

        return app;
    }

    private sealed class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var actionAttributes = context.MethodInfo.GetCustomAttributes(true);
            var controllerAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true) ?? [];

            var hasAuthorize = controllerAttributes.OfType<AuthorizeAttribute>().Any()
                || actionAttributes.OfType<AuthorizeAttribute>().Any();

            var hasAllowAnonymous = actionAttributes.OfType<AllowAnonymousAttribute>().Any();

            if (!hasAuthorize || hasAllowAnonymous)
            {
                return;
            }

            operation.Security =
            [
                new OpenApiSecurityRequirement
                {
                    [
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        }
                    ] = Array.Empty<string>(),
                },
            ];
        }
    }

    private sealed class SnakeCaseSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties is null || schema.Properties.Count == 0)
            {
                return;
            }

            var renamed = new Dictionary<string, OpenApiSchema>();
            foreach (var (key, value) in schema.Properties)
            {
                renamed[JsonNamingPolicy.SnakeCaseLower.ConvertName(key)] = value;
            }

            schema.Properties = renamed;

            if (schema.Required is { Count: > 0 })
            {
                schema.Required = new HashSet<string>(
                    schema.Required.Select(JsonNamingPolicy.SnakeCaseLower.ConvertName));
            }
        }
    }
}
