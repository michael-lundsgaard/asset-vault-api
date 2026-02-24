using AssetVault.Domain.Entities;
using AssetVault.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVault.Infrastructure.Persistence.Configurations
{
    public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
    {
        public void Configure(EntityTypeBuilder<MediaAsset> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.FileName).HasMaxLength(512).IsRequired();
            builder.Property(a => a.ContentType).HasMaxLength(128).IsRequired();
            builder.Property(a => a.Status).IsRequired();

            // Value object: FileSize -> owned, stored as column
            builder.OwnsOne(a => a.Size, size =>
            {
                size.Property(s => s.Bytes).HasColumnName("SizeBytes").IsRequired();
            });

            // Value object: StoragePath -> owned, stored as column
            builder.OwnsOne(a => a.StoragePath, path =>
            {
                path.Property(p => p.Value).HasColumnName("StoragePath").HasMaxLength(1024).IsRequired();
            });

            builder.HasOne(a => a.Collection)
                .WithMany(c => c.Assets)
                .HasForeignKey(a => a.CollectionId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(a => a.Tags)
                .WithMany(t => t.Assets)
                .UsingEntity("AssetTags");

            builder.ToTable("Assets");
        }
    }
}