using AssetVault.Domain.Entities;
using AssetVault.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVault.Infrastructure.Persistence.Configurations
{
    public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
    {
        public void Configure(EntityTypeBuilder<Collection> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name).HasMaxLength(256).IsRequired();
            builder.Property(c => c.Description).HasMaxLength(1024);
            builder.Property(c => c.Type)
                .HasConversion<int>()
                .HasDefaultValue(CollectionType.Shared)
                .IsRequired();

            builder.HasOne(a => a.Owner)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Assets)
                .WithMany(a => a.Collections)
                .UsingEntity("AssetCollections");

            builder.ToTable("Collections");
        }
    }
}
