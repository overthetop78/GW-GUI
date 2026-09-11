using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Formats.Floppy.Atr;
using GWGUI.MediaEngine.Formats.Floppy.Msa;
using GWGUI.MediaEngine.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Representations.Flux;
using GWGUI.MediaEngine.Representations.Sectors;

namespace GWGUI.Tests.MediaEngine.Reading;

public sealed class FloppyMediaReadingTests
{
    [Fact]
    public async Task DefaultReadersKeepFluxAndSectorImagesDistinct()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-media-reading-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var paths = await CreateImagesAsync(directory);
            var engine = MediaEngineComposition.CreateDefault();

            foreach (var path in paths.SectorImages)
            {
                var document = await engine.ReadingService.ReadAsync(new MediaSourceDescriptor(path, []));
                Assert.Equal(MediaKind.Floppy, document.MediaKind);
                Assert.IsType<SectorMediaImageRepresentation>(document.Representation);
            }

            var flux = await engine.ReadingService.ReadAsync(new MediaSourceDescriptor(paths.Scp, []));
            Assert.Equal(MediaKind.Floppy, flux.MediaKind);
            Assert.IsType<FluxMediaImageRepresentation>(flux.Representation);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static async Task<(IReadOnlyList<string> SectorImages, string Scp)> CreateImagesAsync(string directory)
    {
        var adf = Path.Combine(directory, "disk.adf");
        var atr = Path.Combine(directory, "disk.atr");
        var st = Path.Combine(directory, "disk.st");
        var msa = Path.Combine(directory, "disk.msa");
        var ima = Path.Combine(directory, "disk.ima");
        var scp = Path.Combine(directory, "disk.scp");

        await File.WriteAllBytesAsync(adf, new byte[80 * 2 * 11 * 512]);
        await File.WriteAllBytesAsync(st, new byte[80 * 2 * 9 * 512]);
        await File.WriteAllBytesAsync(ima, new byte[80 * 2 * 9 * 512]);
        await new AtrWriter().WriteAsync(CreateSectorImage(DiskImageFormatIds.Atari90, 128, 40, 1, 18), atr, DiskImageFormatIds.Atari90);
        await new MsaWriter().WriteAsync(CreateSectorImage(DiskImageFormatIds.AtariSt720, 512, 80, 2, 9), msa);

        var header = new ScpHeader(
            0x24,
            0,
            1,
            0,
            0,
            ScpFlags.IndexAligned,
            ScpBitCellEncoding.Default16Bit,
            ScpHeadSelection.Side0,
            0,
            0);
        var revolution = new ScpRevolution(4_000_000, 0, [100, 200, 300]);
        var track = new ScpTrack(0, 0, 0, [revolution]);
        await new ScpWriter().WriteAsync(scp, new ScpImage(header, [track], true, 0));

        return ([adf, atr, st, msa, ima], scp);
    }

    private static SectorImage CreateSectorImage(
        string formatId,
        int blockSize,
        int cylinders,
        int heads,
        int sectorsPerTrack)
    {
        var blockCount = cylinders * heads * sectorsPerTrack;
        var blocks = Enumerable.Range(0, blockCount)
            .Select(logical => new SectorBlock(
                logical,
                new SectorAddress(
                    logical / (heads * sectorsPerTrack),
                    logical / sectorsPerTrack % heads,
                    logical % sectorsPerTrack + 1),
                new byte[blockSize]))
            .ToArray();
        return new SectorImage(formatId, blockSize, cylinders, heads, sectorsPerTrack, blocks);
    }
}
