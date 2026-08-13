using GWGUI.MediaEngine.Visualization;
using GWGUI.MediaEngine.Exploration;
using System.IO;
using GWGUI.MediaEngine.Containers.TeleDisk;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Recognition.Definitions;
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
    public async Task BodyBlowsDiskTwoRecognizesItsAtnArchiveWithoutInventingFiles()
    {
        const string path = @"F:\Disquettes\Body Blows Disk 2.scp";
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

        Assert.False(document.FileSystemRecognized);
        Assert.True(document.UsesCustomSectorLoader);
        Assert.Empty(document.Volume.Entries);
        Assert.Equal(DiskContentIds.OrganizationAtnArchive, document.Metadata.Content.OrganizationId);
        Assert.Equal(91, document.Metadata.Content.OrganizationMemberCount);
        Assert.Contains(DiskContentIds.CompressionAtnImploder, document.Metadata.Content.CompressionIds);
    }

    [Fact]
    public async Task BodyBlowsDiskTwoKeepsItsAtnOrganizationAfterInternalAdfConversion()
    {
        const string path = @"F:\Disquettes\Body Blows Disk 2.scp";
        if (!File.Exists(path)) return;
        var outputPath = Path.Combine(Path.GetTempPath(), $"gwgui-body-blows-{Guid.NewGuid():N}.adf");
        try
        {
            await MediaEngineFactory.CreateAmigaAdfConversionService().ConvertAsync(path, outputPath, DiskImageFormatIds.AmigaDos);
            var document = await DiskImageExplorer.CreateDefault().ExploreAsync(outputPath);

            Assert.True(document.UsesCustomSectorLoader);
            Assert.Empty(document.Volume.Entries);
            Assert.Equal(DiskContentIds.OrganizationAtnArchive, document.Metadata.Content.OrganizationId);
            Assert.Equal(91, document.Metadata.Content.OrganizationMemberCount);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task DuneTwoSaveAdfRecoversTheSameCatalogAsItsScpSource()
    {
        const string scpPath = @"F:\Disquettes\Dune II (Save 2).scp";
        const string adfPath = @"F:\Disquettes\Dune II (Save 2).adf";
        if (!File.Exists(scpPath) || !File.Exists(adfPath)) return;
        var explorer = DiskImageExplorer.CreateDefault();
        var scp = await explorer.ExploreAsync(scpPath);
        var adf = await explorer.ExploreAsync(adfPath);

        Assert.True(scp.FileSystemRecognized);
        Assert.True(adf.FileSystemRecognized);
        Assert.Equal(Names(scp.Volume.Entries), Names(adf.Volume.Entries));
    }

    [Fact]
    public async Task SuperCarsTwoDiskTwoIsRecognizedAsACataloglessBootImage()
    {
        const string path = @"F:\Disquettes\Supercars II Disk 2.scp";
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.False(document.FileSystemRecognized);
        Assert.True(document.UsesCustomSectorLoader);
        Assert.Empty(document.Volume.Entries);
        Assert.Equal(DiskContentIds.OrganizationCataloglessBootImage, document.Metadata.Content.OrganizationId);
    }

    [Fact]
    public async Task SkidmarksDiskTwoAdfAndScpExposeTheSameCataloglessOrganization()
    {
        const string scpPath = @"F:\Disquettes\Skidmarks v1.06\Skidmarks v1.06 - disk 2.scp";
        const string adfPath = @"F:\Disquettes\Skidmarks v1.06\Skidmarks v1.06 - disk 2.adf";
        if (!File.Exists(scpPath) || !File.Exists(adfPath)) return;
        var explorer = DiskImageExplorer.CreateDefault();
        var scp = await explorer.ExploreAsync(scpPath);
        var adf = await explorer.ExploreAsync(adfPath);

        Assert.True(scp.UsesCustomSectorLoader);
        Assert.True(adf.UsesCustomSectorLoader);
        Assert.Empty(scp.Volume.Entries);
        Assert.Empty(adf.Volume.Entries);
        Assert.Equal(adf.Metadata.Content.OrganizationId, scp.Metadata.Content.OrganizationId);
    }

    [Theory]
    [InlineData(@"F:\Disquettes\Goblins II Disk 2.scp")]
    [InlineData(@"F:\Disquettes\Speedball II.scp")]
    public async Task CompleteAmigaBootImagesWithoutCatalogDoNotExposePhysicalSectors(string path)
    {
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

        Assert.False(document.FileSystemRecognized);
        Assert.True(document.UsesCustomSectorLoader);
        Assert.Empty(document.Volume.Entries);
        Assert.Equal(DiskContentIds.OrganizationCataloglessBootImage, document.Metadata.Content.OrganizationId);
    }

    [Fact]
    public async Task Generation4Number53DetectsItsAmigaAndAtariFileSystems()
    {
        const string path = @"F:\Disquettes\Génération 4\Génération 4 N°53 - Mars 1993\Génération 4 - Disquette_Demo_N°53.scp";
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        var formats = document.DetectedFileSystems.Select(item => item.FormatId).ToArray();
        Assert.Equal(document.DetectedFileSystems[0].FormatId, document.PrimaryFormatId);
        Assert.StartsWith("atarist.", document.PrimaryFormatId, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(FileSystemIds.Fat12, document.Volume.FileSystemId);
        Assert.Equal(720 * 1024, document.Volume.Capacity);
        Assert.Contains(formats, format => format.StartsWith("atarist.", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(formats, format => format.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(DiskSystemIds.AtariSt, document.Metadata.SystemIds);
        Assert.Contains(DiskSystemIds.Amiga, document.Metadata.SystemIds);
    }

    [Fact]
    public async Task Generation4Number37ReportsOnlyAmigaAndAtari()
    {
        const string path = @"F:\Disquettes\Génération 4\Génération 4 N°37- Octobre 1991\Génération 4 N°37- Octobre 1991.scp";
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.Equal(["atari-st", "amiga"], document.Metadata.SystemIds);
        Assert.DoesNotContain(document.Metadata.SystemIds, system => system is "acorn-bbc" or "amstrad" or "ibm-pc" or "commodore" or "epson-qx10");
        var auto = Assert.Single(document.Volume.Entries, entry => entry.Name.Equals("AUTO", StringComparison.OrdinalIgnoreCase));
        var program = Assert.Single(auto.Children);
        Assert.Equal("TERII.PRG", program.Name, ignoreCase: true);
        Assert.Equal(1578, program.Size);
    }

    [Fact]
    public async Task Generation4PushOverCanBeReopenedExplicitlyAsAtariSt720()
    {
        const string path = @"F:\Disquettes\Génération 4\Génération 4 N°45 - Juin 1992 - Push Over\Génération 4 N°45 - Juin 1992 - Push Over.scp";
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path, "atarist.720");
        Assert.Equal("atarist.720", document.Image.FormatId);
        Assert.Equal(720 * 1024, document.Image.Capacity);
        Assert.True(document.FileSystemRecognized);
        Assert.Equal("PUSHOVER", document.Volume.Name);
        Assert.Contains(document.Volume.Entries, entry => entry.Name == "AUTO" && entry.Kind == GWGUI.MediaEngine.FileSystems.FileSystemEntryKind.Directory);
        Assert.Contains(document.Volume.Entries, entry => entry.Name == "PUSH.EXE");
    }

    [Theory]
    [InlineData(@"F:\Disquettes\Tilt\Tilt N°105\Tilt N°105 - Septembre 1992.scp")]
    [InlineData(@"F:\Disquettes\Tilt\Tilt N°110\Tilt N°110 - Janvier 1993.scp")]
    [InlineData(@"F:\Disquettes\Tilt\Tilt N°117\Tilt N°117 - Septembre 1993.scp")]
    public async Task TiltHybridCorpusReportsOnlyCredibleCompatibleSystems(string path)
    {
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.DoesNotContain(document.Metadata.SystemIds, system => system is "acorn-bbc" or "amstrad" or "commodore" or "epson-qx10");
    }

    [Fact]
    public async Task TiltNumber105ReportsItsAmigaAtariAndIbmFileSystems()
    {
        const string path = @"F:\Disquettes\Tilt\Tilt N°105\Tilt N°105 - Septembre 1992.scp";
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

        Assert.Contains(DiskSystemIds.Amiga, document.Metadata.SystemIds);
        Assert.Contains(DiskSystemIds.AtariSt, document.Metadata.SystemIds);
        Assert.Contains(DiskSystemIds.IbmPc, document.Metadata.SystemIds);
        Assert.Contains(document.DetectedFileSystems, detected => detected.FormatId == DiskImageFormatIds.AmigaDos);
        Assert.Contains(document.DetectedFileSystems, detected => detected.FormatId == DiskImageFormatIds.AtariSt720);
        Assert.Contains(document.DetectedFileSystems, detected => detected.FormatId == DiskImageFormatIds.Ibm720);
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

    private static string[] Names(IEnumerable<GWGUI.MediaEngine.FileSystems.FileSystemEntry> entries)
        => entries.SelectMany(entry => new[] { entry.Name }.Concat(Names(entry.Children))).Order(StringComparer.OrdinalIgnoreCase).ToArray();
}
