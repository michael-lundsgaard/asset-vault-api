using AssetVault.Domain.Entities;
using AssetVault.Domain.Enums;
using AssetVault.Domain.Events;
using FluentAssertions;

namespace AssetVault.UnitTests.Domain.Entities;

public class MediaAssetTests
{
    private static MediaAsset CreateAsset() =>
        MediaAsset.Create(Guid.NewGuid(), "photo.jpg", "image/jpeg", 1024);

    [Fact]
    public void Create_GivenValidArgs_ShouldSetStatusToPending()
    {
        var asset = CreateAsset();

        asset.Status.Should().Be(AssetStatus.Pending);
    }

    [Fact]
    public void Create_GivenValidArgs_ShouldRaiseAssetCreatedEvent()
    {
        var asset = CreateAsset();

        asset.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AssetCreatedEvent>();
    }

    [Fact]
    public void Create_GivenValidArgs_ShouldSetCorrectFileName()
    {
        var asset = MediaAsset.Create(Guid.NewGuid(), "document.pdf", "application/pdf", 2048);

        asset.FileName.Should().Be("document.pdf");
    }

    [Fact]
    public void MarkAsUploaded_ShouldSetStatusToActive()
    {
        var asset = CreateAsset();

        asset.MarkAsUploaded();

        asset.Status.Should().Be(AssetStatus.Active);
    }

    [Fact]
    public void MarkAsUploaded_ShouldRaiseAssetUploadedEvent()
    {
        var asset = CreateAsset();
        asset.ClearDomainEvents();

        asset.MarkAsUploaded();

        asset.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<AssetUploadedEvent>();
    }

    [Fact]
    public void MarkAsFailed_ShouldSetStatusToFailed()
    {
        var asset = CreateAsset();

        asset.MarkAsFailed();

        asset.Status.Should().Be(AssetStatus.Failed);
    }

    [Fact]
    public void AddTag_GivenNewTag_ShouldNormalizeToLowercaseAndAdd()
    {
        var asset = CreateAsset();

        asset.AddTag("NaturePhoto");

        asset.Tags.Should().ContainSingle().Which.Should().Be("naturephoto");
    }

    [Fact]
    public void AddTag_GivenDuplicateTag_ShouldNotAddAgain()
    {
        var asset = CreateAsset();
        asset.AddTag("landscape");

        asset.AddTag("landscape");

        asset.Tags.Should().ContainSingle();
    }

    [Fact]
    public void RemoveTag_GivenExistingTag_ShouldRemoveIt()
    {
        var asset = CreateAsset();
        asset.AddTag("removeme");

        asset.RemoveTag("removeme");

        asset.Tags.Should().BeEmpty();
    }

    [Fact]
    public void AddToCollection_GivenNewCollection_ShouldAddIt()
    {
        var asset = CreateAsset();
        var collection = Collection.Create(asset.UserId, "Favourites");

        asset.AddToCollection(collection);

        asset.Collections.Should().ContainSingle();
    }

    [Fact]
    public void AddToCollection_GivenDuplicateCollection_ShouldNotAddAgain()
    {
        var asset = CreateAsset();
        var collection = Collection.Create(asset.UserId, "Favourites");
        asset.AddToCollection(collection);

        asset.AddToCollection(collection);

        asset.Collections.Should().ContainSingle();
    }

    [Fact]
    public void RemoveFromCollection_GivenExistingCollection_ShouldRemoveIt()
    {
        var asset = CreateAsset();
        var collection = Collection.Create(asset.UserId, "Favourites");
        asset.AddToCollection(collection);

        asset.RemoveFromCollection(collection);

        asset.Collections.Should().BeEmpty();
    }

    [Fact]
    public void SetStoragePath_GivenValidPath_ShouldSetStoragePath()
    {
        var asset = CreateAsset();

        asset.SetStoragePath("uploads/abc/photo.jpg");

        asset.StoragePath.Should().NotBeNull();
        asset.StoragePath!.Value.Should().Be("uploads/abc/photo.jpg");
    }
}
