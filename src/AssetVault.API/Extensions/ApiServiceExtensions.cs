using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

namespace AssetVault.API.Extensions
{
    public static class ApiServiceExtensions
    {
        /// <summary>
        /// Configures JWT Bearer authentication for the API, using settings from the configuration.
        /// </summary>
        public static IServiceCollection AddApiAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["Authentication:Authority"];
                    options.Audience = configuration["Authentication:Audience"];
                });

            return services;
        }

        /// <summary>
        /// Configures OpenAPI documentation for the API, including a security scheme for JWT Bearer authentication.
        /// </summary>
        public static IServiceCollection AddApiDocs(this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                options.AddOperationTransformer((operation, context, _) =>
                {
                    if (context.Description.ActionDescriptor is ControllerActionDescriptor descriptor)
                    {
                        var explicitName = descriptor.EndpointMetadata
                            .OfType<IEndpointNameMetadata>()
                            .FirstOrDefault()?.EndpointName;

                        operation.OperationId = explicitName ?? $"{descriptor.ControllerName}_{descriptor.ActionName}";
                    }
                    return Task.CompletedTask;
                });

                options.AddOperationTransformer((operation, _, _) =>
                {
                    if (operation.Parameters is not null)
                    {
                        foreach (var parameter in operation.Parameters)
                        {
                            if (parameter.In == ParameterLocation.Query && parameter.Name.Length > 0)
                                parameter.Name = char.ToLowerInvariant(parameter.Name[0]) + parameter.Name[1..];
                        }
                    }

                    return Task.CompletedTask;
                });

                options.AddDocumentTransformer((document, _, _) =>
                {
                    var securityScheme = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        In = ParameterLocation.Header,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    };

                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes.Add(JwtBearerDefaults.AuthenticationScheme, securityScheme);

                    document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                    {
                        [
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = JwtBearerDefaults.AuthenticationScheme,
                                    Type = ReferenceType.SecurityScheme
                                }
                            }
                        ] = []
                    });

                    return Task.CompletedTask;
                });
            });

            return services;
        }

        /// <summary>
        /// Enables the OpenAPI documentation UI for the API, allowing developers to explore and test the API endpoints.
        /// </summary>
        public static void UseApiDocs(this WebApplication app)
        {
            app.MapOpenApi();
            app.MapScalarApiReference(options => options
                .AddPreferredSecuritySchemes(JwtBearerDefaults.AuthenticationScheme)
                .EnablePersistentAuthentication()
            );
        }
    }
}
