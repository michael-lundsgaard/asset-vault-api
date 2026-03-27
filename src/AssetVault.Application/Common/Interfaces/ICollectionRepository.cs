using AssetVault.Application.Collections.Queries;
using AssetVault.Application.Common;
using AssetVault.Domain.Entities;

namespace AssetVault.Application.Common.Interfaces
{
    public interface ICollectionRepository
    {
        /// <summary>
        /// Gets a collection by ID, optionally including assets. Returns null if not found.
        /// </summary>
        Task<Collection?> GetByIdAsync(Guid id, CollectionExpand expand = CollectionExpand.None, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a filtered, sorted, and paged list of shared collections.
        /// </summary>
        Task<PagedResult<Collection>> GetPagedSharedAsync(CollectionQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a filtered, sorted, and paged list of the user's private and favorites collections.
        /// </summary>
        Task<PagedResult<Collection>> GetPagedByUserAsync(Guid userId, CollectionQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the user's Favorites collection, or null if it hasn't been created yet.
        /// </summary>
        Task<Collection?> GetFavoritesAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new collection to the repository. The collection's ID should be generated before calling this method.
        /// </summary>
        Task AddAsync(Collection collection, CancellationToken cancellationToken = default);

        /// <summary>'
        /// Updates an existing collection in the repository. The collection must already exist and have a valid ID.
        /// </summary>
        Task UpdateAsync(Collection collection, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a collection from the repository. The collection must already exist and have a valid ID.
        /// </summary>
        Task DeleteAsync(Collection collection, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists any changes made to the collections in the repository.
        /// This should be called after adding, updating, or deleting collections to save those changes to the database.
        /// </summary>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Flags enum used to specify which related entities to include when retrieving a collection.
    /// </summary>
    [Flags]
    public enum CollectionExpand
    {
        None = 0,
        Assets = 1
    }
}
