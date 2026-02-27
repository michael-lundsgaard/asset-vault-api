using AssetVault.Domain.Common;

namespace AssetVault.Domain.Entities
{
    public class UserProfile : BaseEntity
    {
        public string Email { get; private set; } = default!;
        public string DisplayName { get; private set; } = default!;

        private UserProfile() { }

        public static UserProfile Create(Guid userId, string email, string displayName) =>
            new() { Id = userId, Email = email, DisplayName = displayName };
    }
}