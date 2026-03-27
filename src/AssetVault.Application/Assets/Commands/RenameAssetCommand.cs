using AssetVault.Application.Assets.Mappings;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Responses.Assets;
using FluentValidation;
using MediatR;

namespace AssetVault.Application.Assets.Commands
{
    public record RenameAssetCommand(Guid AssetId, string FileName) : IRequest<AssetResponse>;

    public class RenameAssetCommandValidator : AbstractValidator<RenameAssetCommand>
    {
        public RenameAssetCommandValidator()
        {
            RuleFor(x => x.FileName).NotEmpty();
        }
    }

    public class RenameAssetCommandHandler(IAssetRepository assetRepository)
        : IRequestHandler<RenameAssetCommand, AssetResponse>
    {
        public async Task<AssetResponse> Handle(RenameAssetCommand request, CancellationToken cancellationToken)
        {
            var asset = await assetRepository.GetByIdAsync(request.AssetId, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Asset {request.AssetId} not found.");

            asset.Rename(request.FileName);
            await assetRepository.SaveChangesAsync(cancellationToken);

            return asset.ToResponse(AssetExpand.None);
        }
    }
}
