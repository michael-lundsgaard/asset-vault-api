namespace AssetVault.Application.Common.Interfaces
{
    public interface IStorageService
    {
        /// <summary>
        /// Generates a pre-signed URL so the client can upload directly to S3.
        /// </summary>
        Task<PresignedUploadResult> GenerateUploadUrlAsync(Guid assetId, string fileName, string contentType, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a pre-signed URL for temporary read access.
        /// </summary>
        Task<string> GenerateDownloadUrlAsync(string storagePath, TimeSpan expiry, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a file from storage.
        /// </summary>
        Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
    }

    public record PresignedUploadResult(string UploadUrl, string StoragePath, DateTime ExpiresAt);

}