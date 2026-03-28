using AssetVault.API.Extensions;
using AssetVault.Application.Assets.Commands;
using AssetVault.Application.Assets.Queries;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Requests.Assets;
using AssetVault.Contracts.Responses.Assets;
using AssetVault.Contracts.Responses.Common;
using AssetVault.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetVault.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AssetsController(ISender mediator) : ControllerBase
    {
        /// <summary>
        /// Get all assets.
        /// Supports ?expand=collections and filter/sort params: search, contentType, tags, status, sortBy, sortDescending, page, pageSize.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<AssetResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] GetAssetsRequest request,
            [FromQuery] string? expand,
            CancellationToken cancellationToken)
        {
            var query = BuildAssetQuery(request, expand);
            var result = await mediator.Send(new GetAssetsQuery(query), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get all assets owned by a specific user.
        /// Supports ?expand=collections and filter/sort params: search, contentType, tags, status, sortBy, sortDescending, page, pageSize.
        /// </summary>
        [HttpGet("owner/{userId:guid}")]
        [ProducesResponseType(typeof(PaginatedResponse<AssetResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByOwner(
            Guid userId,
            [FromQuery] GetAssetsRequest request,
            [FromQuery] string? expand,
            CancellationToken cancellationToken)
        {
            var query = BuildAssetQuery(request, expand);
            var result = await mediator.Send(new GetAssetsByOwnerQuery(userId, query), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get an asset by ID.
        /// Supports ?expand=collections to include related data.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AssetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            Guid id,
            [FromQuery] string? expand,
            CancellationToken cancellationToken)
        {
            var expandFlags = ExpandParser.Parse<AssetExpand>(expand);
            var result = await mediator.Send(new GetAssetByIdQuery(id, expandFlags), cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Initiates an upload — returns a pre-signed S3 URL.
        /// Client uploads directly to S3, then calls PATCH /assets/{id}/confirm.
        /// </summary>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(PresignedUploadResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> InitiateUpload(
            [FromBody] InitiateUploadRequest request,
            CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetRequiredUserProfile().Id;

            var result = await mediator.Send(
                new InitiateUploadCommand(
                    userId,
                    request.FileName,
                    request.ContentType,
                    request.SizeBytes),
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.AssetId },
                new PresignedUploadResponse(result.AssetId, result.PresignedUrl, result.ExpiresAt));
        }

        /// <summary>
        /// Returns a short-lived pre-signed S3 URL for downloading an asset.
        /// </summary>
        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(PresignedDownloadResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDownloadUrl(Guid id, CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetAssetDownloadUrlQuery(id), cancellationToken);
            return Ok(new PresignedDownloadResponse(result.AssetId, result.PresignedUrl, result.ExpiresAt));
        }

        /// <summary>
        /// Client calls this after completing the S3 upload.
        /// </summary>
        [HttpPatch("{id:guid}/confirm")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmUpload(Guid id, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetRequiredUserProfile().Id;

            await mediator.Send(new ConfirmUploadCommand(userId, id), cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Renames an asset. Request body: { "fileName": "new-name.mp4" }
        /// </summary>
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(AssetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Rename(
            Guid id,
            [FromBody] RenameAssetRequest request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new RenameAssetCommand(id, request.FileName), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Initiates a thumbnail upload — returns a pre-signed S3 URL.
        /// Client uploads directly to S3, then calls PATCH /assets/{id}/thumbnail/confirm.
        /// </summary>
        [HttpPost("{id:guid}/thumbnail/upload")]
        [ProducesResponseType(typeof(InitiateThumbnailUploadResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InitiateThumbnailUpload(
            Guid id,
            [FromBody] InitiateThumbnailUploadRequest request,
            CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetRequiredUserProfile().Id;
            var result = await mediator.Send(
                new InitiateThumbnailUploadCommand(userId, id, request.ContentType, request.SizeBytes),
                cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Client calls this after completing the thumbnail S3 upload.
        /// </summary>
        [HttpPatch("{id:guid}/thumbnail/confirm")]
        [ProducesResponseType(typeof(AssetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmThumbnailUpload(Guid id, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetRequiredUserProfile().Id;
            var result = await mediator.Send(new ConfirmThumbnailUploadCommand(userId, id), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Deletes the thumbnail for an asset.
        /// </summary>
        [HttpDelete("{id:guid}/thumbnail")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteThumbnail(Guid id, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetRequiredUserProfile().Id;
            await mediator.Send(new DeleteAssetThumbnailCommand(userId, id), cancellationToken);
            return NoContent();
        }

        private static AssetQuery BuildAssetQuery(GetAssetsRequest request, string? expand)
        {
            AssetStatus? status = Enum.TryParse<AssetStatus>(request.Status, ignoreCase: true, out var parsedStatus)
                ? parsedStatus
                : null;

            AssetSortBy sortBy = Enum.TryParse<AssetSortBy>(request.SortBy, ignoreCase: true, out var parsedSortBy)
                ? parsedSortBy
                : AssetSortBy.CreatedAt;

            return new AssetQuery(
                request.Page,
                request.PageSize,
                request.Search,
                request.ContentType,
                request.Tags,
                status,
                sortBy,
                request.SortDescending,
                ExpandParser.Parse<AssetExpand>(expand));
        }
    }
}
