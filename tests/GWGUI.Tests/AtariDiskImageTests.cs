using System.Buffers.Binary;
using System.IO;
using GWGUI.Scp;
using GWGUI.Scp.Decoding;
using GWGUI.Scp.Encoding;
using GWGUI.Scp.FileSystems.Readers;
using GWGUI.Scp.Images;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Write;

namespace GWGUI.Tests;

public sealed class AtariDiskImageTests
{
    [Theory]
    [InlineData(9, 2, 0, 79, "atarist.720")]
    [InlineData(10, 2, 0, 79, "atarist.800")]
    [InlineData(10, 2, 0, 80, "atarist.810")]
    [InlineData(11, 2, 0, 79, "atarist.880")]
    public async Task MsaDetectionUsesTheContainerGeometry(int sectors, int heads, int firstCylinder, int lastCylinder, string expected)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.msa");
        try
        {
            var header = new byte[10];
            BinaryPrimitives.WriteUInt16BigEndian(header, 0x0e0f);
            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(2), checked((ushort)sectors));
            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(4), checked((ushort)(heads - 1)));
            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(6), checked((ushort)firstCylinder));
            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(8), checked((ushort)lastCylinder));
            await File.WriteAllBytesAsync(path, header);
            var result = new ImageFormatDetector(new BuiltInImageFormatCatalog()).Detect(path, header.Length);
            Assert.Equal(expected, result.Format?.Id);
            Assert.Equal(FormatConfidence.Certain, result.Confidence);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData("iso.mfm", 9, 512)]
    [InlineData("iso.mfm", 10, 512)]
    [InlineData("iso.mfm", 11, 512)]
    [InlineData("iso.mfm", 18, 512)]
    [InlineData("iso.fm", 18, 128)]
    [InlineData("iso.mfm", 26, 128)]
    [InlineData("iso.mfm", 18, 256)]
    public void CompleteAtariTracksRoundTripWithSectorData(string codec, int sectorCount, int sectorSize)
    {
        var expected = Enumerable.Range(1, sectorCount).Select(number => new TrackSector(number,
            Enumerable.Range(0, sectorSize).Select(index => (byte)(number * 17 + index * 31)).ToArray())).ToArray();
        var encoded = new FluxEncoderRegistry().Encode(codec, new TrackEncodeRequest(12, 0, expected));
        var decoded = new FluxDecoderRegistry().Decode(codec, encoded.Revolution);
        Assert.Equal(sectorCount, decoded.Sectors!.Count);
        foreach (var sector in expected)
        {
            var actual = Assert.Single(decoded.Sectors, item => item.Number == sector.Number);
            Assert.True(actual.IntegrityValid);
            Assert.Equal(sector.Data, actual.Data);
        }
    }

    [Theory]
    [InlineData(368640, "atarist.360", 1, 9)]
    [InlineData(737280, "atarist.720", 2, 9)]
    [InlineData(819200, "atarist.800", 2, 10)]
    [InlineData(829440, "atarist.810", 2, 10)]
    [InlineData(901120, "atarist.880", 2, 11)]
    [InlineData(1474560, "atarist.1440", 2, 18)]
    public async Task AtariStReaderRecognizesStandardRawGeometries(int length, string format, int heads, int sectors)
    {
        var path = Path.GetTempFileName();
        try
        {
            var data = new byte[length];
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(11), 512);
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(19), checked((ushort)(length / 512)));
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(24), checked((ushort)sectors));
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(26), checked((ushort)heads));
            await File.WriteAllBytesAsync(path, data);
            var image = await new AtariStImageReader().ReadAsync(path);
            Assert.Equal(format, image.FormatId); Assert.Equal(heads, image.Heads); Assert.Equal(sectors, image.SectorsPerTrack);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public async Task AtrReaderPreservesMixedBootSectorSizes()
    {
        var path = Path.GetTempFileName();
        try
        {
            var payload = 3 * 128 + 717 * 256; var data = new byte[16 + payload];
            BinaryPrimitives.WriteUInt16LittleEndian(data, 0x0296); BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(2), (ushort)(payload / 16));
            BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(4), 256); await File.WriteAllBytesAsync(path, data);
            var image = await new AtrImageReader().ReadAsync(path);
            Assert.Equal("atari.180", image.FormatId); Assert.Equal(720, image.BlockCount); Assert.Equal(128, image.GetBlock(0).Length); Assert.Equal(256, image.GetBlock(3).Length);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void AtariDosReaderExtractsAFile()
    {
        var blocks = Enumerable.Range(0, 720).Select(index => new GWGUI.Scp.SectorImages.SectorBlock(index, new(index, 0, index + 1), new byte[128])).ToArray();
        ((byte[])blocks[359].Data)[0] = 2;
        var directory = (byte[])blocks[360].Data; directory[0] = 0x42; BinaryPrimitives.WriteUInt16LittleEndian(directory.AsSpan(1), 1); BinaryPrimitives.WriteUInt16LittleEndian(directory.AsSpan(3), 4);
        System.Text.Encoding.ASCII.GetBytes("HELLO   TXT").CopyTo(directory, 5);
        var content = (byte[])blocks[3].Data; System.Text.Encoding.ASCII.GetBytes("ATARI").CopyTo(content, 0); content[125] = 0; content[126] = 0; content[127] = 5;
        var image = new GWGUI.Scp.SectorImages.SectorImage("atari.90", 128, 720, 1, 1, blocks);
        var volume = new AtariDosFileSystemReader().Read(image); var file = Assert.Single(volume.Entries);
        Assert.Equal("HELLO.TXT", file.Name); Assert.Equal("ATARI", System.Text.Encoding.ASCII.GetString(file.Content!.ToArray()));
    }

    [Fact]
    public void AtariDosReaderRejectsRandomDataThatOnlyResemblesDirectoryFlags()
    {
        var blocks = Enumerable.Range(0, 720).Select(index => new GWGUI.Scp.SectorImages.SectorBlock(index, new(index, 0, index + 1), new byte[128])).ToArray();
        ((byte[])blocks[359].Data)[0] = 0x19;
        ((byte[])blocks[360].Data)[0] = 0xa6;
        ((byte[])blocks[360].Data)[5] = 0x1a;
        var image = new GWGUI.Scp.SectorImages.SectorImage("atari.90", 128, 720, 1, 1, blocks);

        Assert.False(new AtariDosFileSystemReader().CanRead(image));
    }

    [Fact]
    public async Task RealAtariCorpusContainersAndFileSystemsAreReadableWhenRequested()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_ATARI_CORPUS"); if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var explorer = DiskImageExplorer.CreateDefault(); var opened = 0;
        foreach (var path in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories).Where(path => Path.GetExtension(path).ToLowerInvariant() is ".st" or ".msa" or ".atr"))
        {
            try { var document = await explorer.ExploreAsync(path); Console.WriteLine($"OPEN {Path.GetFileName(path)}: {document.Volume.FileSystem}, {document.Volume.Entries.Count} root entries"); opened++; }
            catch (InvalidDataException exception) { Console.WriteLine($"CONTAINER ONLY {Path.GetFileName(path)}: {exception.Message}");
                if (Path.GetExtension(path).Equals(".st", StringComparison.OrdinalIgnoreCase)) _ = await new AtariStImageReader().ReadAsync(path);
                else if (Path.GetExtension(path).Equals(".msa", StringComparison.OrdinalIgnoreCase)) _ = await new MsaImageReader().ReadAsync(path);
                else _ = await new AtrImageReader().ReadAsync(path);
            }
        }
        Assert.True(opened > 0, "No Atari file system in the requested corpus could be explored.");
    }

    [Fact]
    public async Task GeneratedAtariScpImagesMatchTheirSectorSourcesWhenRequested()
    {
        var root = Environment.GetEnvironmentVariable("GWGUI_ATARI_CORPUS"); if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) return;
        var generated = Path.Combine(root, "_generated"); if (!Directory.Exists(generated)) return;
        var scpReader = new GWGUI.Scp.SectorImages.AtariScpSectorImageReader(new ScpReader(), new FluxDecoderRegistry()); var compared = 0;
        foreach (var scpPath in Directory.EnumerateFiles(generated, "*.scp", SearchOption.AllDirectories).Where(path => path.Contains("Atari", StringComparison.OrdinalIgnoreCase)))
        {
            var machineFolder = new DirectoryInfo(Path.GetDirectoryName(scpPath)!).Name;
            var sourceDirectory = Path.Combine(root, machineFolder); if (!Directory.Exists(sourceDirectory)) continue;
            var baseName = Path.GetFileNameWithoutExtension(scpPath).Replace(" [test]", string.Empty, StringComparison.OrdinalIgnoreCase);
            var sourcePath = Directory.EnumerateFiles(sourceDirectory).FirstOrDefault(path => Path.GetFileNameWithoutExtension(path).Equals(baseName, StringComparison.OrdinalIgnoreCase));
            if (sourcePath is null) continue;
            GWGUI.Scp.SectorImages.SectorImage source;
            var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            if (extension == ".st") source = await new AtariStImageReader().ReadAsync(sourcePath);
            else if (extension == ".msa") source = await new MsaImageReader().ReadAsync(sourcePath);
            else if (extension == ".atr") source = await new AtrImageReader().ReadAsync(sourcePath);
            else continue;
            var actual = await scpReader.ReadAsync(scpPath, source.FormatId);
            var differences = source.AvailableBlocks.Count(block => !actual.TryGetBlock(block.LogicalBlock, out var decoded) || !block.Data.SequenceEqual(decoded.Data));
            foreach (var block in source.AvailableBlocks.Where(block => actual.TryGetBlock(block.LogicalBlock, out _)))
                Assert.Equal(block.Data, actual.GetBlock(block.LogicalBlock).ToArray());
            Console.WriteLine($"{machineFolder}: {Path.GetFileName(sourcePath)} -> {actual.AvailableBlocks.Count}/{source.BlockCount} sectors, {differences} differences");
            Assert.Equal(0, differences); compared++;
        }
        Assert.True(compared > 0, "No generated Atari SCP/source pair was found.");
    }
}
