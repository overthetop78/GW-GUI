using GWGUI.MediaEngine.Containers.Epson.Raw;
using GWGUI.MediaEngine.Containers.ImageDisk;
using GWGUI.MediaEngine.Conversion.Epson;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.Geometries.Epson;
using GWGUI.MediaEngine.SectorImages;
using System.IO;

namespace GWGUI.Tests;

/// <summary>Vérifie les sorties internes Epson IMG et ImageDisk.</summary>
public sealed class EpsonQx10WriterTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.EpsonQx10_320)]
    [InlineData(DiskImageFormatIds.EpsonQx10_396)]
    [InlineData(DiskImageFormatIds.EpsonQx10_399)]
    [InlineData(DiskImageFormatIds.EpsonQx10_400)]
    [InlineData(DiskImageFormatIds.EpsonQx10Logo)]
    public async Task RawWriterRoundTripsEveryCataloguedConversionTarget(string formatId)
    {
        var source = CreateImage(formatId);
        var path = GeneratedPath($"{formatId}.img");

        await new EpsonQx10RawImageWriter().WriteAsync(source, path, formatId);
        var result = await new EpsonQx10RawImageReader().ReadAsync(path, formatId);

        Assert.Equal(source.Capacity, new FileInfo(path).Length);
        Assert.Equal(source.AvailableBlocks.SelectMany(block => block.Data), result.AvailableBlocks.SelectMany(block => block.Data));
    }

    [Fact]
    public async Task ImdWriterPreservesModesMapsSizesMissingDataAndRecordTypes()
    {
        var sourcePath = Path.Combine(FindImageTestRoot(), "_generated", "imd-all-modes-maps-records.imd");
        if (!File.Exists(sourcePath)) throw new FileNotFoundException("L'image IMD combinée de test est introuvable.", sourcePath);
        var reader = new ImdReader();
        var source = await reader.ReadDetailedAsync(sourcePath);
        var outputPath = GeneratedPath("imd-roundtrip.imd");

        await new ImdWriter().WriteAsync(source, outputPath);
        var result = await reader.ReadDetailedAsync(outputPath);

        Assert.Equal(source.Comment, result.Comment);
        Assert.Equal(source.Tracks.Count, result.Tracks.Count);
        for (var trackIndex = 0; trackIndex < source.Tracks.Count; trackIndex++)
        {
            var expectedTrack = source.Tracks[trackIndex];
            var actualTrack = result.Tracks[trackIndex];
            Assert.Equal((expectedTrack.Mode, expectedTrack.Cylinder, expectedTrack.Head), (actualTrack.Mode, actualTrack.Cylinder, actualTrack.Head));
            Assert.Equal(expectedTrack.Sectors.Count, actualTrack.Sectors.Count);
            for (var sectorIndex = 0; sectorIndex < expectedTrack.Sectors.Count; sectorIndex++)
            {
                var expectedSector = expectedTrack.Sectors[sectorIndex];
                var actualSector = actualTrack.Sectors[sectorIndex];
                Assert.Equal((expectedSector.Cylinder, expectedSector.Head, expectedSector.Number, expectedSector.Size, expectedSector.RecordType), (actualSector.Cylinder, actualSector.Head, actualSector.Number, actualSector.Size, actualSector.RecordType));
                Assert.Equal(expectedSector.Data, actualSector.Data);
            }
        }
        Assert.Equal(source.SectorImage.MissingBlocks, result.SectorImage.MissingBlocks);
    }

    [Theory]
    [InlineData(DiskImageFormatIds.EpsonQx10_320, ".img")]
    [InlineData(DiskImageFormatIds.EpsonQx10_396, ".imd")]
    [InlineData(DiskImageFormatIds.EpsonQx10Logo, ".img")]
    public void ConversionServiceAcceptsEpsonTargets(string formatId, string extension) => Assert.True(EpsonQx10ConversionService.CanCreate(formatId, extension));

    private static SectorImage CreateImage(string formatId)
    {
        var geometry = EpsonQx10GeometryCatalog.Resolve(formatId);
        var blocks = new List<SectorBlock>();
        var maximumSectors = 0;
        var sizes = new HashSet<int>();
        long capacity = 0;
        for (var cylinder = 0; cylinder < geometry.Cylinders; cylinder++)
        {
            for (var head = 0; head < geometry.Heads; head++)
            {
                var track = geometry.Track(cylinder, head);
                maximumSectors = Math.Max(maximumSectors, track.Count);
                if (track.Count > 0) sizes.Add(track.SectorSize);
                for (var index = 0; index < track.Count; index++)
                {
                    var data = Enumerable.Repeat(checked((byte)(blocks.Count % 251)), track.SectorSize).ToArray();
                    blocks.Add(new(blocks.Count, new(cylinder, head, track.FirstSector + index), data));
                    capacity += data.Length;
                }
            }
        }
        var blockSize = sizes.OrderByDescending(size => geometry.AllTracks.Where(track => track.SectorSize == size).Sum(track => track.Count)).First();
        return new(formatId, blockSize, geometry.Cylinders, geometry.Heads, maximumSectors, blocks, sizes.Count > 1, capacity, blocks.Count);
    }

    private static string GeneratedPath(string fileName)
    {
        var directory = Path.Combine(FindImageTestRoot(), "_generated", "epson-writer");
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, fileName);
    }

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
