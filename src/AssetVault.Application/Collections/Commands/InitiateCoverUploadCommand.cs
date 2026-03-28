using AssetVault.Application.Common.Interfaces;
using FluentValidation;
using MediatR;

namespace AssetVault.Application.Collections.Commands
{
    public record InitiateCoverUploadCommand(
        Guid CollectionId,
        string ContentType,
        long SizeBytes
    ) : IRequest<InitiateCoverUploadResult>;

    public record InitiateCoverUploadResult(Guid CollectionId, string PresignedUrl, DateTime ExpiresAt);

    public class InitiateCoverUploadCommandValidator : AbstractValidator<InitiateCoverUploadCommand>
    {
        private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
        private const long MaxSizeBytes = 5 * 1024 * 1024;

        public InitiateCoverUploadCommandValidator()
        {
            RuleFor(x => x.ContentType)
                .Must(ct => AllowedContentTypes.Contains(ct))
                .WithMessage("Cover image must be image/jpeg, image/png, or image/webp.");

            RuleFor(x => x.SizeBytes)
                .InclusiveBetween(1, MaxSizeBytes)
                .WithMessage("Cover image must be between 1 byte and 5 MB.");
        }
    }

    public class InitiateCoverUploadCommandHandler(
        ICollectionRepository collectionRepository,
        IStorageService storageService
    ) : IRequestHandler<InitiateCoverUploadCommand, InitiateCoverUploadResult>
    {
        public async Task<InitiateCoverUploadResult> Handle(
            InitiateCoverUploadCommand request,
            CancellationToken cancellationToken)
        {
            var collection = await collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Collection {request.CollectionId} not found.");

            var presigned = await storageService.GenerateCoverImageUploadUrlAsync(
                request.CollectionId,
                request.ContentType,
                cancellationToken);

            return new InitiateCoverUploadResult(request.CollectionId, presigned.PresignedUrl, presigned.ExpiresAt);
        }
    }
}
