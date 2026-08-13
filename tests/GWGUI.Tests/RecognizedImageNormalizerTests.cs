using System.Buffers.Binary;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Interpretation.Normalizers;
using GWGUI.MediaEngine.Exploration.Interpretation.Policies;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Apple;
using GWGUI.MediaEngine.Geometries.Msx;
using GWGUI.MediaEngine.Recognition.Msx;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie les normalisations Atari et Macintosh extraites.</summary>
public sealed class RecognizedImageNormalizerTests
{
    [Fact]
    public void AtariNormalizerShrinksValidatedBpbAndRejectsWrongReader()
    {
        var image = ImageWithBoot(DiskImageFormatIds.AtariSt360, totalSectors: 2, heads: 1, sectorsPerTrack: 2, blockCount: 4);
        var normalizer = new AtariRecognizedImageNormalizer();
        Assert.False(normalizer.TryNormalize(image, "other", Volume([]), out var unchanged));
        Assert.Same(image, unchanged);
        Assert.True(normalizer.TryNormalize(image, FileSystemIds.Fat12, Volume([]), out var normalized));
        Assert.Equal(2, normalized.BlockCount);
        Assert.Equal(2L * FatBootSectorLayout.SectorSize, normalized.Capacity);
        Assert.Equal(2, normalized.AvailableBlocks.Count);
    }

    [Theory]
    [InlineData(FileSystemIds.MacHfs)]
    [InlineData(FileSystemIds.MacMfs)]
    public void MacNormalizerRequiresReaderAndCompleteMfmGeometry(string readerId)
    {
        var image = new SectorImage(DiskImageFormatIds.Ibm1440, MacintoshMfmGeometry.SectorSize, MacintoshMfmGeometry.CylinderCount, MacintoshMfmGeometry.HeadCount, MacintoshMfmGeometry.SectorsPerTrack, []);
        Assert.True(new MacRecognizedImageNormalizer().TryNormalize(image, readerId, Volume([]), out var normalized));
        Assert.Equal(DiskImageFormatIds.Mac1440, normalized.FormatId);
        Assert.Equal(image.Capacity, normalized.Capacity);
    }

    [Fact]
    public void MacNormalizerRejectsWrongReaderGeometryAndAlreadyNormalizedImage()
    {
        var complete = new SectorImage(DiskImageFormatIds.Ibm1440, MacintoshMfmGeometry.SectorSize, MacintoshMfmGeometry.CylinderCount, MacintoshMfmGeometry.HeadCount, MacintoshMfmGeometry.SectorsPerTrack, []);
        var normalizer = new MacRecognizedImageNormalizer();
        Assert.False(normalizer.TryNormalize(complete, "other", Volume([]), out _));
        Assert.False(normalizer.TryNormalize(new SectorImage("other", 256, 1, 1, 1, []), FileSystemIds.MacHfs, Volume([]), out _));
        Assert.False(normalizer.TryNormalize(complete.WithFormatId(DiskImageFormatIds.Mac1440), FileSystemIds.MacHfs, Volume([]), out _));
    }

    [Theory]
    [InlineData(360, 0xf0, DiskImageFormatIds.Msx1D)]
    [InlineData(720, MsxDiskGeometryCatalog.OneDoubleDensityMediaDescriptor, DiskImageFormatIds.Msx1Dd)]
    [InlineData(720, 0xf9, DiskImageFormatIds.Msx2D)]
    [InlineData(1440, 0xf9, DiskImageFormatIds.Msx2Dd)]
    public void MsxInterpreterRecognizesEveryGeometry(int sectors, byte media, string expectedFormat)
    {
        var source = MsxImage(sectors, media);
        Assert.True(new MsxSectorImageInterpreter().TryInterpret(source, out var interpreted));
        Assert.Equal(expectedFormat, interpreted.FormatId);
        Assert.Equal(source.Capacity, interpreted.Capacity);
    }

    [Fact]
    public void MsxInterpreterRejectsAlreadyMsxAndInvalidBoot()
    {
        var interpreter = new MsxSectorImageInterpreter();
        Assert.False(interpreter.TryInterpret(MsxImage(360, 0xf0).WithFormatId(DiskImageFormatIds.Msx1D), out _));
        Assert.False(interpreter.TryInterpret(new SectorImage("ibm.180", 512, 1, 1, 360, [], logicalBlockCount: 360), out _));
    }

    [Fact]
    public void IbmPolicyUsesSupportedGeometryAndCopiesSupportedFormats()
    {
        var formats = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { DiskImageFormatIds.Ibm360 };
        var policy = new IbmAdditionalImageInterpretationPolicy(formats);
        formats.Clear();
        var source = IbmImage(720, 2, 9);
        Assert.Equal(DiskImageFormatIds.Ibm360, policy.CreateCandidates(source).Single().FormatId);
        Assert.Empty(policy.CreateCandidates(source.WithFormatId(DiskImageFormatIds.Ibm360)));
        Assert.Equal(DiskImageFormatIds.IbmScan, new IbmAdditionalImageInterpretationPolicy([]).CreateCandidates(source).Single().FormatId);
    }

    [Theory]
    [InlineData((byte)FatMediaDescriptor.Ibm160, DiskImageFormatIds.Ibm160)]
    [InlineData((byte)FatMediaDescriptor.Ibm180, DiskImageFormatIds.Ibm180)]
    [InlineData((byte)FatMediaDescriptor.Ibm320, DiskImageFormatIds.Ibm320)]
    [InlineData((byte)FatMediaDescriptor.Ibm360, DiskImageFormatIds.Ibm360)]
    public void IbmPolicySupportsEveryHistoricalFatDescriptor(byte descriptor, string expectedFormat)
    {
        var boot = new byte[512];
        var fat = new byte[512];
        fat[FatBootSectorLayout.FatMediaDescriptorDataOffset] = descriptor;
        var source = new SectorImage("atarist.raw", 512, 1, 1, 2, [new SectorBlock(0, new SectorAddress(0, 0, 1), boot), new SectorBlock(1, new SectorAddress(0, 0, 2), fat)]);
        Assert.Equal(expectedFormat, new IbmAdditionalImageInterpretationPolicy([expectedFormat]).CreateCandidates(source).Single().FormatId);
    }

    private static SectorImage ImageWithBoot(string formatId, int totalSectors, int heads, int sectorsPerTrack, int blockCount)
    {
        var boot = new byte[FatBootSectorLayout.SectorSize];
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.BytesPerSectorOffset), FatBootSectorLayout.SectorSize);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.TotalSectors16Offset), (ushort)totalSectors);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.SectorsPerTrackOffset), (ushort)sectorsPerTrack);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.HeadCountOffset), (ushort)heads);
        var blocks = Enumerable.Range(0, blockCount).Select(index => new SectorBlock(index, new SectorAddress(0, 0, index + 1), index == 0 ? boot : new byte[FatBootSectorLayout.SectorSize])).ToArray();
        return new(formatId, FatBootSectorLayout.SectorSize, 1, 1, blockCount, blocks);
    }

    private static SectorImage MsxImage(int sectors, byte media)
    {
        var heads = sectors == 720 && media == MsxDiskGeometryCatalog.OneDoubleDensityMediaDescriptor ? 1 : sectors == 360 ? 1 : 2;
        var boot = Boot(sectors, heads, 9);
        System.Text.Encoding.ASCII.GetBytes("MSXDOS  ").CopyTo(boot, FatBootSectorLayout.OemOffset);
        boot[FatBootSectorLayout.MediaDescriptorOffset] = media;
        return new("ibm.scan", 512, Math.Max(1, sectors / (heads * 9)), heads, 9, [new SectorBlock(0, new SectorAddress(0, 0, 1), boot)], capacity: sectors * 512L, logicalBlockCount: sectors);
    }

    private static SectorImage IbmImage(int sectors, int heads, int sectorsPerTrack)
    {
        var boot = Boot(sectors, heads, sectorsPerTrack);
        System.Text.Encoding.ASCII.GetBytes("IBM  3.3").CopyTo(boot, FatBootSectorLayout.OemOffset);
        var fat = new byte[512];
        fat[FatBootSectorLayout.FatMediaDescriptorDataOffset] = 0xfd;
        return new("atarist.360", 512, sectors / (heads * sectorsPerTrack), heads, sectorsPerTrack, [new SectorBlock(0, new SectorAddress(0, 0, 1), boot), new SectorBlock(1, new SectorAddress(0, 0, 2), fat)], logicalBlockCount: sectors);
    }

    private static byte[] Boot(int totalSectors, int heads, int sectorsPerTrack)
    {
        var boot = new byte[FatBootSectorLayout.SectorSize];
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.BytesPerSectorOffset), FatBootSectorLayout.SectorSize);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.TotalSectors16Offset), (ushort)totalSectors);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.SectorsPerTrackOffset), (ushort)sectorsPerTrack);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.HeadCountOffset), (ushort)heads);
        return boot;
    }

    private static FileSystemVolume Volume(IReadOnlyList<FileSystemEntry> entries) => new("VOL", FileSystemIds.Fat12, 1, 0, null, null, entries, []);
}
