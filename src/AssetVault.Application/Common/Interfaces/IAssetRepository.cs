using AssetVault.Application.Assets.Queries;
using AssetVault.Application.Common;
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
        /// Gets a filtered, sorted, and paged list of all assets.
        /// </summary>
        Task<PagedResult<MediaAsset>> GetPagedAsync(AssetQuery query, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a filtered, sorted, and paged list of assets owned by a specific user.
        /// </summary>
        Task<PagedResult<MediaAsset>> GetPagedByUserAsync(Guid userId, AssetQuery query, CancellationToken cancellationToken = default);

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
