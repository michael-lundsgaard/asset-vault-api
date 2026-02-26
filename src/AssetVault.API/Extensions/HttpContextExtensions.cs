using AssetVault.Domain.Entities;

namespace AssetVault.API.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the UserProfile from the HttpContext.Items dictionary.
        /// </summary>
        public static UserProfile? GetUserProfile(this HttpContext context)
            => context.Items["UserProfile"] as UserProfile;

        /// <summary>
        /// Gets the UserProfile from the HttpContext.Items dictionary, throwing an exception if not found.
        /// </summary>
        public static UserProfile GetRequiredUserProfile(this HttpContext context)
            => context.GetUserProfile() ?? throw new InvalidOperationException("UserProfile not found in context.");
    }
}