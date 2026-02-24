namespace AssetVault.Contracts.Requests
{
    public record InitiateUploadRequest(
        string FileName,
        string ContentType,
        long SizeInBytes,
        Guid? CollectionId = null
    );

    public record CreateCollectionRequest(
        string Name,
        string? Description = null
    );

}