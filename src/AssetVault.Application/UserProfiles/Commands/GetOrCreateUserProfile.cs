using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace AssetVault.Application.UserProfiles.Commands
{
    public record GetOrCreateUserProfileCommand(Guid UserId, string Email)
        : IRequest<UserProfile>
    {
        /// <summary>
        /// Returns the cache key for a given user ID.
        /// Use this in UpdateUserProfileCommandHandler to invalidate the cache on profile changes.
        /// </summary>
        public static string CacheKey(Guid userId) => $"user-profile:{userId}";
    }

    public class GetOrCreateUserProfileCommandHandler(
        IUserProfileRepository userProfileRepository,
        IMemoryCache cache
    ) : IRequestHandler<GetOrCreateUserProfileCommand, UserProfile>
    {
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

        public async Task<UserProfile> Handle(
            GetOrCreateUserProfileCommand request,
            CancellationToken cancellationToken)
        {
            string cacheKey = GetOrCreateUserProfileCommand.CacheKey(request.UserId);

            if (cache.TryGetValue(cacheKey, out UserProfile? cached) && cached is not null)
                return cached;

            var profile = await userProfileRepository.GetByIdAsync(
                request.UserId, cancellationToken);

            if (profile is null)
            {
                profile = UserProfile.Create(request.UserId, request.Email, request.Email.Split('@')[0]);
                await userProfileRepository.AddAsync(profile, cancellationToken);
                await userProfileRepository.SaveChangesAsync(cancellationToken);
            }

            cache.Set(cacheKey, profile, CacheDuration);

            return profile;
        }
    }
}
