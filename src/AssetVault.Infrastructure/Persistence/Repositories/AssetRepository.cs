using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetVault.Infrastructure.Persistence.Repositories
{
    public class AssetRepository(AppDbContext context) : IAssetRepository
    {
        /// <inheritdoc/>
        public async Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await context.Assets.FindAsync([id], cancellationToken);

        /// <inheritdoc/>
        public async Task<MediaAsset?> GetByIdWithExpandAsync(
            Guid id,
            AssetExpand expand,
            CancellationToken cancellationToken = default)
        {
            var query = context.Assets.AsQueryable();

            if (expand.HasFlag(AssetExpand.Collection))
                query = query.Include(a => a.Collection);

            if (expand.HasFlag(AssetExpand.Tags))
                query = query.Include(a => a.Tags);

            return await query.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<MediaAsset>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await context.Assets.OrderBy(a => a.CreatedAt).ToListAsync(cancellationToken);

        /// <inheritdoc/>
        public async Task AddAsync(MediaAsset asset, CancellationToken cancellationToken = default) =>
            await context.Assets.AddAsync(asset, cancellationToken);

        /// <inheritdoc/>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            context.SaveChangesAsync(cancellationToken);

        /// <inheritdoc/>
        public async Task<IReadOnlyList<MediaAsset>> GetAllAsync(AssetExpand expand, CancellationToken cancellationToken = default)
        {
            var query = context.Assets.AsQueryable();

            if (expand.HasFlag(AssetExpand.Collection))
                query = query.Include(a => a.Collection);

            if (expand.HasFlag(AssetExpand.Tags))
                query = query.Include(a => a.Tags);

            return await query.OrderBy(a => a.CreatedAt).ToListAsync(cancellationToken);
        }
    }
}