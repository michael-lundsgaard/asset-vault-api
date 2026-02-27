using AssetVault.Application.Common.Behaviour;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AssetVault.Application.Extensions
{
    public static class ApplicationServiceExtensions
    {
        /// <summary>
        /// Registers MediatR services, including pipeline behaviors for logging and validation, and FluentValidation
        /// validators from the application assembly. This sets up the core application services for handling requests and enforcing validation rules.
        /// </summary>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly);
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            });
            services.AddValidatorsFromAssembly(typeof(AssemblyMarker).Assembly);

            return services;
        }
    }
}
