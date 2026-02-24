using AssetVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetVault.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<MediaAsset> Assets => Set<MediaAsset>();
        public DbSet<Collection> Collections => Set<Collection>();
        public DbSet<Tag> Tags => Set<Tag>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}