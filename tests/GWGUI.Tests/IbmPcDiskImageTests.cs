using GWGUI.MediaEngine.Exploration;
using System.IO;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Containers.Ibm.Raw;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Geometries.Ibm;
using GWGUI.MediaEngine.Recognition.Ibm;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.Tests;

public sealed class IbmPcDiskImageTests
{
    [Theory]
    [InlineData(160 * 1024, "ibm.160", 40, 1, 8)]
    [InlineData(180 * 1024, "ibm.180", 40, 1, 9)]
    [InlineData(320 * 1024, "ibm.320", 40, 2, 8)]
    [InlineData(360 * 1024, "ibm.360", 40, 2, 9)]
    [InlineData(720 * 1024, "ibm.720", 80, 2, 9)]
    [InlineData(800 * 1024, "ibm.800", 80, 2, 10)]
    [InlineData(1200 * 1024, "ibm.1200", 80, 2, 15)]
    [InlineData(1440 * 1024, "ibm.1440", 80, 2, 18)]
    [InlineData(1680 * 1024, "ibm.1680", 80, 2, 21)]
    [InlineData(2880 * 1024, "ibm.2880", 80, 2, 36)]
    public async Task RawReaderSupportsStandardGeometries(int length, string format, int cylinders, int heads, int sectors)
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".ima");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[length]);
            var image = await new IbmRawImageReader().ReadAsync(path);
            Assert.Equal(format, image.FormatId);
            Assert.Equal(cylinders, image.Cylinders);
            Assert.Equal(heads, image.Heads);
            Assert.Equal(sectors, image.SectorsPerTrack);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(0xfe, "ibm.160")]
    [InlineData(0xfc, "ibm.180")]
    [InlineData(0xff, "ibm.320")]
    [InlineData(0xfd, "ibm.360")]
    public void HistoricalFatDescriptorsResolveThroughTheCatalog(byte descriptor, string formatId)
    {
        Assert.True(IbmPcGeometryCatalog.TryFromMediaDescriptor(descriptor, out var geometry));
        Assert.Equal(formatId, geometry.FormatId);
    }

    [Theory]
    [InlineData("IBM  3.3", true)]
    [InlineData("MSDOS5.0", true)]
    [InlineData("MSWIN4.1", true)]
    [InlineData("DOS     ", true)]
    [InlineData("FRDOS5.1", true)]
    [InlineData("FREEDOS ", true)]
    [InlineData("COPYDISK", true)]
    [InlineData("XXDOSXXX", false)]
    [InlineData("UNKNOWN ", false)]
    public void DosOemProbeOnlyAcceptsDocumentedPrefixes(string oem, bool expected)
    {
        var boot = new byte[FatBootSectorLayout.MinimumLength];
        System.Text.Encoding.ASCII.GetBytes(oem).CopyTo(boot, FatBootSectorLayout.OemOffset);
        Assert.Equal(expected, IbmDosOemProbe.IsKnownDosOem(boot));
    }

    [Fact]
    public void FatBpbDetectorSupportsBothTotalsAndValidatesImageLengthAndLimits()
    {
        var boot = CreateBpb(720, 9, 2, useLargeTotal: false);
        Assert.True(FatBpbGeometryDetector.TryDetect(boot, 720 * FatBootSectorLayout.SectorSize, out var small));
        Assert.Equal(40, small.Cylinders);
        Assert.False(FatBpbGeometryDetector.TryDetect(boot, 721 * FatBootSectorLayout.SectorSize, out _));
        var large = CreateBpb(2_880, 18, 2, useLargeTotal: true);
        Assert.True(FatBpbGeometryDetector.TryDetect(large, null, out var detectedLarge));
        Assert.Equal(80, detectedLarge.Cylinders);
        foreach (var invalid in new[] { CreateBpb(720, 0, 2), CreateBpb(720, 64, 2), CreateBpb(720, 9, 0), CreateBpb(720, 9, 3), CreateBpb(2_304, 9, 1) }) Assert.False(FatBpbGeometryDetector.TryDetect(invalid, null, out _));
    }

    [Fact]
    public void RawGeometryAndBuilderValidateInputOffsetsCancellationAndImmutability()
    {
        Assert.Throws<InvalidDataException>(() => IbmRawImageGeometryDetector.Detect([]));
        Assert.Throws<InvalidDataException>(() => IbmRawImageGeometryDetector.Detect(new byte[FatBootSectorLayout.SectorSize + 1]));
        Assert.Throws<InvalidDataException>(() => IbmRawImageGeometryDetector.Detect(new byte[100 * FatBootSectorLayout.SectorSize]));
        var geometry = IbmPcGeometryCatalog.ByCapacity[160 * 1024];
        var data = new byte[160 * 1024];
        data[FatBootSectorLayout.SectorSize] = 0x5a;
        var image = IbmRawSectorImageBuilder.Create(data, geometry);
        Assert.True(image.TryGetBlock(1, out var block));
        Assert.Equal(new(0, 0, 2), block.Address);
        Assert.Equal(0x5a, block.Data[0]);
        Assert.IsAssignableFrom<IReadOnlyDictionary<int, IbmPcGeometry>>(IbmPcGeometryCatalog.ByCapacity);
        Assert.Throws<NotSupportedException>(() => ((IDictionary<int, IbmPcGeometry>)IbmPcGeometryCatalog.ByCapacity).Add(1, geometry));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => IbmRawSectorImageBuilder.Create(data, geometry, cancellation.Token));
    }

    /// <summary>Crée un BPB FAT minimal avec total sur 16 ou 32 bits.</summary>
    private static byte[] CreateBpb(int totalSectors, int sectorsPerTrack, int heads, bool useLargeTotal = false)
    {
        var boot = new byte[FatBootSectorLayout.MinimumLength];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.BytesPerSectorOffset), FatBootSectorLayout.SectorSize);
        if (useLargeTotal) System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(boot.AsSpan(FatBootSectorLayout.TotalSectors32Offset), checked((uint)totalSectors));
        else System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.TotalSectors16Offset), checked((ushort)totalSectors));
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.SectorsPerTrackOffset), checked((ushort)sectorsPerTrack));
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.HeadCountOffset), checked((ushort)heads));
        return boot;
    }

    [Fact]
    public async Task RawReaderRejectsANonAlignedStandardCapacity()
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".ima");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[720 * 1024 + 1]);
            await Assert.ThrowsAsync<InvalidDataException>(() => new IbmRawImageReader().ReadAsync(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task RealIbmPcRawCorpusCanBeOpened()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_IBM_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var directory = Path.Combine(root, "IBM PC");
        if (!Directory.Exists(directory)) return;
        var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
            .Where(path => new[] { ".img", ".ima" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.NotEmpty(files);
        var explorer = DiskImageExplorer.CreateDefault();
        foreach (var file in files)
        {
            var explored = await explorer.ExploreAsync(file);
            Assert.True(explored.FileSystemRecognized,
                $"{file}; format={explored.Image.FormatId}; geometry={explored.Image.Cylinders}x{explored.Image.Heads}x{explored.Image.SectorsPerTrack}; blocks={explored.Image.AvailableBlocks.Count}; missing={explored.Image.MissingBlocks.Count}");
            Assert.Equal(GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.Fat12, explored.Volume.FileSystemId);
            Assert.StartsWith("ibm.", explored.Image.FormatId);
            var automatic = await explorer.ExploreAsync(file);
            Assert.True(automatic.FileSystemRecognized, $"Automatic detection failed for {file}");
            Assert.True(automatic.Volume.FileSystemId == GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.Fat12, $"{file}; automatic file system={automatic.Volume.FileSystemId}; format={automatic.Image.FormatId}");
            Assert.StartsWith("ibm.", automatic.Image.FormatId);
        }
    }

    [Fact]
    public async Task RealIbmPcScpCorpusCanBeReconstructed()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_IBM_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var directory = Path.Combine(root, "IBM PC");
        if (!Directory.Exists(directory)) return;
        var files = Directory.EnumerateFiles(directory, "*.scp", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.NotEmpty(files);
        var explorer = DiskImageExplorer.CreateDefault();
        foreach (var file in files)
        {
            var explored = await explorer.ExploreAsync(file, "ibm.scan");
            Assert.True(explored.FileSystemRecognized,
                $"{file}; format={explored.Image.FormatId}; geometry={explored.Image.Cylinders}x{explored.Image.Heads}x{explored.Image.SectorsPerTrack}; blocks={explored.Image.AvailableBlocks.Count}; missing={explored.Image.MissingBlocks.Count}");
            Assert.Equal(GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.Fat12, explored.Volume.FileSystemId);
            Assert.StartsWith("ibm.", explored.Image.FormatId);
            var automatic = await explorer.ExploreAsync(file);
            Assert.True(automatic.FileSystemRecognized, $"Automatic detection failed for {file}");
            Assert.True(automatic.Volume.FileSystemId == GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.Fat12, $"{file}; automatic file system={automatic.Volume.FileSystemId}; format={automatic.Image.FormatId}");
            Assert.StartsWith("ibm.", automatic.Image.FormatId);
        }
    }
}
