using Amazon.S3;
using Amazon.S3.Model;
using AssetVault.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace AssetVault.Infrastructure.Storage
{
    public class S3StorageOptions
    {
        public string BucketName { get; set; } = default!;
        public string AccountId { get; set; } = default!; // Cloudflare Account ID
        public string AccessKeyId { get; set; } = default!;
        public string SecretAccessKey { get; set; } = default!;
        public string ServiceUrl { get; set; } = default!;
        public bool UseHttp { get; set; } = false;
        public int UploadPresignedUrlExpiryMinutes { get; set; } = 15;
        public int DownloadPresignedUrlExpiryMinutes { get; set; } = 60;
    }

    /// <summary>
    /// Cloudflare R2 is S3-compatible — the AWS SDK works as-is.
    /// </summary>
    public class S3StorageService : IStorageService
    {
        private readonly IAmazonS3 _s3;
        private readonly S3StorageOptions _options;

        public S3StorageService(IOptions<S3StorageOptions> options)
        {
            _options = options.Value;

            _s3 = new AmazonS3Client(
                _options.AccessKeyId,
                _options.SecretAccessKey,
                new AmazonS3Config
                {
                    ServiceURL = _options.ServiceUrl,
                    ForcePathStyle = true, // Required for R2
                    UseHttp = _options.UseHttp
                });
        }

        public async Task<PresignedUploadResult> GenerateUploadUrlAsync(
            Guid assetId,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            var key = $"uploads/{assetId}/{fileName}";
            var expiry = DateTime.UtcNow.AddMinutes(_options.UploadPresignedUrlExpiryMinutes);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = _options.BucketName,
                Key = key,
                Verb = HttpVerb.PUT,
                ContentType = contentType,
                Expires = expiry
            };

            var url = await _s3.GetPreSignedURLAsync(request);

            // AWS SDK ignores UseHttp for presigned URLs — fix it manually
            if (_options.UseHttp)
                url = url.Replace("https://", "http://");

            return new PresignedUploadResult(url, key, expiry);
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

            var url = await _s3.GetPreSignedURLAsync(request);

            // AWS SDK ignores UseHttp for presigned URLs — fix it manually
            if (_options.UseHttp)
                url = url.Replace("https://", "http://");

            return new PresignedDownloadResult(url, expiry);
        }

        public async Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
        {
            await _s3.DeleteObjectAsync(_options.BucketName, storagePath, cancellationToken);
        }
    }
}
