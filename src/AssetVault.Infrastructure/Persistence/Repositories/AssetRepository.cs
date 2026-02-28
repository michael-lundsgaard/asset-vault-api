using AssetVault.Application.Assets.Queries;
using AssetVault.Application.Common;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using AssetVault.Infrastructure.Persistence.Extensions;
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
        public async Task<PagedResult<MediaAsset>> GetPagedAsync(
            AssetQuery query,
            CancellationToken cancellationToken = default)
        {
            var queryable = ApplyExpands(context.Assets.AsNoTracking(), query.Expand);
            queryable = queryable.ApplyFilters(query);

            var total = await queryable.CountAsync(cancellationToken);

            var items = await queryable
                .ApplySorting(query)
                .ApplyPaging(query)
                .ToListAsync(cancellationToken);

            return new PagedResult<MediaAsset>(items, total, query.Page, query.PageSize);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<MediaAsset>> GetPagedByUserAsync(
            Guid userId,
            AssetQuery query,
            CancellationToken cancellationToken = default)
        {
            var queryable = ApplyExpands(context.Assets.AsNoTracking().Where(a => a.UserId == userId), query.Expand);
            queryable = queryable.ApplyFilters(query);

            var total = await queryable.CountAsync(cancellationToken);

            var items = await queryable
                .ApplySorting(query)
                .ApplyPaging(query)
                .ToListAsync(cancellationToken);

            return new PagedResult<MediaAsset>(items, total, query.Page, query.PageSize);
        }

        /// <inheritdoc/>
        public async Task AddAsync(MediaAsset asset, CancellationToken cancellationToken = default) =>
            await context.Assets.AddAsync(asset, cancellationToken);

        /// <inheritdoc/>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            context.SaveChangesAsync(cancellationToken);

        private static IQueryable<MediaAsset> ApplyExpands(IQueryable<MediaAsset> queryable, AssetExpand expand)
        {
            if (expand.HasFlag(AssetExpand.Collections))
                queryable = queryable.Include(a => a.Collections);

            return queryable;
        }
    }
}
