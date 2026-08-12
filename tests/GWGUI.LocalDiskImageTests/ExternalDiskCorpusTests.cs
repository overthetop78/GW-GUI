using GWGUI.MediaEngine.Visualization;
using GWGUI.MediaEngine.Exploration;
using System.IO;
using GWGUI.MediaEngine.Containers.TeleDisk;
using Xunit.Abstractions;

namespace GWGUI.Tests;

public sealed class ExternalDiskCorpusTests(ITestOutputHelper output)
{
    [Fact]
    public async Task AgonyDiskOneReportsEveryRecognizedFileSystem()
        => await VerifyScp(@"F:\Disquettes\Agony Disk 1.scp", true);

    [Fact]
    public async Task SelectedExternalImageReportsEveryRecognizedFileSystem()
    {
        var path = Environment.GetEnvironmentVariable("GWGUI_TEST_IMAGE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), $"The selected external image does not exist: {path}");
        await VerifyScp(path, false);
    }

    [Fact]
    public async Task SelectedExternalImageCanBeVisualizedNatively()
    {
        var path = Environment.GetEnvironmentVariable("GWGUI_TEST_IMAGE");
        if (string.IsNullOrWhiteSpace(path)) return;
        Assert.True(File.Exists(path), $"The selected external image does not exist: {path}");
        var requestedFormat = Environment.GetEnvironmentVariable("GWGUI_TEST_FORMAT");
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path,
            string.IsNullOrWhiteSpace(requestedFormat) ? null : requestedFormat);
        var visualizer = new SectorImageFluxVisualizer();
        Assert.True(visualizer.CanVisualize(document.Image),
            $"No native visualizer encoder is registered for '{document.Image.FormatId}'.");
        var started = System.Diagnostics.Stopwatch.StartNew();
        var visualization = visualizer.Create(document.Image);
        started.Stop();
        Assert.NotEmpty(visualization.Tracks);
        output.WriteLine($"visualization={started.Elapsed.TotalSeconds:F3}s | format={document.Image.FormatId} | tracks={visualization.Tracks.Count}");
    }

    [Fact]
    public async Task LemmingsDataDiskExposesItsFlatResourceArchive()
    {
        const string path = @"F:\Disquettes\Lemmings Data Disk.scp";
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.True(document.FileSystemRecognized);
        Assert.Equal("amiga-flat-resource-archive", document.Volume.FileSystemId);
        Assert.Empty(document.Volume.Name);
        Assert.Equal(72, document.Volume.Entries.Count);
        var rampic = Assert.Single(document.Volume.Entries, entry => entry.Name == "rampic");
        Assert.Equal(8372, rampic.Size);
        Assert.Equal("FORM", System.Text.Encoding.ASCII.GetString(rampic.Content!.Take(4).ToArray()));
    }

    [Fact]
    public async Task SuperCarsTwoDiskTwoDoesNotExposeAnUnsignedEmptyRootAsOfs()
    {
        const string path = @"F:\Disquettes\Supercars II Disk 2.scp";
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.False(document.FileSystemRecognized);
        Assert.NotEmpty(document.Volume.Entries);
        Assert.All(document.Volume.Entries, entry => Assert.StartsWith("T", entry.Name));
    }

    [Fact]
    public async Task Generation4HybridDetectsAtariIbmAndAmigaWithoutInventingMsxVariants()
    {
        const string path = @"F:\Disquettes\GÃ©nÃ©ration 4\GÃ©nÃ©ration 4 NÂ°53 - Mars 1993\GÃ©nÃ©ration 4 - Disquette_Demo_NÂ°53.scp";
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        var formats = (document.DetectedFileSystems ?? []).Select(item => item.FormatId).ToArray();
        Assert.Contains(formats, format => format.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(formats, format => format.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(formats, format => format.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(formats, format => format.StartsWith("msx.", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Generation4Number37ReportsOnlyAmigaAndAtari()
    {
        const string path = @"F:\Disquettes\Génération 4\Génération 4 N°37- Octobre 1991\Génération 4 N°37- Octobre 1991.scp";
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.Equal(["amiga", "atari-st"], document.Metadata.SystemIds);
        Assert.DoesNotContain(document.Metadata.SystemIds, system => system is "acorn-bbc" or "amstrad" or "ibm-pc" or "commodore" or "epson-qx10");
    }

    [Theory]
    [InlineData(@"F:\Disquettes\Tilt\Tilt N°105\Tilt N°105 - Septembre 1992.scp")]
    [InlineData(@"F:\Disquettes\Tilt\Tilt N°110\Tilt N°110 - Janvier 1993.scp")]
    [InlineData(@"F:\Disquettes\Tilt\Tilt N°117\Tilt N°117 - Septembre 1993.scp")]
    public async Task TiltHybridCorpusDoesNotReportGeometryAliasesAsSystems(string path)
    {
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.DoesNotContain(document.Metadata.SystemIds, system => system is "acorn-bbc" or "amstrad" or "ibm-pc" or "commodore" or "epson-qx10");
    }

    private async Task VerifyScp(string path, bool requireRecognized)
    {
        if (!File.Exists(path)) return;
        if (Path.GetExtension(path).Equals(".td0", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var td0 = await new Td0Reader().ReadAsync(path);
                output.WriteLine($"TD0 reader | format={td0.FormatId} | geometry={td0.Cylinders}x{td0.Heads}x{td0.SectorsPerTrack} | available={td0.AvailableBlocks.Count}");
            }
            catch (Exception exception)
            {
                output.WriteLine($"TD0 reader failed | {exception.GetType().Name}: {exception.Message}");
            }
        }
        var requestedFormat = Environment.GetEnvironmentVariable("GWGUI_TEST_FORMAT");
        var started = System.Diagnostics.Stopwatch.StartNew();
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path, string.IsNullOrWhiteSpace(requestedFormat) ? null : requestedFormat);
        started.Stop();
        output.WriteLine($"analysis={started.Elapsed.TotalSeconds:F3}s");
        output.WriteLine($"{Path.GetFileName(path)} | primary={document.Image.FormatId} | systems={string.Join(',', document.Metadata.SystemIds)} | protection={document.Metadata.ProtectionId ?? "-"} | recognized={document.FileSystemRecognized} | geometry={document.Image.Cylinders}x{document.Image.Heads}x{document.Image.SectorsPerTrack} | available={document.Image.AvailableBlocks.Count} | missing={document.Image.MissingBlocks.Count}");
        if (document.Image.MissingBlocks.Count > 0)
            output.WriteLine($"missing-blocks={string.Join(',', document.Image.MissingBlocks.Take(30))}");
        if (document.Image.TryGetBlock(0, out var boot)) output.WriteLine($"boot={Convert.ToHexString(boot.Data.Take(16).ToArray())}");
        var rootBlock = document.Image.BlockCount / 2;
        if (document.Image.TryGetBlock(rootBlock, out var root)) output.WriteLine($"root[{rootBlock}]={Convert.ToHexString(root.Data.Take(24).ToArray())}");
        foreach (var detected in document.DetectedFileSystems ?? [])
        {
            output.WriteLine($"{detected.FormatId} | {detected.Volume.FileSystemId} | volume={detected.Volume.Name} | entries={Count(detected.Volume.Entries)} | warnings={detected.Volume.Warnings.Count}");
            foreach (var entry in detected.Volume.Entries.Take(30)) output.WriteLine($"  entry: {entry.Name} | {entry.Size} | {entry.Comment}");
            foreach (var warning in detected.Volume.Warnings.Take(10)) output.WriteLine($"  warning: {warning}");
        }
        if (requireRecognized)
        {
            Assert.True(document.FileSystemRecognized);
            Assert.NotEmpty(document.DetectedFileSystems ?? []);
        }
    }

    private static int Count(IEnumerable<GWGUI.MediaEngine.FileSystems.FileSystemEntry> entries)
        => entries.Sum(entry => 1 + Count(entry.Children));
}
