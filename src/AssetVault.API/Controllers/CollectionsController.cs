using AssetVault.API.Extensions;
using AssetVault.Application.Collections.Commands;
using AssetVault.Application.Collections.Queries;
using AssetVault.Application.Common.Interfaces;
using AssetVault.Contracts.Requests.Collections;
using AssetVault.Contracts.Responses.Collections;
using AssetVault.Contracts.Responses.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetVault.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CollectionsController(ISender mediator) : ControllerBase
    {
        /// <summary>
        /// Get all collections.
        /// Supports ?expand=assets and filter/sort params: search, sortBy, sortDescending, page, pageSize.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedResponse<CollectionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] GetCollectionsRequest request,
            [FromQuery] string? expand,
            CancellationToken cancellationToken)
        {
            var query = BuildCollectionQuery(request, expand);
            var result = await mediator.Send(new GetCollectionsQuery(query), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get all collections owned by a specific user.
        /// Supports ?expand=assets and filter/sort params: search, sortBy, sortDescending, page, pageSize.
        /// </summary>
        [HttpGet("owner/{userId:guid}")]
        [ProducesResponseType(typeof(PaginatedResponse<CollectionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByOwner(
            Guid userId,
            [FromQuery] GetCollectionsRequest request,
            [FromQuery] string? expand,
            CancellationToken cancellationToken)
        {
            var query = BuildCollectionQuery(request, expand);
            var result = await mediator.Send(new GetCollectionsByOwnerQuery(userId, query), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get a collection by ID.
        /// Supports ?expand=assets to include related data.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(CollectionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            Guid id,
            [FromQuery] string? expand,
            CancellationToken cancellationToken)
        {
            var expandFlags = ExpandParser.Parse<CollectionExpand>(expand);
            var result = await mediator.Send(new GetCollectionByIdQuery(id, expandFlags), cancellationToken);

            return result is null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Create a new collection.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CollectionResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(
            [FromBody] CreateCollectionRequest request,
            CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetRequiredUserProfile().Id;

            var result = await mediator.Send(
                new CreateCollectionCommand(userId, request.Name, request.Description),
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Update a collection's name and description.
        /// </summary>
        [HttpPatch("{id:guid}")]
        [ProducesResponseType(typeof(CollectionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateCollectionRequest request,
            CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetRequiredUserProfile().Id;

            var result = await mediator.Send(
                new UpdateCollectionCommand(userId, id, request.Name, request.Description),
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Delete a collection.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetRequiredUserProfile().Id;

            await mediator.Send(new DeleteCollectionCommand(userId, id), cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Add an asset to a collection.
        /// </summary>
        [HttpPost("{id:guid}/assets/{assetId:guid}")]
        [EndpointName("AddAssetToCollection")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddAsset(
            Guid id,
            Guid assetId,
            CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetRequiredUserProfile().Id;

            await mediator.Send(new AddAssetToCollectionCommand(userId, id, assetId), cancellationToken);
            return NoContent();
        }

        /// <summary>
        /// Remove an asset from a collection.
        /// </summary>
        [HttpDelete("{id:guid}/assets/{assetId:guid}")]
        [EndpointName("RemoveAssetFromCollection")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveAsset(
            Guid id,
            Guid assetId,
            CancellationToken cancellationToken)
        {
            var userId = HttpContext.GetRequiredUserProfile().Id;

            await mediator.Send(new RemoveAssetFromCollectionCommand(userId, id, assetId), cancellationToken);
            return NoContent();
        }

        private static CollectionQuery BuildCollectionQuery(GetCollectionsRequest request, string? expand)
        {
            CollectionSortBy sortBy = Enum.TryParse<CollectionSortBy>(request.SortBy, ignoreCase: true, out var parsedSortBy)
                ? parsedSortBy
                : CollectionSortBy.CreatedAt;

            return new CollectionQuery(
                request.Page,
                request.PageSize,
                request.Search,
                sortBy,
                request.SortDescending,
                ExpandParser.Parse<CollectionExpand>(expand));
        }
    }
}
