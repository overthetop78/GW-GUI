using System.Buffers.Binary;
using System.IO;
using GWGUI.Scp;
using GWGUI.Scp.Decoding;
using GWGUI.Scp.Encoding;
using GWGUI.Scp.FileSystems;
using GWGUI.Scp.FileSystems.Readers;
using GWGUI.Scp.Images;
using GWGUI.Scp.SectorImages;
using GWGUI.App.Controls;

namespace GWGUI.Tests;

public sealed class DiskImageExplorerTests
{
    [Fact]
    public void ExplorerCapabilitiesComeFromRegisteredFileSystemReaders()
    {
        var explorer = DiskImageExplorer.CreateDefault();
        Assert.Contains("amiga.amigados", explorer.SupportedFormatIds);
        Assert.Contains("amiga.amigados_hd", explorer.SupportedFormatIds);
    }

    [Theory]
    [InlineData("Drawer", FileSystemEntryKind.Directory, ExplorerIconKind.Folder)]
    [InlineData("ReadMe.txt", FileSystemEntryKind.File, ExplorerIconKind.Text)]
    [InlineData("Picture.iff", FileSystemEntryKind.File, ExplorerIconKind.Image)]
    [InlineData("Music.mod", FileSystemEntryKind.File, ExplorerIconKind.Audio)]
    [InlineData("Files.lha", FileSystemEntryKind.File, ExplorerIconKind.Archive)]
    [InlineData("Program.exe", FileSystemEntryKind.File, ExplorerIconKind.Program)]
    [InlineData("Disk.adf", FileSystemEntryKind.File, ExplorerIconKind.DiskImage)]
    [InlineData("Alias", FileSystemEntryKind.Link, ExplorerIconKind.Link)]
    public void ExplorerUsesDistinctIconsForEntryTypes(string name, FileSystemEntryKind kind, ExplorerIconKind expected)
    {
        var entry = new FileSystemEntry(name, kind, 0, null, string.Empty, 0, 0, true, []);
        Assert.Equal(expected, ExplorerFileIconClassifier.IconFor(entry));
    }

    [Theory]
    [InlineData(AdfImageReader.DoubleDensityBytes, 11)]
    [InlineData(AdfImageReader.HighDensityBytes, 22)]
    public async Task AdfReaderBuildsAmigaGeometry(int byteLength, int sectorsPerTrack)
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(path, new byte[byteLength]);
            var image = await new AdfImageReader().ReadAsync(path);
            Assert.Equal(80, image.Cylinders); Assert.Equal(2, image.Heads); Assert.Equal(sectorsPerTrack, image.SectorsPerTrack);
            Assert.Equal(byteLength, image.Capacity); Assert.Empty(image.MissingBlocks);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void AmigaDosReaderReturnsVolumeDirectoriesFilesAndContents()
    {
        var image = BuildAmigaImage(fastFileSystem: true);
        var volume = new AmigaDosFileSystemReader().Read(image);

        Assert.Equal("Workbench", volume.Name); Assert.Equal("AmigaDOS FFS", volume.FileSystem);
        var file = Assert.Single(volume.Entries, entry => entry.Kind == FileSystemEntryKind.File);
        Assert.Equal("Hello", file.Name); Assert.Equal("hello"u8.ToArray(), file.Content);
        var drawer = Assert.Single(volume.Entries, entry => entry.Kind == FileSystemEntryKind.Directory);
        Assert.Equal("Drawer", drawer.Name);
        var nested = Assert.Single(drawer.Children);
        Assert.Equal("Nested", nested.Name); Assert.Equal("inside"u8.ToArray(), nested.Content);
    }

    [Fact]
    public void AmigaDosReaderReadsOfsDataBlocks()
    {
        var image = BuildAmigaImage(fastFileSystem: false);
        var volume = new AmigaDosFileSystemReader().Read(image);
        Assert.Equal("AmigaDOS OFS", volume.FileSystem);
        Assert.Equal("hello"u8.ToArray(), volume.Entries.Single(entry => entry.Name == "Hello").Content);
    }

    [Fact]
    public async Task ExplorerReadsAnAdfEndToEndAndAcceptsTheCatalogFormatId()
    {
        var image = BuildAmigaImage(fastFileSystem: true);
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.adf");
        try
        {
            var bytes = new byte[checked((int)image.Capacity)];
            foreach (var block in image.AvailableBlocks) block.Data.ToArray().CopyTo(bytes, block.LogicalBlock * 512);
            await File.WriteAllBytesAsync(path, bytes);
            var result = await DiskImageExplorer.CreateDefault().ExploreAsync(path, "amiga.amigados");
            Assert.Equal("Workbench", result.Volume.Name);
            Assert.Contains(result.Volume.Entries, entry => entry.Name == "Hello");
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task ScpReconstructionAssociatesAmigaPayloadWithLogicalSector()
    {
        var sectors = Enumerable.Range(0, 11).Select(number => new TrackSector(number, Enumerable.Repeat((byte)(number + 1), 512).ToArray())).ToArray();
        var encoded = new FluxEncoderRegistry().Encode("amiga.mfm", new TrackEncodeRequest(0, 0, sectors));
        var scp = new ScpImage(new(0, 0, 1, 0, 0, ScpFlags.IndexAligned, 16, 0, 0, 0), [new ScpTrack(0, 0, 0, [encoded.Revolution])], true, 0);
        var reader = new AmigaScpSectorImageReader(new MemoryScpReader(scp), new FluxDecoderRegistry());
        var image = await reader.ReadAsync("memory.scp");

        Assert.Equal(11, image.SectorsPerTrack);
        Assert.Equal(Enumerable.Repeat((byte)8, 512), image.GetBlock(7).ToArray());
        Assert.Equal(11, image.AvailableBlocks.Count);
    }

    private static SectorImage BuildAmigaImage(bool fastFileSystem)
    {
        const int blocks = 1760; const int rootBlock = 880; const int bitmapBlock = 881;
        var data = new byte[blocks * 512];
        data[0] = (byte)'D'; data[1] = (byte)'O'; data[2] = (byte)'S'; data[3] = fastFileSystem ? (byte)1 : (byte)0; WriteInt(data, 8, rootBlock);

        var root = Block(data, rootBlock); WriteInt(root, 0, 2); WriteInt(root, 12, 72); WriteInt(root, 24, 10); WriteInt(root, 316, bitmapBlock);
        WriteBString(root, 432, "Workbench"); WriteInt(root, 508, 1); SetChecksum(root);

        var file = Block(data, 10); WriteInt(file, 0, 2); WriteInt(file, 4, 10); WriteInt(file, 8, 1); WriteInt(file, 308, 11);
        WriteInt(file, 324, 5); WriteBString(file, 432, "Hello"); WriteBString(file, 328, "Greeting"); WriteInt(file, 496, 12); WriteInt(file, 500, rootBlock); WriteInt(file, 508, -3); SetChecksum(file);
        WriteFileData(Block(data, 11), "hello"u8, fastFileSystem, 10);

        var drawer = Block(data, 12); WriteInt(drawer, 0, 2); WriteInt(drawer, 4, 12); WriteInt(drawer, 24, 13); WriteBString(drawer, 432, "Drawer"); WriteInt(drawer, 500, rootBlock); WriteInt(drawer, 508, 2); SetChecksum(drawer);
        var nested = Block(data, 13); WriteInt(nested, 0, 2); WriteInt(nested, 4, 13); WriteInt(nested, 8, 1); WriteInt(nested, 308, 14);
        WriteInt(nested, 324, 6); WriteBString(nested, 432, "Nested"); WriteInt(nested, 500, 12); WriteInt(nested, 508, -3); SetChecksum(nested);
        WriteFileData(Block(data, 14), "inside"u8, fastFileSystem, 13);

        var bitmap = Block(data, bitmapBlock); for (var offset = 4; offset < 512; offset += 4) WriteUInt(bitmap, offset, uint.MaxValue); SetChecksum(bitmap, 0);
        var sectorBlocks = Enumerable.Range(0, blocks).Select(logical =>
        {
            var track = logical / 11;
            return new SectorBlock(logical, new(track / 2, track % 2, logical % 11), data.AsSpan(logical * 512, 512).ToArray());
        });
        return new("amiga.amigados", 512, 80, 2, 11, sectorBlocks);
    }

    private static void WriteFileData(Span<byte> block, ReadOnlySpan<byte> content, bool ffs, int header)
    {
        if (ffs) content.CopyTo(block);
        else { WriteInt(block, 0, 8); WriteInt(block, 4, header); WriteInt(block, 8, 1); WriteInt(block, 12, content.Length); content.CopyTo(block[24..]); SetChecksum(block); }
    }

    private static Span<byte> Block(byte[] data, int number) => data.AsSpan(number * 512, 512);
    private static void WriteBString(Span<byte> data, int offset, string value) { data[offset] = (byte)value.Length; System.Text.Encoding.Latin1.GetBytes(value).CopyTo(data[(offset + 1)..]); }
    private static void WriteInt(Span<byte> data, int offset, int value) => BinaryPrimitives.WriteInt32BigEndian(data[offset..], value);
    private static void WriteUInt(Span<byte> data, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(data[offset..], value);
    private static void SetChecksum(Span<byte> block, int offset = 20)
    {
        WriteUInt(block, offset, 0); uint sum = 0; for (var position = 0; position < block.Length; position += 4) sum = unchecked(sum + BinaryPrimitives.ReadUInt32BigEndian(block[position..]));
        WriteUInt(block, offset, unchecked(0u - sum));
    }

    private sealed class MemoryScpReader(ScpImage image) : IScpReader
    {
        public Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default) => Task.FromResult(image);
    }
}
