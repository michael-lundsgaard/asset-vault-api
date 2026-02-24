using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetVault.Infrastructure.Persistence.Repositories
{
    public class CollectionRepository(AppDbContext context) : ICollectionRepository
    {
        /// <inheritdoc/>
        public async Task<Collection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            await context.Collections.FindAsync([id], cancellationToken);

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Collection>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await context.Collections.ToListAsync(cancellationToken);

        /// <inheritdoc/>
        public async Task AddAsync(Collection collection, CancellationToken cancellationToken = default) =>
            await context.Collections.AddAsync(collection, cancellationToken);

        /// <inheritdoc/>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            context.SaveChangesAsync(cancellationToken);
    }
}