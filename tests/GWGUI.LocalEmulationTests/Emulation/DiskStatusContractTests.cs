using System.Text.Json;
using GWGUI.Emulation.Contracts;

namespace GWGUI.Tests.Emulation;

public sealed class DiskStatusContractTests
{
    [Fact]
    public void WebPayloadPreservesOriginalNamesOrderAndNulls()
    {
        var status = new DiskStatus(2, 1, false, [new DiskImageStatus(1, null, "Disk B")]);
        var json = JsonSerializer.Serialize(status, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal("{\"imageCount\":2,\"currentIndex\":1,\"isEjected\":false,\"images\":[{\"index\":1,\"path\":null,\"label\":\"Disk B\"}]}", json);
    }

    [Fact]
    public void OriginalWebPayloadCanBeReadWithNullableImageFields()
    {
        const string json = "{\"imageCount\":2,\"currentIndex\":1,\"isEjected\":false,\"images\":[{\"index\":1,\"path\":null,\"label\":null}]}";
        var status = JsonSerializer.Deserialize<DiskStatus>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(status);
        Assert.Equal(2, status.ImageCount);
        Assert.Equal(1, status.CurrentIndex);
        Assert.False(status.IsEjected);
        Assert.Equal(new DiskImageStatus(1, null, null), Assert.Single(status.Images));
    }

    [Fact]
    public void MissingFieldsKeepTheOriginalConstructorDefaults()
    {
        var status = JsonSerializer.Deserialize<DiskStatus>("{}", new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(status);
        Assert.Equal(0, status.ImageCount);
        Assert.Equal(0, status.CurrentIndex);
        Assert.False(status.IsEjected);
        Assert.Null(status.Images);
    }

    [Fact]
    public void ImageCollectionRemainsBorrowedAndCopiesKeepItsIdentity()
    {
        var images = new List<DiskImageStatus> { new(0, "disk.adf", null) };
        var status = new DiskStatus(1, 0, false, images);
        var copy = status with { IsEjected = true };
        Assert.Same(images, status.Images);
        Assert.Same(images, copy.Images);
        images.Add(new DiskImageStatus(1, null, "Disk B"));
        Assert.Equal(2, status.Images.Count);
        Assert.Equal(2, copy.Images.Count);
        Assert.False(status.IsEjected);
        Assert.True(copy.IsEjected);
    }

    [Fact]
    public void SimpleRecordNamesAndDeconstructionRemainStable()
    {
        var image = new DiskImageStatus(4, null, "Demo");
        var (index, path, label) = image;
        Assert.Equal(4, index);
        Assert.Null(path);
        Assert.Equal("Demo", label);
        Assert.Equal("DiskImageStatus { Index = 4, Path = , Label = Demo }", image.ToString());
        Assert.Equal("DiskStatus", typeof(DiskStatus).Name);
    }
}
