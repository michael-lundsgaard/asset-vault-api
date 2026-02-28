using AssetVault.Application.Collections.Queries;
using AssetVault.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetVault.Infrastructure.Persistence.Extensions
{
    public static class CollectionQueryExtensions
    {
        public static IQueryable<Collection> ApplyFilters(
            this IQueryable<Collection> source,
            CollectionQuery query)
        {
            if (!string.IsNullOrWhiteSpace(query.Search))
                source = source.Where(c => EF.Functions.ILike(c.Name, $"%{query.Search}%"));

            return source;
        }

        public static IQueryable<Collection> ApplySorting(
            this IQueryable<Collection> source,
            CollectionQuery query)
        {
            return query.SortBy switch
            {
                CollectionSortBy.Name =>
                    query.SortDescending
                        ? source.OrderByDescending(x => x.Name)
                        : source.OrderBy(x => x.Name),

                _ =>
                    query.SortDescending
                        ? source.OrderByDescending(x => x.CreatedAt)
                        : source.OrderBy(x => x.CreatedAt)
            };
        }

        public static IQueryable<Collection> ApplyPaging(
            this IQueryable<Collection> source,
            CollectionQuery query)
        {
            return source
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);
        }
    }
}
