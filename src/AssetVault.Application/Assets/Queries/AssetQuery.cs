using AssetVault.Application.Common.Interfaces;
using AssetVault.Domain.Enums;
using FluentValidation;

namespace AssetVault.Application.Assets.Queries
{
    public enum AssetSortBy
    {
        CreatedAt,
        FileName,
        ContentType,
        FileSize,
    }

    public record AssetQuery(
        int Page = 1,
        int PageSize = 20,
        string? Search = null,
        string? ContentType = null,
        List<string>? Tags = null,
        AssetStatus? Status = null,
        AssetSortBy SortBy = AssetSortBy.CreatedAt,
        bool SortDescending = false,
        AssetExpand Expand = AssetExpand.None);

    public class AssetQueryValidator : AbstractValidator<AssetQuery>
    {
        public AssetQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        }
    }
}
