using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetVault.Infrastructure.Persistence.Repositories
{
    public class UserProfileRepository(AppDbContext context) : IUserProfileRepository
    {
        /// <inheritdoc/>
        public async Task<UserProfile?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {

            return await context.UserProfiles
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
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
