using AssetVault.Application.Common.Interfaces;
using AssetVault.Infrastructure.Persistence;
using AssetVault.Infrastructure.Persistence.Repositories;
using AssetVault.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AssetVault.Infrastructure.Extensions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database (Supabase = hosted PostgreSQL)
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Repositories
            services.AddScoped<IAssetRepository, AssetRepository>();
            services.AddScoped<ICollectionRepository, CollectionRepository>();

            // Cloudflare R2 / MinIO (development) / S3 Storage
            services.Configure<S3StorageOptions>(configuration.GetSection("Storage:S3"));
            services.AddScoped<IStorageService, S3StorageService>();

            return services;
        }
    }
}