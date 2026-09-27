using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats.Cartridge.AmstradCpr;
using GWGUI.MediaEngine.Images.Formats.Cartridge.AmstradRom;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Images.Formats.Tape;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Amstrad;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Amstrad;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.App.Services.DiskImages.Selection;
using GWGUI.App.Presenters.Conversion;
using GWGUI.MediaFileSystems.FileSystems.Amstrad.Cartridge;
using GWGUI.MediaFileSystems.FileSystems.Cpm;
using GWGUI.MediaFileSystems.Contracts;

namespace GWGUI.Tests.Media;

public sealed class AmstradCpcMediaFormatTests
{
    [Fact]
    public void RuntimeAmstradDestinationsKeepTheExistingFamilyAndRejectStaleAmigaSelection()
    {
        var curated = new BuiltInImageFormatCatalog(key => key);
        var runtime = new RuntimeImageFormatCatalog(curated,
        [
            new DiskFormat(
                DiskImageFormatIds.AmstradRom,
                string.Empty,
                "Format.amstrad.rom",
                [new ImageExtension(".rom", "ROM", true)],
                CompatibleSourceExtensions: new HashSet<string>([".bin"], StringComparer.OrdinalIgnoreCase))
        ]);

        var rom = runtime.Formats.Single(format => format.Id == DiskImageFormatIds.AmstradRom);
        Assert.Equal("Amstrad", rom.Family);
        Assert.Equal("Format.amstrad.rom", rom.DisplayName);
        Assert.Contains(".bin", rom.CompatibleSourceExtensions!);

        var items = new ConversionFormatPresenter().Build(
            runtime,
            ".bin",
            null,
            new HashSet<string>(["amiga.amigados"], StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase));
        Assert.False(items.Single(item => item.Format.Id == "amiga.amigados").IsCompatible);
        Assert.True(items.Single(item => item.Format.Id == DiskImageFormatIds.AmstradRom).IsCompatible);
    }

    [Fact]
    public async Task CpcBlocksRoundTripThroughTzxRepresentation()
    {
        var block = new SequentialDecodedBlock(
            0,
            new byte[] { 0x10, 0x20, 0x30 },
            [],
            true,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["fileName"] = "HELLO",
                ["blockNumber"] = "1"
            });
        var encoder = new AmstradCpcTapeEncoder();
        var representation = await encoder.EncodeAsync(new SequentialEncodeRequest(
            TapeImageFormatIds.Tzx,
            DiskSystemIds.Amstrad,
            [block]));
        var document = new MediaImageDocument(
            new MediaSourceDescriptor("memory.cdt", []),
            TapeImageFormatIds.Tzx,
            MediaKind.Tape,
            representation,
            [],
            [],
            new Dictionary<string, string>());

        var result = await new AmstradCpcTapeDecoder().DecodeAsync(document, DiskSystemIds.Amstrad);

        var decoded = Assert.Single(result.Blocks);
        Assert.Equal(block.Data.ToArray(), decoded.Data.ToArray());
        Assert.Equal("HELLO", decoded.Metadata["fileName"]);
        Assert.Equal(bool.TrueString, decoded.Metadata["firstBlock"]);
        Assert.Equal(bool.TrueString, decoded.Metadata["lastBlock"]);
    }

    [Fact]
    public async Task CprAndRomExposeTheirRealBanks()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-amstrad-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var cprPath = Path.Combine(directory, "cartridge.cpr");
            var romPath = Path.Combine(directory, "cartridge.rom");
            await File.WriteAllBytesAsync(cprPath, CreateCpr());
            await File.WriteAllBytesAsync(romPath, Enumerable.Repeat((byte)0x5a, 16 * 1024).ToArray());

            var cpr = await ReadAsync(new AmstradCprReader(), cprPath);
            var rom = await ReadAsync(new AmstradRomReader(), romPath, DiskImageFormatIds.AmstradRom);
            var explorer = new AmstradCartridgeFileSystemReader();

            Assert.Equal(["cb00", "cb01"], explorer.Read(cpr, Volume(cpr)).Entries.Select(entry => entry.Name));
            Assert.Equal("bank00", Assert.Single(explorer.Read(rom, Volume(rom)).Entries).Name);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task AutomaticRomRecognitionRequiresCpcStructureInsteadOfAnAmbiguousSize()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-amstrad-rom-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var dandanatorPath = Path.Combine(directory, "dandanator.rom");
            var foreignPath = Path.Combine(directory, "foreign.rom");
            var amsdosPath = Path.Combine(directory, "headered.rom");
            var dandanator = new byte[512 * 1024];
            dandanator[0] = 0xfd;
            dandanator[1] = 0xfd;
            dandanator[2] = 0xfd;
            dandanator[3] = 0x70;
            var amsdos = new byte[128 + 16 * 1024];
            System.Text.Encoding.ASCII.GetBytes("CPCROM", amsdos.AsSpan(1));
            var checksum = amsdos.AsSpan(0, 67).ToArray().Sum(value => value) & ushort.MaxValue;
            BinaryPrimitives.WriteUInt16LittleEndian(amsdos.AsSpan(67), checked((ushort)checksum));
            await File.WriteAllBytesAsync(dandanatorPath, dandanator);
            await File.WriteAllBytesAsync(foreignPath, new byte[512 * 1024]);
            await File.WriteAllBytesAsync(amsdosPath, amsdos);
            IMediaImageReader reader = new AmstradRomReader();

            Assert.True(await reader.CanReadAsync(new MediaRecognitionContext(
                new MediaSourceDescriptor(dandanatorPath, [])), CancellationToken.None));
            Assert.True(await reader.CanReadAsync(new MediaRecognitionContext(
                new MediaSourceDescriptor(amsdosPath, [])), CancellationToken.None));
            Assert.False(await reader.CanReadAsync(new MediaRecognitionContext(
                new MediaSourceDescriptor(foreignPath, [])), CancellationToken.None));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void DefaultRecognitionRegistersBothRawFluxFamilies()
    {
        var readers = MediaRecognitionComposition.CreateDefault().Readers;

        Assert.Contains(readers, reader => reader.SupportsFormatId(DiskImageFormatIds.RawHxcStream));
        Assert.Contains(readers, reader => reader.SupportsFormatId(DiskImageFormatIds.RawKryoFlux));
    }

    [Fact]
    public void DefaultRecognitionPublishesEveryRequestedAmstradMediaExtension()
    {
        var extensions = MediaRecognitionComposition.CreateDefault().SupportedExtensions;

        var required = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".dsk", ".edsk", ".hxcstream", ".raw", ".cdt", ".tzx", ".tsx",
            ".wav", ".mp3", ".flac", ".aac", ".m4a", ".wma", ".voc",
            ".cpr", ".rom", ".bin", ".tap"
        };
        foreach (var extension in required) Assert.Contains(extension, extensions);
    }

    [Fact]
    public void ImageSelectionFilterUsesTheRecognitionCatalog()
    {
        var extensions = MediaRecognitionComposition.CreateDefault().SupportedExtensions;

        var filter = DiskImageFileSelectionService.BuildFilter("Images|*.scp|All files|*.*", extensions);

        foreach (var extension in new[] { ".dsk", ".hxcstream", ".raw", ".cdt", ".wav", ".mp3", ".voc", ".cpr", ".rom", ".bin", ".tap" })
            Assert.Contains($"*{extension}", filter, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CpcDataDskDirectoryIsListedFromTheNeutralContainerFormat()
        => AssertCpcDirectoryIsListed(0x41, 0);

    [Fact]
    public void CpcSystemDskDirectoryIsListedFromTheNeutralContainerFormat()
        => AssertCpcDirectoryIsListed(0xc1, 2);

    private static void AssertCpcDirectoryIsListed(int firstSectorId, int reservedTracks)
    {
        const int cylinders = 40;
        const int sectorsPerTrack = 9;
        const int sectorSize = 512;
        var bytes = Enumerable.Repeat((byte)0xe5, cylinders * sectorsPerTrack * sectorSize).ToArray();
        var directoryOffset = reservedTracks * sectorsPerTrack * sectorSize;
        var entry = bytes.AsSpan(directoryOffset, 32);
        entry.Clear();
        entry[0] = 0;
        System.Text.Encoding.ASCII.GetBytes("HELLO   ", entry[1..9]);
        System.Text.Encoding.ASCII.GetBytes("BAS", entry[9..12]);
        entry[15] = 1;
        entry[16] = 2;
        bytes.AsSpan(directoryOffset + 2 * 1024, 128).Fill(0x41);
        var blocks = Enumerable.Range(0, cylinders * sectorsPerTrack)
            .Select(index => new SectorBlock(
                index,
                new SectorAddress(index / sectorsPerTrack, 0, firstSectorId + index % sectorsPerTrack),
                bytes.AsSpan(index * sectorSize, sectorSize).ToArray()))
            .ToArray();
        var image = new SectorImage(
            DiskImageFormatIds.CpcEmuDsk,
            sectorSize,
            cylinders,
            1,
            sectorsPerTrack,
            blocks);
        var reader = new AmstradCpmFileSystemReader();

        Assert.True(reader.CanRead(image));
        var file = Assert.Single(reader.Read(image).Entries);
        Assert.Equal("HELLO.BAS", file.Name);
        Assert.Equal(128, file.Size);
    }

    private static async Task<MediaImageDocument> ReadAsync(
        IMediaImageReader reader,
        string path,
        string? requestedFormatId = null)
    {
        var context = new MediaRecognitionContext(new MediaSourceDescriptor(path, [], RequestedFormatId: requestedFormatId));
        Assert.True(await reader.CanReadAsync(context, CancellationToken.None));
        return await reader.ReadAsync(context, CancellationToken.None);
    }

    private static MediaVolumeDescriptor Volume(MediaImageDocument document)
    {
        var volume = Assert.Single(document.Volumes);
        return new MediaVolumeDescriptor(volume.Start, volume.Length, volume.Origin, name: volume.Name);
    }

    private static byte[] CreateCpr()
    {
        const int bankLength = 16 * 1024;
        var bytes = new byte[12 + 2 * (8 + bankLength)];
        System.Text.Encoding.ASCII.GetBytes("RIFF", bytes);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), checked((uint)(bytes.Length - 8)));
        System.Text.Encoding.ASCII.GetBytes("AMS!", bytes.AsSpan(8));
        var offset = 12;
        for (var bank = 0; bank < 2; bank++)
        {
            System.Text.Encoding.ASCII.GetBytes($"cb{bank:D2}", bytes.AsSpan(offset));
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset + 4), bankLength);
            bytes.AsSpan(offset + 8, bankLength).Fill((byte)(bank + 1));
            offset += 8 + bankLength;
        }
        return bytes;
    }

}
