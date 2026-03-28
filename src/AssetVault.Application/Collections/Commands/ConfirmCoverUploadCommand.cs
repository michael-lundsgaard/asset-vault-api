using AssetVault.Application.Collections.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Collections;
using MediatR;

namespace AssetVault.Application.Collections.Commands
{
    public record ConfirmCoverUploadCommand(Guid UserId, Guid CollectionId) : IRequest<CollectionResponse>;

    public class ConfirmCoverUploadCommandHandler(
        ICollectionRepository collectionRepository,
        IStorageService storageService
    ) : IRequestHandler<ConfirmCoverUploadCommand, CollectionResponse>
    {
        public async Task<CollectionResponse> Handle(
            ConfirmCoverUploadCommand request,
            CancellationToken cancellationToken)
        {
            var collection = await collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Collection {request.CollectionId} not found.");

            if (collection.UserId != request.UserId)
                throw new UnauthorizedAccessException("You do not have permission to modify this collection.");

            var coverPath = $"covers/{request.CollectionId}/cover";
            var coverImageUrl = storageService.GetPublicUrl(coverPath);
            collection.SetCoverImageUrl(coverImageUrl);
            await collectionRepository.SaveChangesAsync(cancellationToken);

            return collection.ToResponse(CollectionExpand.None);
        }
    }
}
