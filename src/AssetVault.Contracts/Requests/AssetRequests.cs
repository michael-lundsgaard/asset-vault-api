namespace AssetVault.Contracts.Requests
{
    public record InitiateUploadRequest(
        string FileName,
        string ContentType,
        long SizeInBytes
    );

    public record CreateCollectionRequest(
        string Name,
        string? Description = null
    );

    public record UpdateCollectionRequest(
        string Name,
        string? Description = null
    );

}