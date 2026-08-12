using GWGUI.MediaEngine.Exploration;
using GWGUI.MediaEngine.Exploration.Results;
using System.IO;
using GWGUI.MediaEngine.FileSystems.Commodore.Dos;
using GWGUI.MediaEngine.Containers.Commodore;
using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Containers.Commodore.D71;
using GWGUI.MediaEngine.Containers.Commodore.D81;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;
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
    public void CommodoreGeometriesValidateBoundsAndRoundTripZoneBoundaries()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Commodore1541Geometry.SectorsPerTrack(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Commodore1541Geometry.SectorsPerTrack(41));
        foreach (var track in new[] { 1, 17, 18, 24, 25, 30, 31, 40 })
        {
            var sector = Commodore1541Geometry.SectorsPerTrack(track) - 1;
            var logical = Commodore1541Geometry.ToSideLogicalBlock(track, sector, Commodore1541Geometry.ExtendedTrackCount);
            Assert.Equal(new Commodore1541Address(track, sector, 0), Commodore1541Geometry.FromLogicalBlock(logical, Commodore1541Geometry.ExtendedTrackCount, 1));
        }
        foreach (var side in new[] { 0, 1 })
        {
            var logical = Commodore1571Geometry.ToLogicalBlock(40, Commodore1541Geometry.SectorsPerTrack(40) - 1, 40, side);
            Assert.Equal(new Commodore1541Address(40, 16, side), Commodore1571Geometry.FromLogicalBlock(logical, 40));
        }
        Assert.Throws<ArgumentOutOfRangeException>(() => Commodore1571Geometry.ToLogicalBlock(1, 0, 35, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => Commodore1571Geometry.ToLogicalBlock(1, 0, 35, 2));
        foreach (var logical in new[] { 0, 39, 40, 3_199 })
        {
            var address = Commodore1581Geometry.FromLogicalBlock(logical);
            Assert.Equal(logical, Commodore1581Geometry.ToLogicalBlock(address.Track, address.Sector));
        }
        Assert.Throws<ArgumentOutOfRangeException>(() => Commodore1581Geometry.ToLogicalBlock(0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Commodore1581Geometry.ToLogicalBlock(1, 40));
    }

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
            await File.WriteAllBytesAsync(path, new byte[D81Layout.ImageLength]);
            var image = await new D81Reader().ReadAsync(path);
            Assert.Equal("commodore.1581", image.FormatId);
            Assert.Equal(3_200, image.BlockCount);
            Assert.Equal(256, image.BlockSize);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task D81ReaderValidatesLengthAndLogicalAddresses()
    {
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".d81");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[D81Layout.ImageLength]);
            var image = await new D81Reader().ReadAsync(path);
            foreach (var expected in new[] { (Block: 0, Address: new SectorAddress(0, 0, 0)), (Block: 39, Address: new SectorAddress(0, 0, 39)), (Block: 40, Address: new SectorAddress(1, 0, 0)), (Block: 3_199, Address: new SectorAddress(79, 0, 39)) })
            {
                Assert.True(image.TryGetBlock(expected.Block, out var block));
                Assert.Equal(expected.Address, block.Address);
            }
            Assert.Equal(D81Layout.ImageLength, image.Capacity);
            await File.WriteAllBytesAsync(path, new byte[D81Layout.ImageLength - 1]);
            await Assert.ThrowsAsync<InvalidDataException>(() => new D81Reader().ReadAsync(path));
            await File.WriteAllBytesAsync(path, new byte[D81Layout.ImageLength + 1]);
            await Assert.ThrowsAsync<InvalidDataException>(() => new D81Reader().ReadAsync(path));
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(0, 1, 1, 0)]
    [InlineData(0, 1, 10, 18)]
    [InlineData(0, 0, 1, 20)]
    [InlineData(1, 1, 1, 40)]
    public void Commodore1581PhysicalSectorsMapToTwoLogicalBlocks(int cylinder, int head, int sector, int firstLogicalBlock)
    {
        Assert.Equal(firstLogicalBlock, Commodore1581Geometry.PhysicalSectorToLogicalBlock(cylinder, head, sector));
        Assert.Equal(firstLogicalBlock, Commodore1581Geometry.ToLogicalBlock(firstLogicalBlock / Commodore1581Geometry.LogicalBlocksPerTrack + 1, firstLogicalBlock % Commodore1581Geometry.LogicalBlocksPerTrack));
        Assert.Equal(firstLogicalBlock + 1, Commodore1581Geometry.PhysicalSectorToLogicalBlock(cylinder, head, sector) + 1);
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
            var image = await new D71Reader().ReadAsync(path);
            Assert.Equal("commodore.1571", image.FormatId);
            Assert.Equal(tracks, image.Cylinders);
            Assert.Equal(2, image.Heads);
            Assert.Equal(blocks, image.BlockCount);
            Assert.Equal(length, image.Capacity);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(349696, 35, false)]
    [InlineData(351062, 35, true)]
    [InlineData(393216, 40, false)]
    [InlineData(394752, 40, true)]
    public async Task D71ReaderSupportsEveryLayoutAndKeepsBothFacesInOrder(int length, int tracks, bool hasErrorMap)
    {
        var layout = D71Layout.Supported.Single(candidate => candidate.ImageLength == length);
        var data = new byte[length];
        if (hasErrorMap)
        {
            data.AsSpan(layout.ErrorMapOffset!.Value, layout.DataBlockCount).Fill((byte)CommodoreDiskErrorCode.None);
            data[layout.ErrorMapOffset.Value] = (byte)CommodoreDiskErrorCode.HeaderChecksumError;
        }
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".d71");
        try
        {
            await File.WriteAllBytesAsync(path, data);
            var image = await new D71Reader().ReadAsync(path);
            var blocksPerSide = Commodore1541Geometry.BlocksPerSide(tracks);
            Assert.Equal(layout.DataBlockCount * Commodore1541Geometry.SectorSize, image.Capacity);
            Assert.True(image.TryGetBlock(blocksPerSide - 1, out var lastFirstSide));
            Assert.True(image.TryGetBlock(blocksPerSide, out var firstSecondSide));
            Assert.Equal(new(tracks - 1, 0, Commodore1541Geometry.SectorsPerTrack(tracks) - 1), lastFirstSide.Address);
            Assert.Equal(new(0, 1, 0), firstSecondSide.Address);
            Assert.Equal(hasErrorMap ? (byte)CommodoreDiskErrorCode.None : null, firstSecondSide.DiagnosticCode);
            Assert.True(image.TryGetBlock(0, out var first));
            Assert.Equal(hasErrorMap ? (byte)CommodoreDiskErrorCode.HeaderChecksumError : null, first.DiagnosticCode);
            Assert.Equal(!hasErrorMap, first.IntegrityValid);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task D71MatchesD64AddressesAndRejectsInvalidInputs()
    {
        var d64Path = Path.ChangeExtension(Path.GetTempFileName(), ".d64");
        var d71Path = Path.ChangeExtension(Path.GetTempFileName(), ".d71");
        try
        {
            await File.WriteAllBytesAsync(d64Path, new byte[D64Layout.Tracks35.ImageLength]);
            await File.WriteAllBytesAsync(d71Path, new byte[D71Layout.Tracks35.ImageLength]);
            var d64 = await new D64Reader().ReadAsync(d64Path);
            var d71 = await new D71Reader().ReadAsync(d71Path);
            Assert.Equal(d64.AvailableBlocks.Select(block => block.Address), d71.AvailableBlocks.Take(d64.BlockCount).Select(block => block.Address));
            await File.WriteAllBytesAsync(d71Path, new byte[D71Layout.Tracks35.ImageLength - 1]);
            await Assert.ThrowsAsync<InvalidDataException>(() => new D71Reader().ReadAsync(d71Path));
        }
        finally { File.Delete(d64Path); File.Delete(d71Path); }

        var layout = D71Layout.Tracks35WithErrors;
        Assert.Throws<InvalidDataException>(() => Commodore1541SectorImageBuilder.Create(new byte[layout.ImageLength - 1], "commodore.1571", layout.TracksPerSide, 2, layout.DataBlockCount, layout.ErrorMapOffset, D71Exceptions.InvalidErrorMap, CancellationToken.None));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() => Commodore1541SectorImageBuilder.Create(new byte[D71Layout.Tracks35.ImageLength], "commodore.1571", D71Layout.Tracks35.TracksPerSide, 2, D71Layout.Tracks35.DataBlockCount, null, D71Exceptions.InvalidErrorMap, cancellation.Token));
    }

    [Fact]
    public async Task RealCommodoreCorpusCanBeOpened()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_COMMODORE_CORPUS");
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) root = FindImageTestRoot();
        var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(path => new[] { ".d64", ".d71", ".d81" }.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToArray();
        Assert.NotEmpty(files);
        var explorer = DiskImageExplorer.CreateDefault();
        foreach (var file in files)
        {
            var explored = await explorer.ExploreAsync(file);
            Assert.True(explored.FileSystemRecognized, file);
            Assert.False(string.IsNullOrWhiteSpace(explored.Volume.FileSystemId), file);
            Assert.True(explored.Volume.Entries.Count > 0, file);
            var expected = Path.GetFileName(file).Contains("cpm", StringComparison.OrdinalIgnoreCase) ? GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.Cpm : GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.CommodoreDos;
            Assert.Equal(expected, explored.Volume.FileSystemId);
            Assert.InRange(explored.Volume.FreeBytes, 0, explored.Volume.Capacity);
            if (expected == GWGUI.MediaEngine.FileSystems.Definitions.FileSystemIds.CommodoreDos)
            {
                Assert.All(explored.Volume.Entries, entry =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(entry.Name));
                    Assert.NotNull(entry.Content);
                    Assert.InRange(entry.RawAttributes & (uint)CommodoreDosFileType.BaseTypeMask, 1u, (uint)CommodoreDosFileType.Cbm);
                    Assert.True(entry.StorageReference >= 0);
                });
            }
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

    /// <summary>Recherche le corpus local non versionné à partir du dossier d'exécution des tests.</summary>
    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }
}
