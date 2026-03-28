using AssetVault.Application.Common.Interfaces;
using MediatR;

namespace AssetVault.Application.Collections.Commands
{
    public record DeleteCollectionCommand(Guid UserId, Guid Id) : IRequest;

    public class DeleteCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        IStorageService storageService
    ) : IRequestHandler<DeleteCollectionCommand>
    {
        public async Task Handle(DeleteCollectionCommand request, CancellationToken cancellationToken)
        {
            var collection = await collectionRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Collection {request.Id} not found.");

            if (collection.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this collection.");
            }

            if (collection.CoverImageUrl is not null)
                await storageService.DeletePublicAsync($"covers/{request.Id}/cover", cancellationToken);

            await collectionRepository.DeleteAsync(collection, cancellationToken);
            await collectionRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
