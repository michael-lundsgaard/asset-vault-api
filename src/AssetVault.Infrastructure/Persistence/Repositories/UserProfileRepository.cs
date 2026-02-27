using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetVault.Infrastructure.Persistence.Repositories
{
    public class UserProfileRepository(AppDbContext context) : IUserProfileRepository
    {
        /// <inheritdoc/>
        public async Task<UserProfile?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await context.UserProfiles.FindAsync([id], cancellationToken);
        }

        /// <inheritdoc/>
        public async Task AddAsync(
            UserProfile profile,
            CancellationToken cancellationToken = default)
        {

            await context.UserProfiles.AddAsync(profile, cancellationToken);
        }

        /// <inheritdoc/>
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            context.SaveChangesAsync(cancellationToken);
    }
}
