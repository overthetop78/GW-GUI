using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Conversion.Fat12;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.SectorImages;
using System.Buffers.Binary;
using System.IO;

namespace GWGUI.Tests;

public sealed class Fat12ReinterpretationTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.AtariSt720)]
    [InlineData(DiskImageFormatIds.Ibm720)]
    [InlineData(DiskImageFormatIds.Msx2Dd)]
    public void PolicyAcceptsExactlyCompatibleFat12Targets(string targetFormatId)
    {
        var target = Fat12ReinterpretationPolicy.Validate(CreateImage(), targetFormatId);

        Assert.Equal(targetFormatId, target.FormatId);
        Assert.Equal(737_280, target.Capacity);
    }

    [Theory]
    [InlineData(DiskImageFormatIds.AtariSt720, ".st")]
    [InlineData(DiskImageFormatIds.Ibm720, ".ima")]
    [InlineData(DiskImageFormatIds.Msx2Dd, ".dsk")]
    public async Task ServiceUsesTheTargetFamilyWriterWithoutChangingLogicalBlocks(string targetFormatId, string extension)
    {
        var source = CreateImage();
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{extension}");
        try
        {
            await MediaEngineFactory.CreateFat12ReinterpretationService().ConvertAsync(source, path, targetFormatId);

            var bytes = await File.ReadAllBytesAsync(path);
            Assert.Equal(source.Capacity, bytes.LongLength);
            for (var logicalBlock = 0; logicalBlock < source.BlockCount; logicalBlock++) Assert.Equal(source.GetBlock(logicalBlock).ToArray(), bytes.AsSpan(logicalBlock * source.BlockSize, source.BlockSize).ToArray());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void PolicyRejectsIncompatibleGeometry()
    {
        Assert.Throws<InvalidDataException>(() => Fat12ReinterpretationPolicy.Validate(CreateImage(), DiskImageFormatIds.Ibm800));
    }

    [Fact]
    public void PolicyRejectsAContradictoryBpb()
    {
        var source = CreateImage(sectorsPerTrackInBpb: 10);

        Assert.Throws<InvalidDataException>(() => Fat12ReinterpretationPolicy.Validate(source, DiskImageFormatIds.Ibm720));
    }

    [Fact]
    public void PolicyRejectsMissingSectors()
    {
        var source = CreateImage();
        var incomplete = new SectorImage(source.FormatId, source.BlockSize, source.Cylinders, source.Heads, source.SectorsPerTrack, source.AvailableBlocks.Where(block => block.LogicalBlock != 42));

        Assert.Throws<InvalidDataException>(() => Fat12ReinterpretationPolicy.Validate(incomplete, DiskImageFormatIds.Ibm720));
    }

    [Fact]
    public void PolicyRejectsHybridSources()
    {
        Assert.Throws<InvalidDataException>(() => Fat12ReinterpretationPolicy.Validate(CreateImage(), DiskImageFormatIds.Ibm720, sourceIsHybrid: true));
    }

    [Fact]
    public void PolicyRejectsTargetsOutsideTheThreeFat12Families()
    {
        Assert.Throws<InvalidDataException>(() => Fat12ReinterpretationPolicy.Validate(CreateImage(), DiskImageFormatIds.AmigaDos));
    }

    private static SectorImage CreateImage(ushort sectorsPerTrackInBpb = 9)
    {
        const int blockSize = 512;
        const int cylinders = 80;
        const int heads = 2;
        const int sectorsPerTrack = 9;
        const int blockCount = cylinders * heads * sectorsPerTrack;
        var blocks = Enumerable.Range(0, blockCount).Select(logicalBlock =>
        {
            var data = Enumerable.Range(0, blockSize).Select(index => (byte)(logicalBlock * 17 + index * 31)).ToArray();
            if (logicalBlock == 0) WriteBpb(data, sectorsPerTrackInBpb);
            var track = logicalBlock / sectorsPerTrack;
            return new SectorBlock(logicalBlock, new(track / heads, track % heads, logicalBlock % sectorsPerTrack + 1), data);
        });
        return new(DiskImageFormatIds.AtariSt720, blockSize, cylinders, heads, sectorsPerTrack, blocks);
    }

    private static void WriteBpb(Span<byte> boot, ushort sectorsPerTrack)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(boot[FatBootSectorLayout.BytesPerSectorOffset..], 512);
        boot[FatBootSectorLayout.SectorsPerClusterOffset] = 2;
        BinaryPrimitives.WriteUInt16LittleEndian(boot[FatBootSectorLayout.ReservedSectorCountOffset..], 1);
        boot[FatBootSectorLayout.FatCountOffset] = 2;
        BinaryPrimitives.WriteUInt16LittleEndian(boot[FatBootSectorLayout.RootEntryCountOffset..], 112);
        BinaryPrimitives.WriteUInt16LittleEndian(boot[FatBootSectorLayout.TotalSectors16Offset..], 1440);
        boot[FatBootSectorLayout.MediaDescriptorOffset] = 0xf9;
        BinaryPrimitives.WriteUInt16LittleEndian(boot[FatBootSectorLayout.SectorsPerFatOffset..], 3);
        BinaryPrimitives.WriteUInt16LittleEndian(boot[FatBootSectorLayout.SectorsPerTrackOffset..], sectorsPerTrack);
        BinaryPrimitives.WriteUInt16LittleEndian(boot[FatBootSectorLayout.HeadCountOffset..], 2);
    }
}
