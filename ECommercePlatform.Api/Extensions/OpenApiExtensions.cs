using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ECommercePlatform.Api.Extensions;

public static class OpenApiExtensions
{
    private const string BearerSchemeName = "Bearer";

    public static IServiceCollection AddApplicationOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
            options.AddOperationTransformer<BearerSecurityRequirementTransformer>();
        });

        return services;
    }

    /// <summary>Declares the bearer scheme once, in components.</summary>
    private sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            document.Info ??= new OpenApiInfo();
            document.Info.Title = "ECommercePlatform API";
            document.Info.Version = "v1";
            document.Info.Description =
                "Authenticate via POST /api/v1/auth/login, then send the access token as "
                + "`Authorization: Bearer <token>`. Exchange the refresh token at "
                + "POST /api/v1/auth/refresh before the access token expires.";

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

            document.Components.SecuritySchemes[BearerSchemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT access token issued by the auth endpoints."
            };

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Attaches the bearer requirement only to operations that actually require
    /// authorization, so the docs mirror the real policy.
    /// </summary>
    private sealed class BearerSecurityRequirementTransformer : IOpenApiOperationTransformer
    {
        public Task TransformAsync(
            OpenApiOperation operation,
            OpenApiOperationTransformerContext context,
            CancellationToken cancellationToken)
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata;

            var requiresAuth = metadata.OfType<IAuthorizeData>().Any()
                && !metadata.OfType<IAllowAnonymous>().Any();

            if (!requiresAuth)
            {
                return Task.CompletedTask;
            }

            operation.Security ??= [];

            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerSchemeName, context.Document)] = []
            });

            return Task.CompletedTask;
        }
    }
}
