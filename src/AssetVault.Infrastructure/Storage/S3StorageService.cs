using Amazon.S3;
using Amazon.S3.Model;
using AssetVault.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace AssetVault.Infrastructure.Storage
{
    public class S3StorageOptions
    {
        // Private bucket — main asset uploads and presigned downloads
        public string BucketName { get; set; } = default!;
        public string AccountId { get; set; } = default!;
        public string AccessKeyId { get; set; } = default!;
        public string SecretAccessKey { get; set; } = default!;
        public string ServiceUrl { get; set; } = default!;
        public bool UseHttp { get; set; } = false;
        public int UploadPresignedUrlExpiryMinutes { get; set; } = 15;
        public int DownloadPresignedUrlExpiryMinutes { get; set; } = 60;

        // Public bucket — thumbnails and cover images, served via a stable public URL.
        // R2 "Public Development URL" (or a custom domain) must be enabled on this bucket.
        public string PublicBucketName { get; set; } = default!;
        public string PublicServiceUrl { get; set; } = default!;
        public string PublicBaseUrl { get; set; } = default!;
    }

    /// <summary>
    /// Cloudflare R2 is S3-compatible — the AWS SDK works as-is.
    /// Two S3 clients are maintained: one for the private asset bucket and one for the
    /// public thumbnail/cover bucket, which uses a separate endpoint and R2 Public Dev URL.
    /// </summary>
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _privateS3;
        private readonly IAmazonS3 _publicS3;
        private readonly S3StorageOptions _options;

        public S3StorageService(IOptions<S3StorageOptions> options)
        {
            _options = options.Value;

            _privateS3 = new AmazonS3Client(
                _options.AccessKeyId,
                _options.SecretAccessKey,
                new AmazonS3Config
                {
                    ServiceURL = _options.ServiceUrl,
                    ForcePathStyle = true, // Required for R2
                    UseHttp = _options.UseHttp,
                    SignatureVersion = "4",
                    AuthenticationRegion = "auto" // R2 doesn't require a region, but AWS SDK needs this set to avoid signing issues
                });

            _publicS3 = new AmazonS3Client(
                _options.AccessKeyId,
                _options.SecretAccessKey,
                new AmazonS3Config
                {
                    ServiceURL = _options.PublicServiceUrl,
                    ForcePathStyle = true,
                    UseHttp = _options.UseHttp,
                    SignatureVersion = "4",
                    AuthenticationRegion = "auto"
                });
        }

        public async Task<PresignedUploadResult> GenerateUploadUrlAsync(
            Guid assetId,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            var key = $"uploads/{assetId}/{fileName}";
            return await GeneratePresignedUploadAsync(_privateS3, _options.BucketName, key, contentType);
        }

        public async Task<PresignedDownloadResult> GenerateDownloadUrlAsync(
            string storagePath,
            CancellationToken cancellationToken = default)
        {
            var expiry = DateTime.UtcNow.AddMinutes(_options.DownloadPresignedUrlExpiryMinutes);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = storagePath,
                Verb = HttpVerb.GET,
                Expires = expiry
            };

            var url = await _privateS3.GetPreSignedURLAsync(request);

            // AWS SDK ignores UseHttp for presigned URLs — fix it manually
            if (_options.UseHttp)
                url = url.Replace("https://", "http://");

            return new PresignedDownloadResult(url, expiry);
        }

        public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
        {
            await _privateS3.DeleteObjectAsync(_options.BucketName, storagePath, cancellationToken);
        }

        public async Task DeletePublicAsync(string storagePath, CancellationToken cancellationToken = default)
        {
            await _publicS3.DeleteObjectAsync(_options.PublicBucketName, storagePath, cancellationToken);
        }

        public async Task<PresignedUploadResult> GenerateThumbnailUploadUrlAsync(
            Guid assetId,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            var key = $"thumbnails/{assetId}/thumbnail";
            return await GeneratePresignedUploadAsync(_publicS3, _options.PublicBucketName, key, contentType);
        }

        public async Task<PresignedUploadResult> GenerateCoverImageUploadUrlAsync(
            Guid collectionId,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            var key = $"covers/{collectionId}/cover";
            return await GeneratePresignedUploadAsync(_publicS3, _options.PublicBucketName, key, contentType);
        }

        public string GetPublicUrl(string storagePath) =>
            $"{_options.PublicBaseUrl.TrimEnd('/')}/{storagePath}";

        private async Task<PresignedUploadResult> GeneratePresignedUploadAsync(
            IAmazonS3 client,
            string bucketName,
            string key,
            string contentType)
        {
            var expiry = DateTime.UtcNow.AddMinutes(_options.UploadPresignedUrlExpiryMinutes);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                ContentType = contentType,
                Expires = expiry
            };

            var url = await client.GetPreSignedURLAsync(request);

            // AWS SDK ignores UseHttp for presigned URLs — fix it manually
            if (_options.UseHttp)
                url = url.Replace("https://", "http://");

            return new PresignedUploadResult(url, key, expiry);
        }
    }
}
