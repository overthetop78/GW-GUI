using GWGUI.MediaEngine.Visualization;
using GWGUI.MediaEngine.Exploration;
using System.IO;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Reconstruction.Commodore;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

public sealed class SectorImageFluxVisualizerTests
{
    [Theory]
    [InlineData("amiga.amigados", "amiga.mfm", 512, 11)]
    [InlineData("atarist.720", "iso.mfm", 512, 9)]
    [InlineData("ibm.1440", "iso.mfm", 512, 18)]
    [InlineData("atari.90", "iso.fm", 128, 18)]
    [InlineData("commodore.1541", "commodore.gcr", 256, 21)]
    [InlineData("acorn.dfs.ss80", "iso.fm", 256, 10)]
    public void CreatesDecodableNativeVisualization(string formatId, string decoderId, int blockSize, int sectorCount)
    {
        var blocks = Enumerable.Range(0, sectorCount).Select(sector => new SectorBlock(sector,
            new SectorAddress(0, 0, sector + (decoderId.StartsWith("iso", StringComparison.Ordinal) ? 1 : 0)),
            Enumerable.Repeat((byte)(sector + 1), blockSize).ToArray())).ToArray();
        var image = new SectorImage(formatId, blockSize, 1, 1, sectorCount, blocks);

        var visualization = new SectorImageFluxVisualizer().Create(image);
        var decoded = new FluxDecoderRegistry().Decode(decoderId, visualization.Tracks.Single().Revolutions.Single().Flux);

        Assert.Single(visualization.Tracks);
        Assert.NotEmpty(decoded.Sectors);
    }

    [Fact]
    public void SplitsAppleProDosBlocksForAppleGcrVisualization()
    {
        var blocks = Enumerable.Range(0, 8).Select(block => new SectorBlock(block, new SectorAddress(0, 0, block),
            Enumerable.Repeat((byte)block, 512).ToArray())).ToArray();
        var image = new SectorImage("apple2.prodos", 512, 1, 1, 8, blocks);

        var visualization = new SectorImageFluxVisualizer().Create(image);
        var decoded = new FluxDecoderRegistry().Decode("apple2.gcr", visualization.Tracks.Single().Revolutions.Single().Flux);

        Assert.Equal(16, decoded.Sectors.Count);
    }

    [Fact]
    public void SplitsAppleSosBlocksForAppleGcrVisualization()
    {
        var blocks = Enumerable.Range(0, 8).Select(block => new SectorBlock(block, new SectorAddress(0, 0, block),
            Enumerable.Repeat((byte)block, 512).ToArray())).ToArray();
        var image = new SectorImage("apple3.sos", 512, 1, 1, 8, blocks);

        var visualization = new SectorImageFluxVisualizer().Create(image);
        var decoded = new FluxDecoderRegistry().Decode("apple2.gcr", visualization.Tracks.Single().Revolutions.Single().Flux);

        Assert.Equal(16, decoded.Sectors.Count);
    }

    [Fact]
    public void SplitsRt11BlocksIntoPhysicalRx02Sectors()
    {
        var blocks = Enumerable.Range(0, 13).Select(block => new SectorBlock(block, new SectorAddress(0, 0, block + 1),
            Enumerable.Repeat((byte)block, 512).ToArray())).ToArray();
        var image = new SectorImage("dec.rx02", 512, 1, 1, 13, blocks);

        var visualization = new SectorImageFluxVisualizer().Create(image);
        var decoded = new FluxDecoderRegistry().Decode("dec.rx02", visualization.Tracks.Single().Revolutions.Single().Flux);

        Assert.Equal(26, decoded.Sectors.Count);
        Assert.All(decoded.Sectors, sector => Assert.True(sector.IntegrityValid));
    }

    [Theory]
    [InlineData("Apple II", ".2mg")]
    [InlineData("Apple Lisa", ".dc42")]
    public async Task RealAppleContainerCanBeVisualizedNatively(string directory, string extension)
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", directory));
        if (!Directory.Exists(root)) return;
        var path = Directory.EnumerateFiles(root, $"*{extension}", SearchOption.AllDirectories).FirstOrDefault();
        if (path is null) return;

        var image = (await DiskImageExplorer.CreateDefault().ExploreAsync(path)).Image;
        var visualization = new SectorImageFluxVisualizer().Create(image);

        Assert.NotEmpty(visualization.Tracks);
        Assert.All(visualization.Tracks, track => Assert.NotEmpty(track.Revolutions[0].FluxIntervals));
    }

    [Fact]
    public async Task RealLisaFileWareDiskUsesZonedDoubleSidedGeometryAndDedicatedCodec()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "Apple Lisa"));
        var path = Directory.Exists(root) ? Directory.EnumerateFiles(root, "*.dc42", SearchOption.AllDirectories)
            .FirstOrDefault(file => new FileInfo(file).Length > 871_424) : null;
        if (path is null) return;

        var image = (await DiskImageExplorer.CreateDefault().ExploreAsync(path)).Image;
        var visualization = new SectorImageFluxVisualizer().Create(image);
        var first = new FluxDecoderRegistry().Decode("applelisa.fileware.gcr", visualization.Tracks[0].Revolutions[0].Flux);
        var last = new FluxDecoderRegistry().Decode("applelisa.fileware.gcr", visualization.Tracks[^1].Revolutions[0].Flux);

        Assert.Equal(46, image.Cylinders);
        Assert.Equal(2, image.Heads);
        Assert.Equal(92, visualization.Tracks.Count);
        Assert.Equal(22, first.Sectors.Count);
        Assert.Equal(15, last.Sectors.Count);
        Assert.All(first.Sectors, sector => Assert.True(sector.IntegrityValid));
        Assert.All(last.Sectors, sector => Assert.True(sector.IntegrityValid));
    }

    [Fact]
    public async Task RealMacintosh400kDiskUsesFiveZonedGcrGeometries()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test", "Apple Macintosh"));
        var path = Directory.Exists(root) ? Directory.EnumerateFiles(root, "*.dsk", SearchOption.AllDirectories)
            .FirstOrDefault(file => new FileInfo(file).Length == 409_600) : null;
        if (path is null) return;

        var image = (await DiskImageExplorer.CreateDefault().ExploreAsync(path)).Image;
        var visualization = new SectorImageFluxVisualizer().Create(image);
        var decoders = new FluxDecoderRegistry();
        var first = decoders.Decode("applemac.gcr", visualization.Tracks[0].Revolutions[0].Flux);
        var last = decoders.Decode("applemac.gcr", visualization.Tracks[^1].Revolutions[0].Flux);

        Assert.Equal(80, visualization.Tracks.Count);
        Assert.Equal(12, first.Sectors.Count);
        Assert.Equal(8, last.Sectors.Count);
        Assert.All(first.Sectors, sector => Assert.True(sector.IntegrityValid));
        Assert.All(last.Sectors, sector => Assert.True(sector.IntegrityValid));
    }

    [Fact]
    public async Task RealD81UsesPhysicalDoubleSidedMfmGeometryForVisualization()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test"));
        var path = Directory.Exists(root) ? Directory.EnumerateFiles(root, "*.d81", SearchOption.AllDirectories).FirstOrDefault() : null;
        if (path is null) return;

        var image = (await DiskImageExplorer.CreateDefault().ExploreAsync(path)).Image;
        var visualization = new SectorImageFluxVisualizer().Create(image);
        var decoded = new FluxDecoderRegistry().Decode("iso.mfm", visualization.Tracks[0].Revolutions[0].Flux);

        Assert.Equal(160, visualization.Tracks.Count);
        Assert.Equal(10, decoded.Sectors.Count);
        Assert.Contains(visualization.Tracks, track => track.Cylinder == 79 && track.Head == 1);
    }

    [Fact]
    public async Task Commodore1581ScpDecodePreservesBothPhysicalSidesForVisualization()
    {
        var blocks = Enumerable.Range(0, 3_200).Select(logical => new SectorBlock(logical,
            new SectorAddress(logical / 40, 0, logical % 40),
            Enumerable.Repeat((byte)logical, 256).ToArray())).ToArray();
        var source = new SectorImage("commodore.1581", 256, 80, 1, 40, blocks);
        var scp = new SectorImageFluxVisualizer().Create(source);
        var decoded = await new CommodoreScpSectorImageReader(new FixedScpReader(scp), new FluxDecoderRegistry())
            .ReadAsync("unused.scp", "commodore.1581");

        var visualization = new SectorImageFluxVisualizer().Create(decoded);

        Assert.Equal(160, visualization.Tracks.Count);
        Assert.Contains(visualization.Tracks, track => track.Cylinder == 79 && track.Head == 1);
    }

    [Fact]
    public async Task RealAtrUsesPhysicalTrackGeometryForVisualization()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test"));
        SectorImage? image = null;
        if (Directory.Exists(root))
            foreach (var path in Directory.EnumerateFiles(root, "*.atr", SearchOption.AllDirectories))
            {
                var candidate = (await DiskImageExplorer.CreateDefault().ExploreAsync(path)).Image;
                if (!candidate.FormatId.Equals("atari.90", StringComparison.OrdinalIgnoreCase)) continue;
                image = candidate;
                break;
            }
        if (image is null) return;

        var visualization = new SectorImageFluxVisualizer().Create(image);
        var decoded = new FluxDecoderRegistry().Decode("iso.fm", visualization.Tracks[0].Revolutions[0].Flux);

        Assert.Equal(40, visualization.Tracks.Count);
        Assert.Equal(18, decoded.Sectors.Count);
    }

    [Fact]
    public async Task RealCommodore900ImageUsesZonedGcrGeometry()
    {
        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "image_test"));
        var path = Directory.Exists(root) ? Directory.EnumerateFiles(root, "*.bin", SearchOption.AllDirectories)
            .FirstOrDefault(file => file.Contains("COHERENT", StringComparison.OrdinalIgnoreCase)) : null;
        if (path is null) return;

        var image = (await DiskImageExplorer.CreateDefault().ExploreAsync(path)).Image;
        var visualization = new SectorImageFluxVisualizer().Create(image);
        var first = new FluxDecoderRegistry().Decode("commodore900.gcr", visualization.Tracks[0].Revolutions[0].Flux);
        var last = new FluxDecoderRegistry().Decode("commodore900.gcr", visualization.Tracks[^1].Revolutions[0].Flux);

        Assert.Equal(80, image.Cylinders);
        Assert.Equal(2, image.Heads);
        Assert.Empty(image.MissingBlocks);
        Assert.Equal(16, first.Sectors.Count);
        Assert.Equal(13, last.Sectors.Count);
        Assert.All(first.Sectors, sector => Assert.True(sector.IntegrityValid));
        Assert.All(last.Sectors, sector => Assert.True(sector.IntegrityValid));
    }

    private sealed class FixedScpReader(ScpImage image) : IScpReader
    {
        public Task<ScpImage> ReadAsync(string path, CancellationToken cancellationToken = default)
            => Task.FromResult(image);
    }
}
