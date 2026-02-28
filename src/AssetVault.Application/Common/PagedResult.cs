namespace AssetVault.Application.Common
{
    public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int PageNumber, int PageSize);
}
