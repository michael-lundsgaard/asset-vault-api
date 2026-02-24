using AssetVault.Domain.Entities;

namespace AssetVault.Application.Common.Interfaces
{
    public interface ICollectionRepository
    {
        /// <summary>
        /// Gets a collection by ID, including its assets and tags. Returns null if not found.
        /// </summary>
        Task<Collection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all collections, including their assets and tags. Returns an empty list if none exist.
        /// </summary>
        Task<IReadOnlyList<Collection>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new collection to the repository. The collection's ID should be generated before calling this method.
        /// </summary>
        Task AddAsync(Collection collection, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists any changes made to the collections in the repository. 
        /// This should be called after adding, updating, or deleting collections to save those changes to the database.
        /// </summary>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}