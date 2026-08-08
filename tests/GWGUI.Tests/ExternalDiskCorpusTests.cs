using System.IO;
using GWGUI.Scp.Images;
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
    public async Task Generation4HybridDetectsAtariIbmAndAmigaWithoutInventingMsxVariants()
    {
        const string path = @"F:\Disquettes\Génération 4\Génération 4 N°53 - Mars 1993\Génération 4 - Disquette_Demo_N°53.scp";
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        var formats = (document.DetectedFileSystems ?? []).Select(item => item.FormatId).ToArray();
        Assert.Contains(formats, format => format.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(formats, format => format.StartsWith("ibm.", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(formats, format => format.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(formats, format => format.StartsWith("msx.", StringComparison.OrdinalIgnoreCase));
    }

    private async Task VerifyScp(string path, bool requireRecognized)
    {
        if (!File.Exists(path)) return;
        if (Path.GetExtension(path).Equals(".td0", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var td0 = await new Td0ImageReader().ReadAsync(path);
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
        output.WriteLine($"{Path.GetFileName(path)} | primary={document.Image.FormatId} | recognized={document.FileSystemRecognized} | geometry={document.Image.Cylinders}x{document.Image.Heads}x{document.Image.SectorsPerTrack} | available={document.Image.AvailableBlocks.Count} | missing={document.Image.MissingBlocks.Count}");
        if (document.Image.MissingBlocks.Count > 0)
            output.WriteLine($"missing-blocks={string.Join(',', document.Image.MissingBlocks.Take(30))}");
        if (document.Image.TryGetBlock(0, out var boot)) output.WriteLine($"boot={Convert.ToHexString(boot.Data.Take(16).ToArray())}");
        var rootBlock = document.Image.BlockCount / 2;
        if (document.Image.TryGetBlock(rootBlock, out var root)) output.WriteLine($"root[{rootBlock}]={Convert.ToHexString(root.Data.Take(24).ToArray())}");
        foreach (var detected in document.DetectedFileSystems ?? [])
        {
            output.WriteLine($"{detected.FormatId} | {detected.Volume.FileSystem} | volume={detected.Volume.Name} | entries={Count(detected.Volume.Entries)} | warnings={detected.Volume.Warnings.Count}");
            foreach (var entry in detected.Volume.Entries.Take(30)) output.WriteLine($"  entry: {entry.Name} | {entry.Size} | {entry.Comment}");
            foreach (var warning in detected.Volume.Warnings.Take(10)) output.WriteLine($"  warning: {warning}");
        }
        if (requireRecognized)
        {
            Assert.True(document.FileSystemRecognized);
            Assert.NotEmpty(document.DetectedFileSystems ?? []);
        }
    }

    private static int Count(IEnumerable<GWGUI.Scp.FileSystems.FileSystemEntry> entries)
        => entries.Sum(entry => 1 + Count(entry.Children));
}
