using AssetVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVault.Infrastructure.Persistence.Configurations
{
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
            builder.Property(u => u.DisplayName).HasMaxLength(256).IsRequired();

            builder.ToTable("UserProfiles");
        }
    }
}
