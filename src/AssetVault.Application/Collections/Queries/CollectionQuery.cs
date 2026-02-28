using AssetVault.Application.Common.Interfaces;
using FluentValidation;

namespace AssetVault.Application.Collections.Queries
{
    public enum CollectionSortBy
    {
        CreatedAt,
        Name,
    }

    public record CollectionQuery(
        int Page = 1,
        int PageSize = 20,
        string? Search = null,
        CollectionSortBy SortBy = CollectionSortBy.CreatedAt,
        bool SortDescending = false,
        CollectionExpand Expand = CollectionExpand.None);

    public class CollectionQueryValidator : AbstractValidator<CollectionQuery>
    {
        public CollectionQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
