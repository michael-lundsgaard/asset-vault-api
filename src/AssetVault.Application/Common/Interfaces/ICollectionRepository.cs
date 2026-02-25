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
        /// Gets all collections, optionally including related entities. Returns an empty list if none exist.
        /// </summary>
        Task<IReadOnlyList<Collection>> GetAllAsync(CollectionExpand expand = CollectionExpand.None, CancellationToken cancellationToken = default);

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

    public record CollectionWithCount(Collection Collection, int AssetCount);

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
