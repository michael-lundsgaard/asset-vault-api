using AssetVault.Application.Collections.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Collections;
using AssetVault.Domain.Entities;
using MediatR;

namespace AssetVault.Application.Collections.Commands
{
    public record CreateCollectionCommand(Guid UserId, string Name, string? Description)
        : IRequest<CollectionResponse>;

    public class CreateCollectionCommandHandler(ICollectionRepository collectionRepository)
        : IRequestHandler<CreateCollectionCommand, CollectionResponse>
    {
        public async Task<CollectionResponse> Handle(
            CreateCollectionCommand request,
            CancellationToken cancellationToken)
        {
            var collection = Collection.Create(request.UserId, request.Name, request.Description);

            await collectionRepository.AddAsync(collection, cancellationToken);
            await collectionRepository.SaveChangesAsync(cancellationToken);

            return collection.ToResponse(CollectionExpand.None);
        }
    }
}
