using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetVault.Infrastructure.Persistence.Repositories
{
    public class AssetRepository(AppDbContext context) : IAssetRepository
    {
        /// <inheritdoc/>
        public async Task<MediaAsset?> GetByIdAsync(
            Guid id,
            AssetExpand expand = AssetExpand.None,
            CancellationToken cancellationToken = default)
        {
            if (expand == AssetExpand.None)
                return await context.Assets.FindAsync([id], cancellationToken);

            var query = context.Assets.AsQueryable();

            if (expand.HasFlag(AssetExpand.Collections))
                query = query.Include(a => a.Collections);

            return await query.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<MediaAsset>> GetAllAsync(
            AssetExpand expand = AssetExpand.None,
            CancellationToken cancellationToken = default)
        {
            var query = context.Assets.AsQueryable();

            if (expand.HasFlag(AssetExpand.Collections))
                query = query.Include(a => a.Collections);

            return await query.OrderBy(a => a.CreatedAt).ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task AddAsync(MediaAsset asset, CancellationToken cancellationToken = default) =>
            await context.Assets.AddAsync(asset, cancellationToken);

        /// <inheritdoc/>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            context.SaveChangesAsync(cancellationToken);

        /// <inheritdoc/>
        public async Task<IReadOnlyList<MediaAsset>> GetByUserAsync(
            Guid userId,
            AssetExpand expand = AssetExpand.None,
            CancellationToken cancellationToken = default)
        {
            if (expand == AssetExpand.None)
                return await context.Assets.Where(a => a.UserId == userId).ToListAsync(cancellationToken);

            var query = context.Assets.AsQueryable();

            if (expand.HasFlag(AssetExpand.Collections))
                query = query.Include(a => a.Collections);

            return await query.Where(a => a.UserId == userId).ToListAsync(cancellationToken);
        }
    }
}