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

            builder.OwnsOne(a => a.Size, size =>
            {
                size.Property(s => s.Bytes).HasColumnName("SizeBytes").IsRequired();
            });

            builder.OwnsOne(a => a.StoragePath, path =>
            {
                // Nullable during upload process, but should be required once upload is confirmed
                path.Property(p => p.Value).HasColumnName("StoragePath").HasMaxLength(1024);
            });

            builder.Property(a => a.Tags)
                .HasColumnType("text[]")
                .IsRequired();

            builder.HasOne(a => a.Owner)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(a => a.Collections)
                .WithMany(c => c.Assets)
                .UsingEntity("AssetCollections");

            builder.ToTable("Assets");
        }
    }
}
