using AssetVault.Domain.Entities;

namespace AssetVault.Application.Common.Interfaces
{
    public interface IAssetRepository
    {
        /// <summary>
        /// Gets an asset by ID, optionally including related entities. Returns null if not found.
        /// </summary>
        Task<MediaAsset?> GetByIdAsync(Guid id, AssetExpand expand = AssetExpand.None, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all assets owned by a specific user, optionally including related entities. Returns an empty list if none exist.
        /// </summary>
        Task<IReadOnlyList<MediaAsset>> GetByUserAsync(Guid userId, AssetExpand expand = AssetExpand.None, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all assets, optionally including related entities. Returns an empty list if none exist.
        /// </summary>
        Task<IReadOnlyList<MediaAsset>> GetAllAsync(AssetExpand expand = AssetExpand.None, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new asset to the repository. The asset's ID should be generated before calling this method.
        /// </summary>
        Task AddAsync(MediaAsset asset, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists any changes made to the assets in the repository.
        /// This should be called after adding, updating, or deleting assets to save those changes to the database.
        /// </summary>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Flags enum used to specify which related entities to include when retrieving an asset.
    /// </summary>
    [Flags]
    public enum AssetExpand
    {
        None = 0,
        Collections = 1,
        All = Collections
    }
}