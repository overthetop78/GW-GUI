using System.IO;
using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.Containers.Commodore;
using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages.Builders;

namespace GWGUI.Tests;

public sealed class CommodoreDiskImageTests
{
    [Theory]
    [InlineData(174848, 35, 683)]
    [InlineData(196608, 40, 768)]
    public async Task D64ReaderSupportsStandardGeometries(int length, int tracks, int blocks)
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".d64");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[length]);
            var image = await new D64Reader().ReadAsync(path);
            Assert.Equal("commodore.1541", image.FormatId);
            Assert.Equal(tracks, image.Cylinders);
            Assert.Equal(blocks, image.BlockCount);
            Assert.Equal(length, image.Capacity);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(174848, 35, false)]
    [InlineData(175531, 35, true)]
    [InlineData(196608, 40, false)]
    [InlineData(197376, 40, true)]
    public async Task D64ReaderSupportsEveryLayoutAndPreservesErrorCodes(int length, int tracks, bool hasErrorMap)
    {
        var layout = D64Layout.Supported.Single(candidate => candidate.ImageLength == length);
        var data = new byte[length];
        if (hasErrorMap)
        {
            data.AsSpan(layout.ErrorMapOffset!.Value, layout.DataBlockCount).Fill((byte)CommodoreDiskErrorCode.None);
            data[layout.ErrorMapOffset.Value] = (byte)CommodoreDiskErrorCode.DataChecksumError;
        }
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".d64");
        try
        {
            await File.WriteAllBytesAsync(path, data);
            var image = await new D64Reader().ReadAsync(path);
            Assert.Equal(tracks, image.Cylinders);
            Assert.Equal(layout.DataBlockCount * Commodore1541Geometry.SectorSize, image.Capacity);
            Assert.True(image.TryGetBlock(0, out var first));
            Assert.True(image.TryGetBlock(image.BlockCount - 1, out var last));
            Assert.Equal(new(0, 0, 0), first.Address);
            Assert.Equal(Commodore1541Geometry.ToCylinder(tracks), last.Address.Cylinder);
            Assert.Equal(hasErrorMap ? (byte)CommodoreDiskErrorCode.DataChecksumError : null, first.DiagnosticCode);
            Assert.Equal(!hasErrorMap, first.IntegrityValid);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(4, false)]
    [InlineData(5, false)]
    [InlineData(6, false)]
    [InlineData(7, false)]
    [InlineData(8, false)]
    [InlineData(9, false)]
    [InlineData(10, false)]
    [InlineData(11, false)]
    [InlineData(15, false)]
    public async Task D64ReaderInterpretsDocumentedErrorCodes(byte code, bool integrity)
    {
        var layout = D64Layout.Tracks35WithErrors;
        var data = new byte[layout.ImageLength];
        data.AsSpan(layout.ErrorMapOffset!.Value, layout.DataBlockCount).Fill((byte)CommodoreDiskErrorCode.None);
        data[layout.ErrorMapOffset.Value] = code;
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".d64");
        try
        {
            await File.WriteAllBytesAsync(path, data);
            var image = await new D64Reader().ReadAsync(path);
            Assert.True(image.TryGetBlock(0, out var block));
            Assert.Equal(code, block.DiagnosticCode);
            Assert.Equal(integrity, block.IntegrityValid);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task D64ReaderRejectsUnknownLengthAndHonorsCancellation()
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".d64");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[D64Layout.Tracks35.ImageLength - 1]);
            await Assert.ThrowsAsync<InvalidDataException>(() => new D64Reader().ReadAsync(path));
            await File.WriteAllBytesAsync(path, new byte[D64Layout.Tracks35.ImageLength]);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new D64Reader().ReadAsync(path, cancellation.Token));
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(1, 21)]
    [InlineData(17, 21)]
    [InlineData(18, 19)]
    [InlineData(24, 19)]
    [InlineData(25, 18)]
    [InlineData(30, 18)]
    [InlineData(31, 17)]
    [InlineData(40, 17)]
    public void Commodore1541GeometryDefinesEveryZoneBoundary(int track, int sectors) => Assert.Equal(sectors, Commodore1541Geometry.SectorsPerTrack(track));

    [Fact]
    public void D64LayoutsHaveExactLengthsAndRejectATruncatedErrorMap()
    {
        Assert.Equal(new[] { 174_848, 175_531, 196_608, 197_376 }, D64Layout.Supported.Select(layout => layout.ImageLength));
        var layout = D64Layout.Tracks35WithErrors;
        var truncated = new byte[layout.ImageLength - 1];
        Assert.Throws<InvalidDataException>(() => Commodore1541SectorImageBuilder.Create(truncated, "commodore.1541", layout.TrackCount, 1, layout.DataBlockCount, layout.ErrorMapOffset, D64Exceptions.InvalidErrorMap, CancellationToken.None));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => Commodore1541SectorImageBuilder.Create(new byte[D64Layout.Tracks35.ImageLength], "commodore.1541", D64Layout.Tracks35.TrackCount, 1, D64Layout.Tracks35.DataBlockCount, null, D64Exceptions.InvalidErrorMap, cancellation.Token));
    }

    [Fact]
    public async Task D81ReaderBuildsCbmLogicalSectors()
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".d81");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[CommodoreD81ImageReader.ImageBytes]);
            var image = await new CommodoreD81ImageReader().ReadAsync(path);
            Assert.Equal("commodore.1581", image.FormatId);
            Assert.Equal(3_200, image.BlockCount);
            Assert.Equal(256, image.BlockSize);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(349696, 35, 1366)]
    [InlineData(393216, 40, 1536)]
    public async Task D71ReaderSupportsBothSidesAndExtendedGeometries(int length, int tracks, int blocks)
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".d71");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[length]);
            var image = await new CommodoreD71ImageReader().ReadAsync(path);
            Assert.Equal("commodore.1571", image.FormatId);
            Assert.Equal(tracks, image.Cylinders);
            Assert.Equal(2, image.Heads);
            Assert.Equal(blocks, image.BlockCount);
            Assert.Equal(length, image.Capacity);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task RealCommodoreCorpusCanBeOpened()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_COMMODORE_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(path => new[] { ".d64", ".d71", ".d81" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.NotEmpty(files);
        var explorer = DiskImageExplorer.CreateDefault();
        foreach (var file in files)
        {
            var explored = await explorer.ExploreAsync(file);
            Assert.True(explored.FileSystemRecognized, file);
            Assert.False(string.IsNullOrWhiteSpace(explored.Volume.FileSystem), file);
            Assert.True(explored.Volume.Entries.Count > 0, file);
            var expected = Path.GetFileName(file).Contains("cpm", StringComparison.OrdinalIgnoreCase) ? "CP/M 3" : "CBM DOS";
            Assert.Equal(expected, explored.Volume.FileSystem);
        }
    }

    [Fact]
    public async Task RealCommodoreScpCorpusCanBeReconstructed()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_COMMODORE_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var generatedRoot = Path.Combine(root, "_generated");
        if (!Directory.Exists(generatedRoot)) return;
        var originals = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(path => !path.StartsWith(generatedRoot, StringComparison.OrdinalIgnoreCase))
            .Where(path => new[] { ".d64", ".d71", ".d81" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .GroupBy(path => (Directory: Path.GetRelativePath(root, Path.GetDirectoryName(path)!), Name: Path.GetFileNameWithoutExtension(path)))
            .Select(group => group.OrderByDescending(path => Path.GetExtension(path).Equals(".d81", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(path => Path.GetExtension(path).Equals(".d71", StringComparison.OrdinalIgnoreCase)).First()).ToArray();
        var explorer = DiskImageExplorer.CreateDefault();
        foreach (var original in originals)
        {
            var relativeDirectory = Path.GetRelativePath(root, Path.GetDirectoryName(original)!);
            var generated = Path.Combine(generatedRoot, relativeDirectory, Path.GetFileNameWithoutExtension(original) + " [test].scp");
            if (!File.Exists(generated)) continue;
            var formatId = Path.GetExtension(original).ToLowerInvariant() switch { ".d71" => "commodore.1571", ".d81" => "commodore.1581", _ => "commodore.1541" };
            ExploredDiskImage explored;
            try { explored = await explorer.ExploreAsync(generated, formatId); }
            catch (Exception exception) { throw new InvalidDataException(generated, exception); }
            var decodedTracks = string.Join(", ", explored.Image.AvailableBlocks
                .GroupBy(block => block.Address.Cylinder)
                .OrderBy(group => group.Key)
                .Select(group => $"T{group.Key + 1}:{group.Count()}"));
            Assert.True(explored.FileSystemRecognized, $"{generated}; format={explored.Image.FormatId}; blocks={explored.Image.AvailableBlocks.Count}; missing={explored.Image.MissingBlocks.Count}; {decodedTracks}");
            Assert.True(explored.Volume.Entries.Count > 0, generated);
            var automatic = await explorer.ExploreAsync(generated);
            Assert.True(automatic.FileSystemRecognized, $"Automatic detection failed for {generated}");
            Assert.StartsWith("commodore.", automatic.Image.FormatId);
        }
    }
}
