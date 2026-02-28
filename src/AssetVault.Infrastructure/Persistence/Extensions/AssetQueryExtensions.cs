using AssetVault.Application.Assets.Queries;
using AssetVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetVault.Infrastructure.Persistence.Extensions
{
    public static class AssetQueryExtensions
    {
        public static IQueryable<MediaAsset> ApplyFilters(
            this IQueryable<MediaAsset> source,
            AssetQuery query)
        {
            if (!string.IsNullOrWhiteSpace(query.Search))
                source = source.Where(a => EF.Functions.ILike(a.FileName, $"%{query.Search}%"));

            if (!string.IsNullOrWhiteSpace(query.ContentType))
                source = source.Where(a => a.ContentType == query.ContentType);

            if (query.Tags?.Count > 0)
                source = source.Where(a => query.Tags.Any(tag => a.Tags.Contains(tag)));

            if (query.Status.HasValue)
                source = source.Where(a => a.Status == query.Status.Value);

            return source;
        }

        public static IQueryable<MediaAsset> ApplySorting(
            this IQueryable<MediaAsset> source,
            AssetQuery query)
        {
            return query.SortBy switch
            {
                AssetSortBy.FileName =>
                    query.SortDescending
                        ? source.OrderByDescending(x => x.FileName)
                        : source.OrderBy(x => x.FileName),

                AssetSortBy.ContentType =>
                    query.SortDescending
                        ? source.OrderByDescending(x => x.ContentType)
                        : source.OrderBy(x => x.ContentType),

                AssetSortBy.FileSize =>
                    query.SortDescending
                        ? source.OrderByDescending(x => x.Size.Bytes)
                        : source.OrderBy(x => x.Size.Bytes),

                _ =>
                    query.SortDescending
                        ? source.OrderByDescending(x => x.CreatedAt)
                        : source.OrderBy(x => x.CreatedAt)
            };
        }

        public static IQueryable<MediaAsset> ApplyPaging(
            this IQueryable<MediaAsset> source,
            AssetQuery query)
        {
            return source
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);
        }
    }
}
