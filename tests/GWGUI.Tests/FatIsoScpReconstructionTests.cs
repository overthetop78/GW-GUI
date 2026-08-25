using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Reconstruction.Iso;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
using GWGUI.MediaEngine.Decoding;

namespace GWGUI.Tests;

public sealed class FatIsoScpReconstructionTests
{
    [Fact]
    public void DetectsLogicalGeometryFromShortPhysicalBootSector()
    {
        var boot = BootSector(1600, 10, 2);
        var candidates = Candidates((new(0, 0, 1), boot));

        Assert.True(FatIsoScpGeometryDetector.TryDetect(candidates, out var geometry));
        Assert.Equal(80, geometry.Cylinders);
        Assert.Equal(2, geometry.Heads);
        Assert.Equal(10, geometry.SectorsPerTrack);
        Assert.Equal(512, geometry.SectorSize);
    }

    [Fact]
    public void NormalizesMixedPhysicalSectorSizesWithoutTruncation()
    {
        var candidates = Candidates(
            (new(0, 0, 1), Enumerable.Repeat((byte)0x5a, 256).ToArray()),
            (new(0, 0, 2), Enumerable.Repeat((byte)0xa5, 512).ToArray()));

        var image = IsoSectorImageBuilder.CreateUniform("test", candidates, 512, 1, 1, 2, address => address.Number - 1,
            normalizeData: data => IsoSectorDataNormalizer.PadTo(data, 512));

        Assert.Equal(512, image.GetBlock(0).Length);
        Assert.All(image.GetBlock(0).Span[..256].ToArray(), value => Assert.Equal(0x5a, value));
        Assert.All(image.GetBlock(0).Span[256..].ToArray(), value => Assert.Equal(0, value));
        Assert.All(image.GetBlock(1).ToArray(), value => Assert.Equal(0xa5, value));
        Assert.Empty(IsoSectorDataNormalizer.PadTo(new byte[1024], 512));
    }

    [Fact]
    public void AcceptsAllocatedLegacyFatButRejectsAnEmptyOne()
    {
        var empty = new byte[512];
        var allocated = new byte[512];
        allocated[3] = 0xff;
        allocated[4] = 0x0f;

        Assert.False(Fat12FatReader.IsUsable(empty, FatBootSectorLayout.UnknownMediaDescriptor, 32));
        Assert.True(Fat12FatReader.IsUsable(allocated, FatBootSectorLayout.UnknownMediaDescriptor, 32));
    }

    private static Dictionary<SectorAddress, List<IsoSectorCandidate>> Candidates(params (SectorAddress Address, byte[] Data)[] sectors) =>
        sectors.ToDictionary(item => item.Address, item => new List<IsoSectorCandidate> { new(new((byte)item.Address.Cylinder, (byte)item.Address.Head, item.Address.Number, (byte)(item.Data.Length == 256 ? 1 : 2), item.Data.Length, true, 0, Data: item.Data), 1) });

    private static byte[] BootSector(ushort totalSectors, ushort sectorsPerTrack, ushort heads)
    {
        var boot = new byte[256];
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.BytesPerSectorOffset), 512);
        boot[FatBootSectorLayout.SectorsPerClusterOffset] = 2;
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.ReservedSectorCountOffset), 1);
        boot[FatBootSectorLayout.FatCountOffset] = 2;
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.RootEntryCountOffset), 80);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.TotalSectors16Offset), totalSectors);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.SectorsPerFatOffset), 9);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.SectorsPerTrackOffset), sectorsPerTrack);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.HeadCountOffset), heads);
        return boot;
    }
}
