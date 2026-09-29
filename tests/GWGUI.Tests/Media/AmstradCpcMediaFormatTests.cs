using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats.Cartridge.AmstradCpr;
using GWGUI.MediaEngine.Images.Formats.Cartridge.AmstradRom;
using GWGUI.MediaEngine.Images.Formats.Cartridge.Console;
using GWGUI.MediaEngine.Images.Formats.Floppy.FamicomFds;
using GWGUI.MediaEngine.Images.Formats.Optical.Gdi;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Images.Formats.Tape;
using GWGUI.MediaEngine.Images.Conversion;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Images.Reading.Decoding.Sequential.Amstrad;
using GWGUI.MediaEngine.Images.Reading.Recognition;
using GWGUI.MediaEngine.Images.Writing;
using GWGUI.MediaEngine.Images.Writing.Encoding.Sequential.Amstrad;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Models.Blocks;
using GWGUI.MediaEngine.Images.Visualization;
using GWGUI.MediaEngine.Interfaces.Reading;
using GWGUI.App.Services.DiskImages.Selection;
using GWGUI.App.Presenters.Conversion;
using GWGUI.MediaFileSystems.FileSystems.Amstrad.Cartridge;
using GWGUI.MediaFileSystems.FileSystems.Console.Cartridge;
using GWGUI.MediaFileSystems.FileSystems.Nintendo.FamicomDisk;
using GWGUI.MediaFileSystems.Constants;
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
    public async Task ConsoleCartridgeFormatsRoundTripWithNamedBanks()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-console-cartridge-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var sourcePath = Path.Combine(directory, "game.nes");
            var outputPath = Path.Combine(directory, "roundtrip.nes");
            var source = Enumerable.Range(0, 16 * 1024 + 17)
                .Select(value => (byte)(value % byte.MaxValue)).ToArray();
            await File.WriteAllBytesAsync(sourcePath, source);

            var document = await ReadAsync(new ConsoleCartridgeReader(), sourcePath);
            Assert.Equal(DiskImageFormatIds.NintendoNes, document.FormatId);
            Assert.Equal("2", document.Metadata[ConsoleCartridgeMetadataConstants.BankCount]);
            Assert.Equal("16384", document.Metadata[ConsoleCartridgeMetadataConstants.BankSize]);
            var blocks = Assert.IsType<BlockMediaImageRepresentation>(document.Representation);
            Assert.Equal(2, blocks.Ranges.Count);

            var explorer = new ConsoleCartridgeFileSystemReader();
            var entries = explorer.Read(document, Volume(document)).Entries;
            Assert.Equal(["bank00", "bank01"], entries.Select(entry => entry.Name));
            Assert.All(entries, entry => Assert.Equal(
                ConsoleCartridgeMetadataConstants.BankEntryType, entry.NativeTypeId));
            Assert.Equal(source.Length - (16 * 1024), entries[1].Size);

            await new ConsoleCartridgeWriter().WriteAsync(document, outputPath,
                DiskImageFormatIds.NintendoNes);
            Assert.Equal(source, await File.ReadAllBytesAsync(outputPath));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task NintendoGameWatchCartridgeFormatRoundTripsAndExploresBanks()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-gamewatch-cartridge-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var sourcePath = Path.Combine(directory, "game.mgw");
            var outputPath = Path.Combine(directory, "roundtrip.mgw");
            var source = Enumerable.Repeat((byte)0x5a, 16 * 1024 + 3).ToArray();
            await File.WriteAllBytesAsync(sourcePath, source);

            var document = await ReadAsync(new ConsoleCartridgeReader(), sourcePath);
            Assert.Equal(DiskImageFormatIds.NintendoGameWatch, document.FormatId);
            var explorer = new ConsoleCartridgeFileSystemReader();
            var entries = explorer.Read(document, Volume(document)).Entries;
            Assert.Equal(2, entries.Count);
            Assert.Equal(source.Length - 16 * 1024, entries[1].Size);

            await new ConsoleCartridgeWriter().WriteAsync(document, outputPath,
                DiskImageFormatIds.NintendoGameWatch);
            Assert.Equal(source, await File.ReadAllBytesAsync(outputPath));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Nintendo3DsFormatsRoundTripAndExposeAllCitraExtensions()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-citra-cartridge-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var source = Enumerable.Range(0, 16 * 1024 + 5)
                .Select(value => (byte)(value % byte.MaxValue)).ToArray();
            foreach (var extension in new[] { ".3ds", ".3dsx", ".elf", ".axf", ".cci", ".cxi", ".app" })
            {
                var sourcePath = Path.Combine(directory, $"game{extension}");
                var outputPath = Path.Combine(directory, $"roundtrip{extension}");
                await File.WriteAllBytesAsync(sourcePath, source);
                var document = await ReadAsync(new ConsoleCartridgeReader(), sourcePath);
                Assert.Equal(DiskImageFormatIds.Nintendo3Ds, document.FormatId);
                await new ConsoleCartridgeWriter().WriteAsync(document, outputPath,
                    DiskImageFormatIds.Nintendo3Ds);
                Assert.Equal(source, await File.ReadAllBytesAsync(outputPath));
            }

            var supported = MediaRecognitionComposition.CreateDefault().SupportedExtensions;
            Assert.All(new[] { ".3ds", ".3dsx", ".elf", ".axf", ".cci", ".cxi", ".app" },
                extension => Assert.Contains(extension, supported));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task SegaMyCardAndNecSuperGrafxFormatsRoundTripAndExploreBanks()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-card-formats-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var source = Enumerable.Range(0, 16 * 1024 + 7)
                .Select(value => (byte)(value % byte.MaxValue)).ToArray();
            var engine = MediaEngineComposition.CreateDefault();
            var cases = new[]
            {
                (Extension: DiskImageFileExtensions.Sms, FormatId: DiskImageFormatIds.SegaMasterSystem),
                (Extension: DiskImageFileExtensions.Sg, FormatId: DiskImageFormatIds.SegaSg1000),
                (Extension: DiskImageFileExtensions.Mv, FormatId: DiskImageFormatIds.SegaSg1000),
                (Extension: DiskImageFileExtensions.Md, FormatId: DiskImageFormatIds.SegaMegaDrive),
                (Extension: DiskImageFileExtensions.Mdx, FormatId: DiskImageFormatIds.SegaMegaDrive),
                (Extension: DiskImageFileExtensions.Sgd, FormatId: DiskImageFormatIds.SegaMegaDrive),
                (Extension: DiskImageFileExtensions.Smd, FormatId: DiskImageFormatIds.SegaMegaDrive),
                (Extension: DiskImageFileExtensions.Bms, FormatId: DiskImageFormatIds.SegaMegaDrive),
                (Extension: DiskImageFileExtensions.SixtyEightK, FormatId: DiskImageFormatIds.SegaMegaDrive),
                (Extension: DiskImageFileExtensions.Gen, FormatId: DiskImageFormatIds.SegaMegaDrive),
                (Extension: DiskImageFileExtensions.Gg, FormatId: DiskImageFormatIds.SegaGameGear),
                (Extension: DiskImageFileExtensions.ThirtyTwoX, FormatId: DiskImageFormatIds.SegaThirtyTwoX),
                (Extension: DiskImageFileExtensions.Pce, FormatId: DiskImageFormatIds.NecPcEngine),
                (Extension: DiskImageFileExtensions.Sgx, FormatId: DiskImageFormatIds.NecSuperGrafx)
            };
            foreach (var item in cases)
            {
                var sourcePath = Path.Combine(directory, $"game{item.Extension}");
                var outputPath = Path.Combine(directory, $"roundtrip{item.Extension}");
                await File.WriteAllBytesAsync(sourcePath, source);

                var document = await ReadAsync(new ConsoleCartridgeReader(), sourcePath);
                Assert.Equal(item.FormatId, document.FormatId);
                var visualization = MediaVisualizationComposition.CreateDefault().Registry
                    .CreateDescriptor(document);
                Assert.Equal(MediaRepresentationKind.Blocks, visualization.RepresentationKind);
                Assert.Equal(MediaVisualizationProgressUnit.BlockRange, visualization.ProgressUnit);
                Assert.True(visualization.Elements.Count >= 2);
                var blocks = Assert.IsType<BlockMediaImageRepresentation>(document.Representation);
                Assert.Equal(blocks.Capacity,
                    visualization.Elements.Sum(element => element.Length));
                var explorer = new ConsoleCartridgeFileSystemReader();
                Assert.Equal(2, explorer.Read(document, Volume(document)).Entries.Count);

                var conversion = await engine.ConversionService.ConvertAsync(
                    new MediaConversionRequest(document, outputPath, item.FormatId));
                Assert.Equal([outputPath], conversion.ProducedFiles);
                Assert.Equal(source, await File.ReadAllBytesAsync(outputPath));
            }

            var supported = MediaRecognitionComposition.CreateDefault().SupportedExtensions;
            Assert.Contains(DiskImageFileExtensions.Sms, supported);
            Assert.Contains(DiskImageFileExtensions.Sg, supported);
            Assert.Contains(DiskImageFileExtensions.Mv, supported);
            Assert.Contains(DiskImageFileExtensions.Md, supported);
            Assert.Contains(DiskImageFileExtensions.Mdx, supported);
            Assert.Contains(DiskImageFileExtensions.Sgd, supported);
            Assert.Contains(DiskImageFileExtensions.Smd, supported);
            Assert.Contains(DiskImageFileExtensions.Bms, supported);
            Assert.Contains(DiskImageFileExtensions.SixtyEightK, supported);
            Assert.Contains(DiskImageFileExtensions.Gen, supported);
            Assert.Contains(DiskImageFileExtensions.Gg, supported);
            Assert.Contains(DiskImageFileExtensions.ThirtyTwoX, supported);
            Assert.Contains(DiskImageFileExtensions.Pce, supported);
            Assert.Contains(DiskImageFileExtensions.Sgx, supported);
            var megaDrive = new BuiltInImageFormatCatalog().Formats.Single(format =>
                format.Id == DiskImageFormatIds.SegaMegaDrive);
            Assert.Equal(
                [DiskImageFileExtensions.Md, DiskImageFileExtensions.Mdx,
                 DiskImageFileExtensions.Sgd, DiskImageFileExtensions.Smd,
                 DiskImageFileExtensions.Bms, DiskImageFileExtensions.SixtyEightK,
                 DiskImageFileExtensions.Gen],
                megaDrive.Extensions.Select(extension => extension.Extension));
            Assert.Contains(MediaImageWriterIds.ConsoleCartridge,
                MediaWritingComposition.CreateDefault().Writers.Select(writer => writer.Id));
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task FamicomDiskSystemFacesRoundTripAndExposeFiles()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-fds-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var sourcePath = Path.Combine(directory, "game.fds");
            var outputPath = Path.Combine(directory, "roundtrip.fds");
            var source = CreateFds();
            await File.WriteAllBytesAsync(sourcePath, source);

            var document = await ReadAsync(new FamicomFdsReader(), sourcePath);
            Assert.Equal(DiskImageFormatIds.NintendoFamicomDisk, document.FormatId);
            var blocks = Assert.IsType<BlockMediaImageRepresentation>(document.Representation);
            Assert.Equal(1, blocks.LogicalBlockCount);
            var explorer = new FamicomDiskFileSystemReader();
            var file = Assert.Single(explorer.Read(document, Volume(document)).Entries);
            Assert.Equal("HELLO", file.Name);
            Assert.Equal(5, file.Size);
            Assert.Equal("0", file.Metadata["side"]);

            await new FamicomFdsWriter().WriteAsync(document, outputPath,
                DiskImageFormatIds.NintendoFamicomDisk);
            Assert.Equal(source, await File.ReadAllBytesAsync(outputPath));
            Assert.Contains(DiskImageFileExtensions.Fds,
                MediaRecognitionComposition.CreateDefault().SupportedExtensions);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task DreamcastGdiTracksRoundTripWithAssociatedFiles()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-gdi-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var descriptorPath = Path.Combine(directory, "game.gdi");
            var trackOnePath = Path.Combine(directory, "track01.bin");
            var trackTwoPath = Path.Combine(directory, "track02.raw");
            var outputPath = Path.Combine(directory, "roundtrip.gdi");
            await File.WriteAllBytesAsync(trackOnePath, Enumerable.Repeat((byte)0x11, 100 * 2048).ToArray());
            await File.WriteAllBytesAsync(trackTwoPath, Enumerable.Repeat((byte)0x22, 2 * 2352).ToArray());
            await File.WriteAllTextAsync(
                descriptorPath,
                "2\n1 0 4 2048 \"track01.bin\" 0\n2 100 0 2352 \"track02.raw\" 0\n");

            var document = await ReadAsync(new GdiReader(), descriptorPath);
            Assert.Equal(OpticalImageFormatIds.Gdi, document.FormatId);
            var optical = Assert.IsType<GWGUI.MediaEngine.Images.Models.Optical.OpticalMediaImageRepresentation>(document.Representation);
            Assert.Equal(2, optical.Tracks!.Count);
            Assert.Equal(100, optical.Tracks[0].SectorCount);
            Assert.Equal(OpticalTrackMode.Audio, optical.Tracks[1].Mode);
            Assert.Equal(2, optical.Tracks[1].SectorCount);

            await new GdiWriter().WriteAsync(document, outputPath, OpticalImageFormatIds.Gdi);
            var roundtrip = await ReadAsync(new GdiReader(), outputPath);
            var roundtripOptical = Assert.IsType<GWGUI.MediaEngine.Images.Models.Optical.OpticalMediaImageRepresentation>(roundtrip.Representation);
            Assert.Equal(optical.Tracks.Select(track => (track.TrackNumber, track.FirstSector, track.SectorCount, track.Mode)),
                roundtripOptical.Tracks!.Select(track => (track.TrackNumber, track.FirstSector, track.SectorCount, track.Mode)));
            Assert.Contains(DiskImageFileExtensions.Gdi, MediaRecognitionComposition.CreateDefault().SupportedExtensions);
            Assert.Contains(MediaImageWriterIds.OpticalGdi,
                MediaWritingComposition.CreateDefault().Writers.Select(writer => writer.Id));
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
    public void DefaultRecognitionPublishesOpticalDescriptorExtensions()
    {
        var extensions = MediaRecognitionComposition.CreateDefault().SupportedExtensions;

        foreach (var extension in new[] { ".cue", ".ccd", ".mds", ".chd" })
            Assert.Contains(extension, extensions);
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

    private static byte[] CreateFds()
    {
        const int headerLength = 16;
        const int sideLength = 65_500;
        var source = new byte[headerLength + sideLength];
        System.Text.Encoding.ASCII.GetBytes("FDS\x1A", source);
        source[4] = 1;
        var side = source.AsSpan(headerLength, sideLength);
        side[0] = 1;
        System.Text.Encoding.ASCII.GetBytes("NINTENDO-HVC", side[1..13]);
        side[56] = 2;
        side[57] = 1;
        var fileHeader = side[58..76];
        fileHeader[0] = 3;
        fileHeader[1] = 0;
        System.Text.Encoding.ASCII.GetBytes("HVC", fileHeader[2..5]);
        System.Text.Encoding.ASCII.GetBytes("HELLO", fileHeader[5..10]);
        BinaryPrimitives.WriteUInt16LittleEndian(fileHeader[13..15], 0x8000);
        BinaryPrimitives.WriteUInt16LittleEndian(fileHeader[15..17], 5);
        fileHeader[17] = 2;
        side[76] = 4;
        System.Text.Encoding.ASCII.GetBytes("HELLO", side[77..82]);
        return source;
    }

}
