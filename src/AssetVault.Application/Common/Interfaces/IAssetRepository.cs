using AssetVault.Domain.Entities;

namespace AssetVault.Application.Common.Interfaces
{
    public interface IAssetRepository
    {
        /// <summary>
        /// Gets an asset by ID. Returns null if not found.
        /// </summary>
        Task<MediaAsset?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an asset by ID, including related entities based on the specified expand flags. Returns null if not found.
        /// </summary>
        Task<MediaAsset?> GetByIdWithExpandAsync(Guid id, AssetExpand expand, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all assets. Returns an empty list if none exist.
        /// </summary>
        Task<IReadOnlyList<MediaAsset>> GetAllAsync(CancellationToken cancellationToken = default);

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
        Collection = 1,
        Tags = 2,
        All = Collection | Tags
    }
}