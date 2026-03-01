namespace AssetVault.Application.Common.Interfaces
{
    public interface IStorageService
    {
        /// <summary>
        /// Generates a pre-signed URL so the client can upload directly to S3.
        /// </summary>
        Task<PresignedUploadResult> GenerateUploadUrlAsync(Guid assetId, string fileName, string contentType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a pre-signed URL for temporary read access. Expiry is determined by the storage service configuration.
        /// </summary>
        Task<PresignedDownloadResult> GenerateDownloadUrlAsync(string storagePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file from storage.
        /// </summary>
        Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
    }

    public record PresignedUploadResult(string PresignedUrl, string StoragePath, DateTime ExpiresAt);
    public record PresignedDownloadResult(string PresignedUrl, DateTime ExpiresAt);

}
