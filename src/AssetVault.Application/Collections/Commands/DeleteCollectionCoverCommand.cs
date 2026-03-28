using AssetVault.Application.Common.Interfaces;
using MediatR;

namespace AssetVault.Application.Collections.Commands
{
    public record DeleteCollectionCoverCommand(Guid UserId, Guid CollectionId) : IRequest;

    public class DeleteCollectionCoverCommandHandler(
        ICollectionRepository collectionRepository,
        IStorageService storageService
    ) : IRequestHandler<DeleteCollectionCoverCommand>
    {
        public async Task Handle(DeleteCollectionCoverCommand request, CancellationToken cancellationToken)
        {
            var collection = await collectionRepository.GetByIdAsync(request.CollectionId, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Collection {request.CollectionId} not found.");

            if (collection.UserId != request.UserId)
                throw new UnauthorizedAccessException("You do not have permission to modify this collection.");

            if (collection.CoverImageUrl is null)
                return;

            await storageService.DeletePublicAsync($"covers/{request.CollectionId}/cover", cancellationToken);
            collection.RemoveCoverImage();
            await collectionRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
