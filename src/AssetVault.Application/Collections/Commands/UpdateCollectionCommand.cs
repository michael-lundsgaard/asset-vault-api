using AssetVault.Application.Collections.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Collections;
using MediatR;

namespace AssetVault.Application.Collections.Commands
{
    public record UpdateCollectionCommand(Guid UserId, Guid Id, string Name, string? Description)
        : IRequest<CollectionResponse>;

    public class UpdateCollectionCommandHandler(ICollectionRepository collectionRepository)
        : IRequestHandler<UpdateCollectionCommand, CollectionResponse>
    {
        public async Task<CollectionResponse> Handle(
            UpdateCollectionCommand request,
            CancellationToken cancellationToken)
        {
            var collection = await collectionRepository.GetByIdAsync(request.Id, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Collection {request.Id} not found.");

            if (collection.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to modify this collection.");
            }

            collection.Update(request.Name, request.Description);

            await collectionRepository.UpdateAsync(collection, cancellationToken);
            await collectionRepository.SaveChangesAsync(cancellationToken);

            return collection.ToResponse(CollectionExpand.None);
        }
    }
}
