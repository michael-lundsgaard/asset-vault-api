using AssetVault.Application.Common.Interfaces;

namespace AssetVault.IntegrationTests.Infrastructure;

/// <summary>
/// In-process substitute for <see cref="IStorageService"/> that returns deterministic URLs
/// without touching MinIO or Cloudflare R2. Registered by <see cref="AssetVaultWebAppFactory"/>.
/// </summary>
public class FakeStorageService : IStorageService
{
    public Task<PresignedUploadResult> GenerateUploadUrlAsync(
        Guid assetId, string fileName, string contentType, CancellationToken cancellationToken = default) =>
        Task.FromResult(new PresignedUploadResult(
            $"https://fake-storage.test/upload/{assetId}/{fileName}",
            $"uploads/{assetId}/{fileName}",
            DateTime.UtcNow.AddMinutes(15)));

    public Task<PresignedDownloadResult> GenerateDownloadUrlAsync(
        string storagePath, CancellationToken cancellationToken = default) =>
        Task.FromResult(new PresignedDownloadResult(
            $"https://fake-storage.test/download/{storagePath}",
            DateTime.UtcNow.AddHours(1)));

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
