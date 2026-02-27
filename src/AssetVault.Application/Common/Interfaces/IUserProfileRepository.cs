using AssetVault.Domain.Entities;

namespace AssetVault.Application.Common.Interfaces
{
    public interface IUserProfileRepository
    {
        /// <summary>
        /// Gets a user profile by ID (which is the Supabase auth UUID). Returns null if not found.
        /// </summary>
        Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

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
