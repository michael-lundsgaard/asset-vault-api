using AssetVault.Application.Assets.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Assets;
using FluentValidation;
using MediatR;

namespace AssetVault.Application.Assets.Commands
{
    public record InitiateThumbnailUploadCommand(
        Guid UserId,
        Guid AssetId,
        string ContentType,
        long SizeBytes
    ) : IRequest<InitiateThumbnailUploadResult>;

    public record InitiateThumbnailUploadResult(Guid AssetId, string PresignedUrl, DateTime ExpiresAt);

    public class InitiateThumbnailUploadCommandValidator : AbstractValidator<InitiateThumbnailUploadCommand>
    {
        private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxSizeBytes = 5 * 1024 * 1024;

        public InitiateThumbnailUploadCommandValidator()
        {
            RuleFor(x => x.ContentType)
                .Must(ct => AllowedContentTypes.Contains(ct))
                .WithMessage("Thumbnail must be image/jpeg, image/png, or image/webp.");

            RuleFor(x => x.SizeBytes)
                .InclusiveBetween(1, MaxSizeBytes)
                .WithMessage("Thumbnail must be between 1 byte and 5 MB.");
        }
    }

    public class InitiateThumbnailUploadCommandHandler(
        IAssetRepository assetRepository,
        IStorageService storageService
    ) : IRequestHandler<InitiateThumbnailUploadCommand, InitiateThumbnailUploadResult>
    {
        public async Task<InitiateThumbnailUploadResult> Handle(
            InitiateThumbnailUploadCommand request,
            CancellationToken cancellationToken)
        {
            var asset = await assetRepository.GetByIdAsync(request.AssetId, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Asset {request.AssetId} not found.");

            if (asset.UserId != request.UserId)
                throw new UnauthorizedAccessException("You do not have permission to modify this asset.");

            var presigned = await storageService.GenerateThumbnailUploadUrlAsync(
                request.AssetId,
                request.ContentType,
                cancellationToken);

            return new InitiateThumbnailUploadResult(request.AssetId, presigned.PresignedUrl, presigned.ExpiresAt);
        }
    }
}
