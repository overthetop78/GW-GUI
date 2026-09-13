using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Conversion;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Interfaces;
using GWGUI.MediaEngine.Representations.Blocks;

namespace GWGUI.Tests.MediaEngine.HardDisk;

public sealed class HardDiskImageConversionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CommonConversionCopiesHddRangesAndPreservesOptionalGeometry(bool withGeometry)
    {
        using var temporary = new TemporaryDirectory();
        var bytes = Enumerable.Range(0, 192 * 1024).Select(index => (byte)(index * 17)).ToArray();
        var source = new RecordingData(bytes);
        var geometry = withGeometry ? new HardDiskGeometry(6, 2, 32, 512) : null;
        var document = Document(source, geometry);
        var output = Path.Combine(temporary.Path, "converted.img");
        var service = MediaEngineComposition.CreateDefault().ConversionService;

        var result = await service.ConvertAsync(new MediaConversionRequest(document, output, HardDiskImageFormatIds.Raw));

        Assert.Equal([output], result.ProducedFiles);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(output));
        Assert.True(source.Reads >= 3);
        Assert.Same(geometry, Assert.IsType<BlockMediaImageRepresentation>(document.Representation).Geometry);
    }

    [Fact]
    public async Task ExistingDestinationIsReplacedOnlyAfterTheCompleteConversion()
    {
        using var temporary = new TemporaryDirectory();
        var output = Path.Combine(temporary.Path, "converted.img");
        await File.WriteAllBytesAsync(output, [99, 98, 97]);
        var bytes = new byte[64 * 1024];
        Array.Fill(bytes, (byte)42);

        var service = MediaEngineComposition.CreateDefault().ConversionService;
        await service.ConvertAsync(new MediaConversionRequest(
            Document(new RecordingData(bytes), null), output, HardDiskImageFormatIds.Raw));

        Assert.Equal(bytes, await File.ReadAllBytesAsync(output));
        Assert.Empty(Directory.EnumerateFiles(temporary.Path, "*.tmp"));
    }

    [Fact]
    public async Task CancellationPreservesTheExistingDestinationAndLeavesNoPartialFile()
    {
        using var temporary = new TemporaryDirectory();
        var output = Path.Combine(temporary.Path, "converted.img");
        var original = new byte[] { 99, 98, 97 };
        await File.WriteAllBytesAsync(output, original);
        var source = new RecordingData(new byte[192 * 1024], failAfterReads: 1);
        var service = MediaEngineComposition.CreateDefault().ConversionService;

        await Assert.ThrowsAsync<OperationCanceledException>(() => service.ConvertAsync(new MediaConversionRequest(
            Document(source, null), output, HardDiskImageFormatIds.Raw)));

        Assert.Equal(original, await File.ReadAllBytesAsync(output));
        Assert.Empty(Directory.EnumerateFiles(temporary.Path, "*.tmp"));
    }

    private static MediaImageDocument Document(IMediaRandomAccessData source, HardDiskGeometry? geometry) =>
        new(
            new MediaSourceDescriptor("simulated.img", [], source.Length, HardDiskImageFormatIds.Raw),
            HardDiskImageFormatIds.Raw,
            MediaKind.HardDisk,
            new BlockMediaImageRepresentation(
                source.Length,
                512,
                [new MediaDataRange(0, source.Length, MediaDataRangeKind.Stored, source)],
                geometry),
            [],
            [],
            new Dictionary<string, string>(StringComparer.Ordinal));

    private sealed class RecordingData(byte[] bytes, int? failAfterReads = null) : IMediaRandomAccessData
    {
        public long Length => bytes.LongLength;
        public int Reads { get; private set; }

        public ValueTask ReadExactlyAsync(long offset, Memory<byte> destination, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (failAfterReads is { } limit && Reads >= limit) throw new OperationCanceledException(cancellationToken);
            bytes.AsMemory(checked((int)offset), destination.Length).CopyTo(destination);
            Reads++;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        internal TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "gwgui-hdd-convert-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
        }
    }
}
