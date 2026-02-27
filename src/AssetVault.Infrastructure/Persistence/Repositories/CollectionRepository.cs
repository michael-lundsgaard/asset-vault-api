using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
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
        public async Task<IReadOnlyList<Collection>> GetAllAsync(
            CollectionExpand expand = CollectionExpand.None,
            CancellationToken cancellationToken = default)
        {
            var query = context.Collections.AsQueryable();

            if (expand.HasFlag(CollectionExpand.Assets))
                query = query.Include(c => c.Assets);

            return await query.ToListAsync(cancellationToken);
        }

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
    }
}
