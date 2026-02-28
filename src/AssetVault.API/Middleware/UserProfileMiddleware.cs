using System.Security.Claims;
using AssetVault.Application.UserProfiles.Commands;
using MediatR;

namespace AssetVault.API.Middleware
{
    public class UserProfileMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                // Supabase JWTs carry `sub` (UUID) and `email` as standard claims.
                var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = context.User.FindFirstValue(ClaimTypes.Email);

                if (Guid.TryParse(sub, out var userId) && email is not null)
                {
                    // ISender is scoped — resolve per-request to avoid capturing a scoped
                    // service inside the singleton middleware.
                    var sender = context.RequestServices.GetRequiredService<ISender>();
                    var profile = await sender.Send(
                        new GetOrCreateUserProfileCommand(userId, email),
                        context.RequestAborted);

                    // Store the profile in HttpContext.Items for downstream access in controllers, etc.
                    context.Items["UserProfile"] = profile;
                }
            }

            await next(context);
        }
    }
}
