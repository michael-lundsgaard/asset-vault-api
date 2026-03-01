using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Enums;
using MediatR;

namespace AssetVault.Application.Assets.Queries
{
    public record GetAssetDownloadUrlQuery(Guid AssetId) : IRequest<AssetDownloadUrlResult>;

    public record AssetDownloadUrlResult(
        Guid AssetId,
        string PresignedUrl,
        DateTime ExpiresAt
    );

    public class GetAssetDownloadUrlQueryHandler(
        IAssetRepository assetRepository,
        IStorageService storageService
    ) : IRequestHandler<GetAssetDownloadUrlQuery, AssetDownloadUrlResult>
    {
        public async Task<AssetDownloadUrlResult> Handle(
            GetAssetDownloadUrlQuery request,
            CancellationToken cancellationToken)
        {
            var asset = await assetRepository.GetByIdAsync(request.AssetId, cancellationToken: cancellationToken)
                ?? throw new KeyNotFoundException($"Asset {request.AssetId} not found.");

            if (asset.Status != AssetStatus.Active || asset.StoragePath is null)
                throw new InvalidOperationException($"Asset {request.AssetId} is not available for download.");

            var result = await storageService.GenerateDownloadUrlAsync(
                asset.StoragePath.Value,
                cancellationToken);

            return new AssetDownloadUrlResult(asset.Id, result.PresignedUrl, result.ExpiresAt);
        }
    }
}
