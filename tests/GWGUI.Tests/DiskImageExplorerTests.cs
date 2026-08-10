using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;
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
    [InlineData("Drawer", FileSystemEntryKind.Directory, ExplorerFileSystemFamily.Unknown, ExplorerIconKind.Folder)]
    [InlineData("ReadMe.txt", FileSystemEntryKind.File, ExplorerFileSystemFamily.IbmPc, ExplorerIconKind.Text)]
    [InlineData("Picture.iff", FileSystemEntryKind.File, ExplorerFileSystemFamily.Amiga, ExplorerIconKind.Image)]
    [InlineData("Music.mod", FileSystemEntryKind.File, ExplorerFileSystemFamily.Amiga, ExplorerIconKind.Audio)]
    [InlineData("Files.lha", FileSystemEntryKind.File, ExplorerFileSystemFamily.Amiga, ExplorerIconKind.Archive)]
    [InlineData("Program.exe", FileSystemEntryKind.File, ExplorerFileSystemFamily.IbmPc, ExplorerIconKind.Program)]
    [InlineData("Disk.adf", FileSystemEntryKind.File, ExplorerFileSystemFamily.Amiga, ExplorerIconKind.DiskImage)]
    [InlineData("Alias", FileSystemEntryKind.Link, ExplorerFileSystemFamily.Unknown, ExplorerIconKind.Link)]
    public void ExplorerUsesDistinctIconsForEntryTypes(string name, FileSystemEntryKind kind, ExplorerFileSystemFamily family, ExplorerIconKind expected)
    {
        var entry = new FileSystemEntry(name, kind, 0, null, string.Empty, 0, 0, true, []);
        Assert.Equal(expected, ExplorerFileIconClassifier.IconFor(entry, family));
    }

    [Fact]
    public void ProgramExtensionsAreInterpretedForTheCurrentMachineOnly()
    {
        var batch = new FileSystemEntry("START.BAT", FileSystemEntryKind.File, 0, null, string.Empty, 0, 0, true, []);
        var atariProgram = new FileSystemEntry("GAME.PRG", FileSystemEntryKind.File, 0, null, string.Empty, 0, 0, true, []);

        Assert.Equal(ExplorerIconKind.Program, ExplorerFileIconClassifier.IconFor(batch, ExplorerFileSystemFamily.IbmPc));
        Assert.Equal(ExplorerIconKind.File, ExplorerFileIconClassifier.IconFor(batch, ExplorerFileSystemFamily.Amiga));
        Assert.Equal(ExplorerIconKind.Program, ExplorerFileIconClassifier.IconFor(atariProgram, ExplorerFileSystemFamily.AtariSt));
        Assert.Equal(ExplorerIconKind.File, ExplorerFileIconClassifier.IconFor(atariProgram, ExplorerFileSystemFamily.IbmPc));
    }

    [Theory]
    [InlineData(ExplorerFileSystemFamily.AppleDos, "Applesoft BASIC", ExplorerIconKind.Program)]
    [InlineData(ExplorerFileSystemFamily.ProDos, "Text", ExplorerIconKind.Text)]
    [InlineData(ExplorerFileSystemFamily.Commodore, "PRG", ExplorerIconKind.Program)]
    [InlineData(ExplorerFileSystemFamily.Macintosh, "APPL", ExplorerIconKind.Program)]
    [InlineData(ExplorerFileSystemFamily.Macintosh, "PICT", ExplorerIconKind.Image)]
    public void NativeCatalogTypesTakePriority(ExplorerFileSystemFamily family, string nativeType, ExplorerIconKind expected)
    {
        var entry = new FileSystemEntry("NO_EXTENSION", FileSystemEntryKind.File, 0, null, nativeType, 0, 0, true, []);
        Assert.Equal(expected, ExplorerFileIconClassifier.IconFor(entry, family));
    }

    [Theory]
    [InlineData(ExplorerIconKind.Text, "Explorer.Type.Text")]
    [InlineData(ExplorerIconKind.Image, "Explorer.Type.Image")]
    [InlineData(ExplorerIconKind.Audio, "Explorer.Type.Audio")]
    [InlineData(ExplorerIconKind.Archive, "Explorer.Type.Archive")]
    [InlineData(ExplorerIconKind.Program, "Explorer.Type.Program")]
    [InlineData(ExplorerIconKind.DiskImage, "Explorer.Type.DiskImage")]
    [InlineData(ExplorerIconKind.File, "Explorer.File")]
    public void ExplorerTypeMatchesTheDetectedIcon(ExplorerIconKind kind, string expectedResourceKey)
        => Assert.Equal(expectedResourceKey, ExplorerFileIconClassifier.TypeResourceKeyFor(kind));

    [Fact]
    public void ExplorerIssueDialogIncludesFileSystemWarningsBadSectorsAndMissingSectors()
    {
        var blocks = new[]
        {
            new SectorBlock(0, new SectorAddress(0, 0, 0), new byte[256]),
            new SectorBlock(1, new SectorAddress(0, 0, 1), new byte[256], IntegrityValid: false)
        };
        var image = new SectorImage("apple2.dos33", 256, 1, 1, 3, blocks);
        var volume = new FileSystemVolume("TEST", "Apple DOS 3.3", 768, 0, null, null, [], ["Catalog warning"]);
        var issues = ExplorerSection.BuildIssues(new ExploredDiskImage("test.nib", image, volume));

        Assert.Contains("Catalog warning", issues);
        Assert.Contains(issues, issue => issue.Contains("1", StringComparison.Ordinal));
        Assert.Contains(issues, issue => issue.Contains("2", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(AdfImageReader.DoubleDensityBytes, 11, "amiga.amigados")]
    [InlineData(AdfImageReader.HighDensityBytes, 22, "amiga.amigados_hd")]
    public async Task AdfReaderBuildsAmigaGeometry(int byteLength, int sectorsPerTrack, string formatId)
    {
        var path = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(path, new byte[byteLength]);
            var image = await new AdfImageReader().ReadAsync(path);
            Assert.Equal(formatId, image.FormatId);
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
    public void AmigaDosHdFallsBackToItsConventionalRootWhenTheBootPointerIsStale()
    {
        var image = BuildAmigaImage(fastFileSystem: true, sectorsPerTrack: 22, bootRootPointer: 880);
        var volume = new AmigaDosFileSystemReader().Read(image);

        Assert.Equal("Workbench", volume.Name);
        Assert.Equal("AmigaDOS FFS", volume.FileSystem);
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
    public async Task ValidContainerWithoutKnownFileSystemStillOpensAsDiskInformation()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.st");
        try
        {
            var bytes = new byte[368640];
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(11), 512);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(19), 720);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(24), 9);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(26), 1);
            await File.WriteAllBytesAsync(path, bytes);

            var result = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

            Assert.False(result.FileSystemRecognized);
            Assert.Equal(368640, result.Volume.Capacity);
            Assert.Equal(80, result.Volume.Entries.Count);
            Assert.Equal(720, result.Volume.Entries.Sum(track => track.Children.Count));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task ValidFatBpbTakesPriorityOverAnAccidentalCpmDirectoryPattern()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.img");
        try
        {
            var bytes = new byte[360 * 1024];
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(11), 512);
            bytes[13] = 2;
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(14), 1);
            bytes[16] = 2;
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(17), 112);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(19), 720);
            bytes[21] = 0xfd;
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(22), 2);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(24), 9);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(26), 2);

            const int falseDirectory = 4096;
            bytes.AsSpan(falseDirectory, 64 * 32).Fill(0xe5);
            bytes[falseDirectory] = 0;
            "FAKE    TXT"u8.CopyTo(bytes.AsSpan(falseDirectory + 1));
            bytes[falseDirectory + 15] = 1;
            Assert.True(AmstradCpmFileSystemReader.LooksLikeCpcRawImage(bytes));

            await File.WriteAllBytesAsync(path, bytes);
            var result = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

            Assert.Equal("ibm.360", result.Image.FormatId);
            Assert.DoesNotContain(result.DetectedFileSystems ?? [], match => match.FormatId.StartsWith("amstrad.", StringComparison.OrdinalIgnoreCase));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task UnknownImageFormatOpensAsUnrecognizedWithoutThrowing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.unknown");
        try
        {
            await File.WriteAllBytesAsync(path, [1, 2, 3, 4, 5]);

            var result = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

            Assert.False(result.FileSystemRecognized);
            Assert.Equal("unknown", result.Image.FormatId);
            Assert.Equal(5, result.Volume.Capacity);
            Assert.Empty(result.Volume.Entries);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task AtrRawPayloadStagingRemovesOnlyTheContainerHeader()
    {
        var source = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.atr");
        var destination = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.img");
        try
        {
            var payload = Enumerable.Range(0, 720 * 128).Select(index => (byte)index).ToArray();
            var container = new byte[payload.Length + 16];
            BinaryPrimitives.WriteUInt16LittleEndian(container, 0x0296);
            BinaryPrimitives.WriteUInt16LittleEndian(container.AsSpan(2), checked((ushort)(payload.Length / 16)));
            BinaryPrimitives.WriteUInt16LittleEndian(container.AsSpan(4), 128);
            payload.CopyTo(container, 16);
            await File.WriteAllBytesAsync(source, container);

            await AtrImageReader.WriteRawPayloadAsync(source, destination);

            Assert.Equal(payload, await File.ReadAllBytesAsync(destination));
        }
        finally
        {
            if (File.Exists(source)) File.Delete(source);
            if (File.Exists(destination)) File.Delete(destination);
        }
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
        Assert.Equal("amiga.amigados", image.FormatId);
        Assert.Equal(Enumerable.Repeat((byte)8, 512), image.GetBlock(7).ToArray());
        Assert.Equal(11, image.AvailableBlocks.Count);
    }

    [Fact]
    public void IsolatedInvalidSectorNumberDoesNotTurnAnAmigaDdImageIntoHd()
    {
        var addresses = Enumerable.Range(0, 11).Select(number => new SectorAddress(0, 0, number))
            .Append(new SectorAddress(12, 1, 19));
        Assert.Equal(11, AmigaScpSectorImageReader.InferSectorsPerTrack(addresses));
    }

    [Fact]
    public void MultipleCompleteTwentyTwoSectorTracksIdentifyAnAmigaHdImage()
    {
        var addresses = Enumerable.Range(0, 2)
            .SelectMany(cylinder => Enumerable.Range(0, 22).Select(number => new SectorAddress(cylinder, 0, number)));
        Assert.Equal(22, AmigaScpSectorImageReader.InferSectorsPerTrack(addresses));
    }

    private static SectorImage BuildAmigaImage(bool fastFileSystem, int sectorsPerTrack = 11, int? bootRootPointer = null)
    {
        var blocks = 80 * 2 * sectorsPerTrack; var rootBlock = blocks / 2; var bitmapBlock = rootBlock + 1;
        var data = new byte[blocks * 512];
        data[0] = (byte)'D'; data[1] = (byte)'O'; data[2] = (byte)'S'; data[3] = fastFileSystem ? (byte)1 : (byte)0; WriteInt(data, 8, bootRootPointer ?? rootBlock);

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
            var track = logical / sectorsPerTrack;
            return new SectorBlock(logical, new(track / 2, track % 2, logical % sectorsPerTrack), data.AsSpan(logical * 512, 512).ToArray());
        });
        return new(sectorsPerTrack == 22 ? "amiga.amigados_hd" : "amiga.amigados", 512, 80, 2, sectorsPerTrack, sectorBlocks);
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
