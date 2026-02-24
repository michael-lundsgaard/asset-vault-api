using AssetVault.Application.Common.Interfaces;
using MediatR;

namespace AssetVault.Application.Assets.Commands
{
    public record ConfirmUploadCommand(Guid AssetId) : IRequest;

    public class ConfirmUploadCommandHandler(IAssetRepository assetRepository)
        : IRequestHandler<ConfirmUploadCommand>
    {
        public async Task Handle(ConfirmUploadCommand request, CancellationToken cancellationToken)
        {
            var asset = await assetRepository.GetByIdAsync(request.AssetId, cancellationToken)
                ?? throw new KeyNotFoundException($"Asset {request.AssetId} not found.");

            asset.MarkAsUploaded();
            await assetRepository.SaveChangesAsync(cancellationToken);
        }
    }
}