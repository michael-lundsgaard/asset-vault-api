using AssetVault.Application.Collections.Queries;
using AssetVault.Application.Common;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using AssetVault.Domain.Enums;
using AssetVault.Infrastructure.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AssetVault.Infrastructure.Persistence.Repositories
{
    public class CollectionRepository(AppDbContext context) : ICollectionRepository
    {
        /// <inheritdoc/>
        public async Task<Collection?> GetByIdAsync(
            Guid id,
            CollectionExpand expand = CollectionExpand.None,
            CancellationToken cancellationToken = default)
        {
            if (expand == CollectionExpand.None)
                return await context.Collections.FindAsync([id], cancellationToken);

            var query = context.Collections.AsQueryable();

            if (expand.HasFlag(CollectionExpand.Assets))
                query = query.Include(c => c.Assets);

            return await query.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<Collection>> GetPagedSharedAsync(
            CollectionQuery query,
            CancellationToken cancellationToken = default)
        {
            var queryable = ApplyExpands(context.Collections.AsNoTracking().Where(c => c.Type == CollectionType.Shared), query.Expand);
            queryable = queryable.ApplyFilters(query);

            var total = await queryable.CountAsync(cancellationToken);

            var items = await queryable
                .ApplySorting(query)
                .ApplyPaging(query)
                .ToListAsync(cancellationToken);

            return new PagedResult<Collection>(items, total, query.Page, query.PageSize);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<Collection>> GetPagedByUserAsync(
            Guid userId,
            CollectionQuery query,
            CancellationToken cancellationToken = default)
        {
            var queryable = ApplyExpands(
                context.Collections.AsNoTracking()
                    .Where(c => c.UserId == userId && (c.Type == CollectionType.Private || c.Type == CollectionType.Favorites)),
                query.Expand);
            queryable = queryable.ApplyFilters(query);

            var total = await queryable.CountAsync(cancellationToken);

            var items = await queryable
                .ApplySorting(query)
                .ApplyPaging(query)
                .ToListAsync(cancellationToken);

            return new PagedResult<Collection>(items, total, query.Page, query.PageSize);
        }

        /// <inheritdoc/>
        public async Task<Collection?> GetFavoritesAsync(Guid userId, CancellationToken cancellationToken = default) =>
            await context.Collections
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Type == CollectionType.Favorites, cancellationToken);

        /// <inheritdoc/>
        public async Task AddAsync(Collection collection, CancellationToken cancellationToken = default) =>
            await context.Collections.AddAsync(collection, cancellationToken);

        /// <inheritdoc/>
        public Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default)
        {
            context.Collections.Update(collection);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task DeleteAsync(Collection collection, CancellationToken cancellationToken = default)
        {
            context.Collections.Remove(collection);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            context.SaveChangesAsync(cancellationToken);

        private static IQueryable<Collection> ApplyExpands(IQueryable<Collection> queryable, CollectionExpand expand)
        {
            if (expand.HasFlag(CollectionExpand.Assets))
                queryable = queryable.Include(a => a.Assets);

            return queryable;
        }
    }
}
