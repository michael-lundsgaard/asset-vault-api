using System.Security.Claims;
using AssetVault.Application.Common.Interfaces;

namespace AssetVault.API.Middleware
{
    public class UserProfileMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = context.User.FindFirstValue(ClaimTypes.Email);

                if (Guid.TryParse(sub, out var userId) && email is not null)
                {
                    // Resolve per-request to avoid capturing a scoped service inside the singleton middleware.
                    var userProfileService = context.RequestServices.GetRequiredService<IUserProfileService>();
                    var profile = await userProfileService.GetOrCreateAsync(userId, email, context.RequestAborted);

                    context.Items["UserProfile"] = profile;
                }
            }

            await next(context);
        }
    }
}
