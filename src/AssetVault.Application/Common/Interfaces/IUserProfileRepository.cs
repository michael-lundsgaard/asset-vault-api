using AssetVault.Domain.Entities;

namespace AssetVault.Application.Common.Interfaces
{
    public interface IUserProfileRepository
    {
        /// <summary>
        /// Gets a user profile by Supabase user ID. Returns null if not found.
        /// </summary>
        Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a new user profile to the repository.
        /// </summary>
        Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists any pending changes to the database.
        /// </summary>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
