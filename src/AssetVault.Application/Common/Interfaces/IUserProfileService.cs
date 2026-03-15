using AssetVault.Domain.Entities;

namespace AssetVault.Application.Common.Interfaces
{
    public interface IUserProfileService
    {
        /// <summary>
        /// Retrieves the profile associated with the specified user.
        /// If no profile exists, one is created.
        /// </summary>
        Task<UserProfile> GetOrCreateAsync(Guid userId, string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates the cache key used for a user profile.
        /// </summary>
        static string CacheKey(Guid userId) => $"user-profile:{userId}";
    }
}
