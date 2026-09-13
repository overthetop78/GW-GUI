using System.IO.Compression;
using GWGUI.Emulation.HardDisks;
using GWGUI.Emulation.HardDisks.Containers;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class ContainerSignatureTests
{
    [Theory]
    [InlineData(DiskContainerKind.Vhd, "vhd")]
    [InlineData(DiskContainerKind.Vhdx, "vhdx")]
    [InlineData(DiskContainerKind.Vdi, "vdi")]
    [InlineData(DiskContainerKind.Vmdk, "vmdk")]
    [InlineData(DiskContainerKind.Qcow2, "qcow")]
    [InlineData(DiskContainerKind.Qcow, "qcow")]
    [InlineData(DiskContainerKind.Qed, "qed")]
    [InlineData(DiskContainerKind.Chd, "chd")]
    [InlineData(DiskContainerKind.TwoImg, "twoimg")]
    [InlineData(DiskContainerKind.Parallels, "parallels")]
    [InlineData(DiskContainerKind.Nhd, "nhd")]
    [InlineData(DiskContainerKind.Gzip, "gzip")]
    public void CreatedContainersCannotBeReusedAsRenamedRawImages(DiskContainerKind kind, string signature)
    {
        using var image = new MemoryStream(); DiskContainerWriter.Write(image, 2L << 20, kind);
        image.Position = 17; Assert.Equal(signature, DiskContainerSignatures.Identify(image)); Assert.Equal(17, image.Position);
        var format = new HardDiskImageFormat("raw", ".img", "block", 64L << 20, 2L << 20);
        Assert.ThrowsAny<Exception>(() => HardDiskImageValidation.ValidateExisting(image, "renamed.img", format));
        Assert.Equal(17, image.Position); Assert.True(image.CanRead);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void FooterBasedContainersAreIdentified(bool udif)
    {
        using var image = new MemoryStream();
        if (udif) UdifImageWriter.Write(image, 2L << 20);
        else VhdImageWriter.Write(image, 2L << 20, true);
        Assert.Equal(udif ? "udif" : "vhd", DiskContainerSignatures.Identify(image));
    }

    [Fact]
    public void CompressedContainerIsNotMistakenForCompressedRawSectors()
    {
        using var parent = new MemoryStream(); VhdImageWriter.Write(parent, 2L << 20, true);
        using var compressed = new MemoryStream();
        using (var gzip = new GZipStream(compressed, CompressionLevel.Optimal, true))
        { parent.Position = 0; parent.CopyTo(gzip); }
        var format = new HardDiskImageFormat("gzip", ".gz", "block", 64L << 20, 2L << 20, Container: DiskContainerKind.Gzip);
        compressed.Position = 7;
        Assert.Throws<InvalidDataException>(() => HardDiskImageValidation.ValidateExisting(compressed, "renamed.gz", format));
        Assert.Equal(7, compressed.Position); Assert.True(compressed.CanRead);
    }

    [Theory]
    [InlineData(false)] [InlineData(true)]
    public void RawSectorsRemainAcceptedWithAndWithoutCompression(bool compressed)
    {
        using var image = new MemoryStream();
        DiskContainerWriter.Write(image, 4096, compressed ? DiskContainerKind.Gzip : DiskContainerKind.Raw,
            content => { content.Position = 4095; content.WriteByte(42); });
        var extension = compressed ? ".gz" : ".img";
        var format = new HardDiskImageFormat("raw", extension, "block", 4096, 4096,
            Container: compressed ? DiskContainerKind.Gzip : DiskContainerKind.Raw);
        image.Position = 5; HardDiskImageValidation.ValidateExisting(image, "image" + extension, format);
        Assert.Equal(5, image.Position); Assert.True(image.CanRead);
    }
}
