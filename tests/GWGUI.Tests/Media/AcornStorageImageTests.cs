using System.Buffers.Binary;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Reading.Sources;
using GWGUI.MediaEngine.Images.Visualization.Providers;
using GWGUI.App.Contracts.Rendering.Blocks;
using GWGUI.App.Presenters.Visualization;
using GWGUI.App.Rendering.Blocks;
using SkiaSharp;

namespace GWGUI.Tests.Media;

/// <summary>Contrôles autonomes des répertoires BBC, identiques dans une disquette et un Winchester.</summary>
public sealed class AcornStorageImageTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.AcornAdfs160)]
    [InlineData(DiskImageFormatIds.AcornAdfs320)]
    [InlineData(DiskImageFormatIds.AcornAdfs640)]
    public async Task BlankBbcAdfsHasAUsableMapAndEmptyRoot(string format)
    {
        var image = GWGUI.MediaEngine.Images.Creation.BlankSectorImageFactory.Create(format);
        var document = MediaImageDocument.CreateUnexplored(new("blank.adf", []), format, MediaKind.Floppy, new SectorMediaImageRepresentation(image), [], new Dictionary<string, string>());
        var explored = await MediaEngineComposition.CreateDefault().Explorer.ExploreAsync(document);
        var volume = Assert.Single(explored.Volumes).FileSystem!;
        Assert.NotNull(volume); Assert.Empty(volume.Entries); Assert.Empty(volume.Warnings);
        Assert.Equal(image.Capacity - 1792, volume.FreeBytes);
        for (var index = 0; index < 2; index++)
        {
            Assert.True(image.TryGetBlock(index, out var block)); var sum = 0;
            for (var offset = 254; offset >= 0; offset--) sum = (sum & 255) + (sum >> 8) + block.Data[offset];
            Assert.Equal(sum & 255, block.Data[255]);
        }
    }

    [Theory]
    [InlineData(15)]
    [InlineData(1279)]
    public async Task AdfsLFileAddressesCrossTracksAndSidesInLogicalOrder(int start)
    {
        var blank = GWGUI.MediaEngine.Images.Creation.BlankSectorImageFactory.Create(DiskImageFormatIds.AcornAdfs640);
        var blocks = blank.AvailableBlocks.ToDictionary(block => block.LogicalBlock, block => block.Data.ToArray());
        var directory = blocks[2]; "CROSS"u8.CopyTo(directory.AsSpan(5));
        BinaryPrimitives.WriteUInt32LittleEndian(directory.AsSpan(23), 512);
        directory[27] = (byte)start; directory[28] = (byte)(start >> 8);
        for (var offset = 0; offset < 2; offset++)
        {
            var logical = start + offset; var side = logical / 1280; var track = logical % 1280 / 16;
            blocks[(track * 2 + side) * 16 + logical % 16].AsSpan().Fill((byte)(0x61 + offset));
        }
        var image = new SectorImage(blank.FormatId, 256, 80, 2, 16, blank.AvailableBlocks.Select(block => block with { Data = blocks[block.LogicalBlock] }));
        var document = MediaImageDocument.CreateUnexplored(new("cross.adl", []), image.FormatId, MediaKind.Floppy, new SectorMediaImageRepresentation(image), [], new Dictionary<string, string>());
        var explored = await MediaEngineComposition.CreateDefault().Explorer.ExploreAsync(document);
        var file = Assert.Single(Assert.Single(explored.Volumes).FileSystem!.Entries);
        Assert.Equal("CROSS", file.Name); Assert.True(file.MetadataValid);
        Assert.Equal(Enumerable.Repeat((byte)0x61, 256).Concat(Enumerable.Repeat((byte)0x62, 256)), file.Content);
    }

    [Theory]
    [InlineData(DiskImageFormatIds.AcornDfsDoubleSided)]
    [InlineData(DiskImageFormatIds.AcornDfsDoubleSided80)]
    public async Task DoubleSidedDfsKeepsIdenticallyNamedFilesOnTheirOwnSide(string format)
    {
        var blank = GWGUI.MediaEngine.Images.Creation.BlankSectorImageFactory.Create(format);
        var blocks = blank.AvailableBlocks.ToDictionary(block => block.LogicalBlock, block => block.Data.ToArray());
        for (var head = 0; head < 2; head++)
        {
            "TEST   $"u8.CopyTo(blocks[head * 10].AsSpan(8));
            var metadata = blocks[head * 10 + 1]; metadata[5] = 8; metadata[12] = 1; metadata[15] = 12;
            blocks[20 + head * 10 + 2][0] = (byte)(65 + head);
        }
        var image = new SectorImage(format, 256, blank.Cylinders, 2, 10, blank.AvailableBlocks.Select(block => block with { Data = blocks[block.LogicalBlock] }));
        var document = MediaImageDocument.CreateUnexplored(new("memory.dsd", []), format, MediaKind.Floppy, new SectorMediaImageRepresentation(image), [], new Dictionary<string, string>());
        var result = await MediaEngineComposition.CreateDefault().Explorer.ExploreAsync(document);
        var volume = Assert.Single(result.Volumes).FileSystem!;
        Assert.Equal(blank.Capacity, volume.Capacity);
        Assert.Empty(volume.Warnings);
        for (var head = 0; head < 2; head++)
        {
            var file = Assert.Single(volume.Entries[head].Children);
            Assert.Equal("TEST", file.Name); Assert.Equal(new[] { (byte)(65 + head) }, file.Content);
            Assert.True(file.MetadataValid);
        }
    }

    [Theory]
    [InlineData(DiskImageFormatIds.AcornDfsSingleSided, 40, 1)]
    [InlineData(DiskImageFormatIds.AcornDfsSingleSided80, 80, 1)]
    [InlineData(DiskImageFormatIds.AcornDfsDoubleSided, 40, 2)]
    [InlineData(DiskImageFormatIds.AcornDfsDoubleSided80, 80, 2)]
    public async Task BlankDfsHasAnEmptyCatalogueOnEachPhysicalSide(string format, int cylinders, int heads)
    {
        var image = GWGUI.MediaEngine.Images.Creation.BlankSectorImageFactory.Create(format);
        Assert.Equal(cylinders, image.Cylinders); Assert.Equal(heads, image.Heads);
        Assert.Equal(cylinders * heads * 10 * 256, image.Capacity);
        for (var head = 0; head < heads; head++)
        {
            Assert.True(image.TryGetBlock(head * 10 + 1, out var catalogue));
            Assert.Equal(0, catalogue.Data[5]);
            Assert.Equal(cylinders * 10, (catalogue.Data[6] & 3) * 256 + catalogue.Data[7]);
        }
        var document = MediaImageDocument.CreateUnexplored(new("memory" + (heads == 1 ? ".ssd" : ".dsd"), []), format, MediaKind.Floppy, new SectorMediaImageRepresentation(image), [], new Dictionary<string, string>());
        var result = await MediaEngineComposition.CreateDefault().Explorer.ExploreAsync(document);
        var volume = Assert.Single(result.Volumes).FileSystem!;
        Assert.NotNull(volume);
        if (heads == 1) Assert.Empty(volume.Entries);
        else
        {
            Assert.Equal(new[] { "0", "2" }, volume.Entries.Select(entry => entry.Name));
            Assert.All(volume.Entries, entry => { Assert.True(entry.MetadataValid); Assert.Empty(entry.Children); });
        }
    }

    [Theory]
    [InlineData(DiskImageFormatIds.AcornAdfs160, 40, 1, 160 * 1024)]
    [InlineData(DiskImageFormatIds.AcornAdfs320, 80, 1, 320 * 1024)]
    [InlineData(DiskImageFormatIds.AcornAdfs640, 80, 2, 640 * 1024)]
    public void BbcAdfsGeometriesRetainTheirSectorSizeAndPhysicalSides(string format, int cylinders, int heads, int capacity)
    {
        var geometry = Assert.Single(GWGUI.MediaEngine.Images.Formats.Floppy.Adf.AcornAdfGeometry.BbcGeometries, candidate => candidate.FormatId == format);
        Assert.Equal(cylinders, geometry.Cylinders); Assert.Equal(heads, geometry.Heads); Assert.Equal(capacity, geometry.Capacity);
        Assert.Equal(256, geometry.BlockSize); Assert.Equal(16, geometry.SectorsPerTrack);
    }

    [Fact]
    public async Task WinchesterScanCoversEveryTrackAndSurfaceWithLbaPositions()
    {
        var bytes = Image();
        var document = MediaImageDocument.CreateUnexplored(new("memory.dat", []), HardDiskImageFormatIds.AcornWinchester, MediaKind.HardDisk,
            new BlockMediaImageRepresentation(bytes.Length, 256, [new(0, bytes.Length, MediaDataRangeKind.Stored, new MemoryRandomAccessData(bytes))], new(2, 4, 33, 256)), [], new Dictionary<string, string>());
        var descriptor = new BlockMediaVisualizationProvider().CreateDescriptor(document);
        Assert.Equal(new[] { 0, 1, 2, 3 }, descriptor.Surfaces);
        Assert.Equal(8, descriptor.Elements.Count);
        Assert.Equal(Enumerable.Range(0, 8).Select(index => (long)index * 33), descriptor.Elements.Select(element => element.Position));
        Assert.All(descriptor.Elements, element => Assert.Equal(33, element.Length));
        foreach (var element in descriptor.Elements) Assert.True(await BlockMediaVisualizationProvider.ReadElementAsync(document, element));
        var model = new BlockMediaInspectorPresenter((key, _) => key).BuildRenderModel(document);
        Assert.Equal(8, model.Ranges.Count);
        Assert.Equal(264, model.Ranges.Sum(range => range.Length));
        var renderer = new SkiaHardDiskGeometryRenderer();
        Assert.Equal(3, renderer.HitTest(model, 800, 800, new(700, 600)));
        Assert.Equal(231, renderer.HitTestPosition(model, 800, 800, new(700, 600)));
        Assert.Null(renderer.HitTest(model, 800, 800, new(600, 600)));
    }

    [Fact]
    public async Task UnavailableTrackIsNotReportedAsReadable()
    {
        const int trackBytes = 33 * 256;
        var document = MediaImageDocument.CreateUnexplored(new("memory.dat", []), HardDiskImageFormatIds.AcornWinchester, MediaKind.HardDisk,
            new BlockMediaImageRepresentation(trackBytes * 2, 256, [new(0, trackBytes, MediaDataRangeKind.Zero), new(trackBytes, trackBytes, MediaDataRangeKind.Unavailable)], new(1, 2, 33, 256)), [], new Dictionary<string, string>());
        var descriptor = new BlockMediaVisualizationProvider().CreateDescriptor(document);
        Assert.True(await BlockMediaVisualizationProvider.ReadElementAsync(document, descriptor.Elements[0]));
        Assert.False(await BlockMediaVisualizationProvider.ReadElementAsync(document, descriptor.Elements[1]));
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => BlockMediaVisualizationProvider.ReadElementAsync(document, descriptor.Elements[0], cancellation.Token));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BbcAdfsExploresDirectoriesAndFilesWith256ByteSectors(bool hardDisk)
    {
        var bytes = Image();
        var engine = MediaEngineComposition.CreateDefault();
        GWGUI.MediaEngine.Interfaces.IMediaImageRepresentation representation = hardDisk
            ? new BlockMediaImageRepresentation(bytes.Length, 256, [new(0, bytes.Length, MediaDataRangeKind.Stored, new MemoryRandomAccessData(bytes))], new(2, 4, 33, 256))
            : new SectorMediaImageRepresentation(new SectorImage("acorn.adfs.bbc", 256, 2, 4, 33, Enumerable.Range(0, bytes.Length / 256).Select(index => new SectorBlock(index, new(index / 132, index / 33 % 4, index % 33), bytes.AsSpan(index * 256, 256).ToArray()))));
        var document = MediaImageDocument.CreateUnexplored(new("memory.dat", []), hardDisk ? HardDiskImageFormatIds.AcornWinchester : "acorn.adfs.bbc", hardDisk ? MediaKind.HardDisk : MediaKind.Floppy, representation, [], new Dictionary<string, string>());
        var result = await engine.Explorer.ExploreAsync(document);
        var volume = Assert.Single(result.Volumes).FileSystem!;
        Assert.NotNull(volume);
        var file = Assert.Single(volume.Entries, entry => entry.Name == "HELLO");
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, file.Content!.ToArray());
        var folder = Assert.Single(volume.Entries, entry => entry.Name == "DIRX");
        Assert.Equal(new byte[] { 5, 6, 7 }, Assert.Single(folder.Children).Content!.ToArray());
    }

    internal static byte[] Image()
    {
        var bytes = new byte[2 * 4 * 33 * 256];
        void Directory(int sector, string name)
        {
            var offset = sector * 256;
            bytes[offset] = bytes[offset + 1274] = 1;
            "Hugo"u8.CopyTo(bytes.AsSpan(offset + 1)); "Hugo"u8.CopyTo(bytes.AsSpan(offset + 1275));
            System.Text.Encoding.ASCII.GetBytes(name).CopyTo(bytes, offset + 1228);
        }
        void Entry(int sector, int number, string name, int address, int length, bool folder = false)
        {
            var offset = sector * 256 + 5 + number * 26;
            System.Text.Encoding.ASCII.GetBytes(name).CopyTo(bytes, offset);
            bytes[offset] |= 128; bytes[offset + 1] |= 128;
            if (folder) bytes[offset + 3] |= 128;
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset + 18), (uint)length);
            bytes[offset + 22] = (byte)address;
            bytes[offset + 25] = 8; // Numéro de séquence BBC : ce n'est pas le bit répertoire FileCore.
        }
        Directory(2, "$"); Directory(7, "DIRX");
        Entry(2, 0, "HELLO", 12, 4); Entry(2, 1, "DIRX", 7, 1280, true); Entry(7, 0, "DEEP", 13, 3);
        new byte[] { 1, 2, 3, 4 }.CopyTo(bytes, 12 * 256); new byte[] { 5, 6, 7 }.CopyTo(bytes, 13 * 256);
        return bytes;
    }
}
