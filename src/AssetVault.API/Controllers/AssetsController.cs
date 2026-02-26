using AssetVault.API.Extensions;
using AssetVault.Application.Assets.Commands;
using AssetVault.Application.Assets.Queries;
using AssetVault.Contracts.Requests;
using AssetVault.Contracts.Responses;
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
        /// Supports ?expand=collection,tags to include related data.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<AssetResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? expand,
            CancellationToken cancellationToken)
        {
            var user = HttpContext.User;

            var expandFlags = ExpandParser.Parse(expand);
            var result = await mediator.Send(new GetAssetsQuery(expandFlags), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get an asset by ID.
        /// Supports ?expand=collection,tags to include related data.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(AssetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            Guid id,
            [FromQuery] string? expand,
            CancellationToken cancellationToken)
        {
            var expandFlags = ExpandParser.Parse(expand);
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
            var result = await mediator.Send(
                new InitiateUploadCommand(
                    request.FileName,
                    request.ContentType,
                    request.SizeInBytes,
                    request.CollectionId),
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.AssetId },
                new PresignedUploadResponse(result.AssetId, result.UploadUrl, result.UrlExpiresAt));
        }

        /// <summary>
        /// Client calls this after completing the S3 upload.
        /// </summary>
        [HttpPatch("{id:guid}/confirm")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmUpload(Guid id, CancellationToken cancellationToken)
        {
            await mediator.Send(new ConfirmUploadCommand(id), cancellationToken);
            return NoContent();
        }
    }
}