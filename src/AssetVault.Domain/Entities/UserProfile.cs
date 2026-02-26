using AssetVault.Domain.Common;

namespace AssetVault.Domain.Entities
{
    public class UserProfile : BaseEntity
    {
        public Guid UserId { get; private set; } = default!; // mirrors Supabase auth.users UUID
        public string Email { get; private set; } = default!;
        public string DisplayName { get; private set; } = default!;

        // Required for EF Core and to enforce use of the static Create method;
        private UserProfile() { }

        public static UserProfile Create(Guid userId, string email, string displayName)
        {
            return new UserProfile
            {
                UserId = userId,
                Email = email,
                DisplayName = displayName
            };
        }
    }
}