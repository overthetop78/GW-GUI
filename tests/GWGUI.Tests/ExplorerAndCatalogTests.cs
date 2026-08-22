using GWGUI.App.Contracts.Storage;
using GWGUI.App.Enums.Explorer;
using GWGUI.App.Functions.Storage;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Explorer;
using GWGUI.App.ViewModels.Explorer;
using GWGUI.App.Views.Controls.Common;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.MediaEngine.Exploration.Metadata;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.SectorImages;
using System.IO;
using GWGUI.MediaEngine;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Exploration;
using GWGUI.Infrastructure.Processes;
using GWGUI.Infrastructure.Settings;
using GWGUI.Infrastructure.Hardware;
using SkiaSharp;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace GWGUI.Tests;

public sealed class ExplorerAndCatalogTests : CoreTestBase
{
    [Fact]
    public void ExplorerDetailsSwitchBetweenDiskAndCentralListItemInformation()
    {
        var child = new GWGUI.MediaEngine.FileSystems.FileSystemEntry(
            "README.TXT", GWGUI.MediaEngine.FileSystems.FileSystemEntryKind.File, 42,
            new DateTimeOffset(1993, 8, 20, 14, 37, 0, TimeSpan.Zero), "Test comment", 0, 1, true, [], [65, 66]);
        var folder = new GWGUI.MediaEngine.FileSystems.FileSystemEntry(
            "DOCS", GWGUI.MediaEngine.FileSystems.FileSystemEntryKind.Directory, 0, null, "", 0, 2, true, [child]);
        var volume = new GWGUI.MediaEngine.FileSystems.FileSystemVolume(
            "TEST", "Atari TOS FAT12", 737280, 249 * 1024, null, null, [folder], ["warning"]);

        var image = new GWGUI.MediaEngine.SectorImages.SectorImage("atarist.720", 512, 80, 2, 9, []);
        var diskDetails = ExplorerDetailsPresenter.ForDisk(new ExploredDiskImage("test.st", image, volume, new GWGUI.MediaEngine.Exploration.Metadata.DiskImageMetadata(["atari-st"], null)));
        var fileDetails = ExplorerDetailsPresenter.ForItem(new ExplorerContentItem(child));
        var folderDetails = ExplorerDetailsPresenter.ForItem(new ExplorerContentItem(folder));

        Assert.Equal("TEST", diskDetails.Title);
        Assert.Equal(ExplorerIconCategory.DiskImage, diskDetails.IconCategory);
        Assert.Contains(diskDetails.Rows, row => row.Key == "Explorer.FileSystem" && row.Value == "Atari TOS FAT12");
        Assert.Contains(diskDetails.Rows, row => row.Key == "Explorer.System" && row.Value == "Atari ST");
        Assert.Contains(diskDetails.Rows, row => row.Key == "Explorer.Protection" && row.Value == "\u2014");
        Assert.Contains(diskDetails.Rows, row => row.Key == "Explorer.Entries" && row.Value == "2");
        Assert.Equal("README.TXT", fileDetails.Title);
        Assert.Equal(ExplorerIconCategory.Text, fileDetails.IconCategory);
        Assert.Contains(fileDetails.Rows, row => row.Key == "Explorer.Comment" && row.Value == "Test comment");
        Assert.Contains(folderDetails.Rows, row => row.Key == "Explorer.Entries" && row.Value == "1");
    }

    [Fact]
    public void ExplorerDiskDetailsUseTheSameCombinedWarningCountAsTheSummaryButton()
    {
        var image = new GWGUI.MediaEngine.SectorImages.SectorImage("amiga.amigados", 512, 1, 1, 1,
            [new GWGUI.MediaEngine.SectorImages.SectorBlock(0, new(0, 0, 0), new byte[512], false)]);
        var volume = new GWGUI.MediaEngine.FileSystems.FileSystemVolume("TEST", "amigados.ofs", 512, 0, null, null, [], ["filesystem warning"]);
        var document = new ExploredDiskImage("test.adf", image, volume,
            new GWGUI.MediaEngine.Exploration.Metadata.DiskImageMetadata(["amiga"], null));

        var details = ExplorerDetailsPresenter.ForDisk(document);

        Assert.Equal(ExplorerSection.BuildIssues(document).Count.ToString(),
            Assert.Single(details.Rows, row => row.Key == "Explorer.Warnings").Value);
    }

    [Fact]
    public void ExplorerShowsOnlyDetectedCustomLoaderCharacteristicsOutsideProtection()
    {
        var image = new GWGUI.MediaEngine.SectorImages.SectorImage("amiga.amigados", 512, 1, 1, 1, []);
        var content = new GWGUI.MediaEngine.Exploration.Metadata.DiskContentMetadata(true, GWGUI.MediaEngine.Exploration.Metadata.DiskContentIds.CrackTheCompany, [GWGUI.MediaEngine.Exploration.Metadata.DiskContentIds.CompressionFire]);
        var metadata = new GWGUI.MediaEngine.Exploration.Metadata.DiskImageMetadata(["amiga"], null, content);
        var volume = new GWGUI.MediaEngine.FileSystems.FileSystemVolume("Elf", "amiga.amigados", 901120, 0, null, null, [], []);
        var details = ExplorerDetailsPresenter.ForDisk(new ExploredDiskImage("elf.adf", image, volume, metadata, false));

        Assert.Contains(details.Rows, row => row.Key == "Explorer.Organization");
        Assert.Contains(details.Rows, row => row.Key == "Explorer.Modification" && row.Value.Contains("The Company"));
        Assert.Contains(details.Rows, row => row.Key == "Explorer.Compression" && row.Value == "FIRE");
        Assert.Contains(details.Rows, row => row.Key == "Explorer.Protection" && row.Value == "\u2014");

        var ordinary = ExplorerDetailsPresenter.ForDisk(new ExploredDiskImage("disk.adf", image, volume, new(["amiga"], null), false));
        Assert.DoesNotContain(ordinary.Rows, row => row.Key is "Explorer.Organization" or "Explorer.Modification" or "Explorer.Compression");
    }

    [Fact]
    public void ExplorerPresentsAtnMembersAsDataBlocksInsteadOfFiles()
    {
        var image = new GWGUI.MediaEngine.SectorImages.SectorImage("amiga.amigados", 512, 1, 1, 1, []);
        var content = new GWGUI.MediaEngine.Exploration.Metadata.DiskContentMetadata(false, null, [GWGUI.MediaEngine.Exploration.Metadata.DiskContentIds.CompressionAtnImploder], GWGUI.MediaEngine.Exploration.Metadata.DiskContentIds.OrganizationAtnArchive, 91);
        var metadata = new GWGUI.MediaEngine.Exploration.Metadata.DiskImageMetadata(["amiga"], null, content);
        var volume = new GWGUI.MediaEngine.FileSystems.FileSystemVolume("Body Blows Disk 2", "amiga.amigados", 901120, 0, null, null, [], []);
        var details = ExplorerDetailsPresenter.ForDisk(new ExploredDiskImage("body-blows-disk-2.adf", image, volume, metadata, false));

        Assert.Contains(details.Rows, row => row.Key == "Explorer.Compression" && row.Value == "ATN! (File Imploder)");
        Assert.Contains(details.Rows, row => row.Key == "Explorer.DataBlocks" && row.Value == "91");
        Assert.DoesNotContain(details.Rows, row => row.Key == "Explorer.Entries");
        Assert.True(details.IsSyntheticTitle);
        Assert.Equal($"({LocExtension.Get("Explorer.Unnamed")})", details.Title);
    }

    [Fact]
    public void ExplorerMarksARecognizedVolumeWithoutARealNameAsSynthetic()
    {
        var image = new GWGUI.MediaEngine.SectorImages.SectorImage("amiga.amigados", 512, 1, 1, 1,
            [new GWGUI.MediaEngine.SectorImages.SectorBlock(0, new(0, 0, 0), new byte[512], false)]);
        var volume = new GWGUI.MediaEngine.FileSystems.FileSystemVolume(string.Empty, "amigados.ofs", 512, 0, null, null, [], []);
        var document = new ExploredDiskImage("test.adf", image, volume,
            new GWGUI.MediaEngine.Exploration.Metadata.DiskImageMetadata(["amiga"], null));

        var name = ExplorerDetailsPresenter.VolumeName(document);
        var details = ExplorerDetailsPresenter.ForDisk(document);

        Assert.True(name.IsSynthetic);
        Assert.Equal($"({LocExtension.Get("Explorer.Unnamed")})", name.Text);
        Assert.True(details.IsSyntheticTitle);
        Assert.Contains(details.Rows, row => row.Key == "Explorer.Volume" && row.IsSyntheticValue);
    }

    [Fact]
    public void VisualizationUsesGwOnlyForAdvertisedInputAndScpOutput()
    {
        var catalog = new BuiltInImageFormatCatalog();
        var detector = new ImageFormatDetector(catalog);
        var capabilities = new GwFormatCapabilities(
            new HashSet<string>(["atarist.720"], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([".st", ".scp"], StringComparer.OrdinalIgnoreCase));
        var st = detector.Detect("disk.st", 737280);
        var atr = detector.Detect("disk.atr", 92176);

        Assert.True(GwVisualizationPolicy.CanConvertToScp("disk.st", st, capabilities));
        Assert.False(GwVisualizationPolicy.CanConvertToScp("disk.atr", atr, capabilities));
        Assert.False(GwVisualizationPolicy.CanConvertToScp("disk.st", st, GwFormatCapabilities.Unknown));
        Assert.False(GwVisualizationPolicy.CanConvertToScp("disk.st", st,
            capabilities with { ImageExtensions = new HashSet<string>([".st"], StringComparer.OrdinalIgnoreCase) }));
        var dskCapabilities = capabilities with { ImageExtensions = new HashSet<string>([".dsk", ".edsk", ".scp"], StringComparer.OrdinalIgnoreCase) };
        Assert.True(GwVisualizationPolicy.CanConvertToScp("disk.dsk", detector.Detect("disk.dsk", 194816), dskCapabilities));
        Assert.True(GwVisualizationPolicy.CanConvertToScp("disk.EDSK", detector.Detect("disk.EDSK", 194816), dskCapabilities));
    }

    [Fact]
    public void AtariStHighDensityUsesTheCompatibleGwIbmGeometry()
    {
        var capabilities = new GwFormatCapabilities(
            new HashSet<string>(["ibm.1440"], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([".st", ".scp"], StringComparer.OrdinalIgnoreCase));
        var catalog = new CapabilityAwareImageFormatCatalog(new BuiltInImageFormatCatalog(), capabilities);
        var detection = new ImageFormatDetector(catalog).Detect("disk.st", 1474560);

        Assert.Equal("atarist.1440", detection.Format?.Id);
        Assert.Equal("ibm.1440", GwFormatArgument.FromCatalogId(detection.Format?.Id));
        Assert.True(GwVisualizationPolicy.CanConvertToScp("disk.st", detection, capabilities));
    }

    [Fact]
    public void AtariAtrVisualizationUsesOnlyNativeGwNinetyAndOneThirtyFormats()
    {
        var capabilities = new GwFormatCapabilities(
            new HashSet<string>(["atari.90", "atari.130"], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([".img", ".scp"], StringComparer.OrdinalIgnoreCase));
        var detector = new ImageFormatDetector(new BuiltInImageFormatCatalog());

        Assert.True(GwVisualizationPolicy.CanConvertToScp("disk.atr", detector.Detect("disk.atr", 92176), capabilities));
        Assert.True(GwVisualizationPolicy.CanConvertToScp("disk.atr", detector.Detect("disk.atr", 133136), capabilities));
        Assert.False(GwVisualizationPolicy.CanConvertToScp("disk.atr", detector.Detect("disk.atr", 183952), capabilities));
    }

    [Fact]
    public void RuntimeCapabilitiesKeepCuratedFormatsAndExposeUnknownDiskDefinitions()
    {
        var capabilities = new GwFormatCapabilities(
            new HashSet<string>(["ibm.720", "dec.rx02", "ensoniq.mirage"], StringComparer.OrdinalIgnoreCase),
            new HashSet<string>([".scp", ".img"], StringComparer.OrdinalIgnoreCase));

        var catalog = new CapabilityAwareImageFormatCatalog(new BuiltInImageFormatCatalog(), capabilities);

        var dec = Assert.Single(catalog.Formats, format => format.Id == "dec.rx02");
        Assert.Equal("DEC", dec.Family);
        Assert.Equal("DEC RX02 — 512 KiB", dec.DisplayName);
        Assert.False(dec.IsCommon);
        Assert.Equal(".img", Assert.Single(dec.Extensions).Extension);
        Assert.Equal("DEC-RX02", dec.Tag);
        Assert.Contains(".scp", dec.CompatibleSourceExtensions!);
        Assert.Contains(catalog.Formats, format => format.Id == "ensoniq.mirage");
    }

    [Fact]
    public void CustomDiskDefsReaderResolvesPrefixesAndImports()
    {
        var directory = Path.Combine(Path.GetTempPath(), "gwgui-diskdefs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            File.WriteAllText(Path.Combine(directory, "child.cfg"), "disk format1\nend\n");
            File.WriteAllText(Path.Combine(directory, "root.cfg"), "disk local\nend\nimport vendor. \"child.cfg\"\n");

            var formats = DiskDefsFormatReader.Read(Path.Combine(directory, "root.cfg"));

            Assert.Equal(new HashSet<string>(["local", "vendor.format1"], StringComparer.OrdinalIgnoreCase), formats);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void CuratedCatalogContainsOfficialIbmAndAtariProfiles()
    {
        var catalog = new BuiltInImageFormatCatalog();
        string[] ibm = ["ibm.160", "ibm.180", "ibm.320", "ibm.360", "ibm.720", "ibm.800", "ibm.1200", "ibm.1440", "ibm.1680", "ibm.dmf", "ibm.2880", "ibm.scan"];
        string[] atari = ["atarist.180", "atarist.360", "atarist.400", "atarist.440", "atarist.720", "atarist.800", "atarist.810", "atarist.880"];

        Assert.All(ibm.Concat(atari), id => Assert.Contains(catalog.Formats, format => format.Id == id));
        Assert.Contains(catalog.Formats, format => format.Id == "amiga.amigados_hd");
        Assert.DoesNotContain(catalog.Formats, format => format.Id == "amiga.amigadoshd");
        Assert.All(catalog.Formats.Where(format => format.Family == "IBM PC"), format =>
            Assert.Equal(".ima", Assert.Single(format.Extensions, extension => extension.IsDefault).Extension));
    }

    [Fact]
    public void CatalogDisplayNamesAreProvidedByTheActiveLocalizer()
    {
        var catalog = new BuiltInImageFormatCatalog(key => "localized:" + key);
        var format = Assert.Single(catalog.Formats, item => item.Id == "ibm.720");
        Assert.Equal("localized:Format.ibm.720", format.DisplayName);
        Assert.Equal("localized:Extension.ima", format.Extensions[0].DisplayName);
    }

    [Fact]
    public void CuratedCatalogContainsEveryInternallyExplorableMachineFamily()
    {
        var catalog = new BuiltInImageFormatCatalog();
        string[] formats =
        [
            "amstrad.cpc", "amstrad.pcw",
            "acorn.dfs.ss", "acorn.dfs.ss80", "acorn.dfs.ds", "acorn.dfs.ds80",
            "epson.qx10.320", "epson.qx10.396", "epson.qx10.399", "epson.qx10.400", "epson.qx10.logo",
            "msx.1d", "msx.1dd", "msx.2d", "msx.2dd",
            "dec.rx02", "ucsd.ibm.mfm", "commodore900.coherent",
            "applelisa.office", "applelisa.macworks"
        ];

        Assert.All(formats, id => Assert.Contains(catalog.Formats, format => format.Id == id));
        Assert.Contains(catalog.Formats, format => format.Family == "Amstrad");
    }

    [Fact]
    public void AutomaticClassificationReplacesOrClearsThePreviousImageSelection()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                var selector = new DiskClassificationSelector();
                selector.SetCatalog(new BuiltInImageFormatCatalog().Formats);

                selector.ApplyDetection("atarist.360", null);
                Assert.Equal("Atari ST", selector.SelectedMachine);
                Assert.Equal("atarist.360", selector.SelectedFormatId);

                selector.ApplyDetection("unknown", null);
                Assert.Null(selector.SelectedMachine);
                Assert.Null(selector.SelectedFormatId);
            }
            catch (Exception exception) { failure = exception; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "The automatic classification test timed out.");
        if (failure is not null) throw failure;
    }

    [Fact]
    public void ExplorerRunsAutomaticClassificationOnlyForANewImage()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                const string path = @"F:\disk.scp";
                var explorer = new ExplorerSection();
                explorer.SetFormats(new BuiltInImageFormatCatalog().Formats, null);
                var selector = Assert.IsType<DiskClassificationSelector>(explorer.FindName("Classification"));

                explorer.Clear(path, true);
                explorer.Display(Document(path, "scp.composite", ["atarist.720", "amiga.amigados"]));
                Assert.Equal("Atari ST", selector.SelectedMachine);
                Assert.Equal("atarist.720", selector.SelectedFormatId);

                var detected = Assert.IsType<System.Windows.Controls.TextBlock>(explorer.FindName("DetectedFormatsText"));
                var currentSystem = Assert.IsType<System.Windows.Controls.TextBlock>(explorer.FindName("SystemText"));
                var currentFileSystem = Assert.IsType<System.Windows.Controls.TextBlock>(explorer.FindName("FileSystemText"));
                var currentCapacity = Assert.IsType<System.Windows.Controls.TextBlock>(explorer.FindName("CapacityText"));
                Assert.Contains("Atari ST", detected.Text);
                Assert.Contains("Amiga", detected.Text);
                Assert.Equal("Atari ST", currentSystem.Text);
                Assert.Equal("fat12", currentFileSystem.Text);
                Assert.Equal(StorageSizeFormatter.FormatBytes(720 * 1024), currentCapacity.Text);

                var machine = Assert.IsType<System.Windows.Controls.ComboBox>(selector.FindName("Machine"));
                var format = Assert.IsType<System.Windows.Controls.ComboBox>(selector.FindName("Format"));
                Assert.True(Assert.IsType<DiskMachineChoice>(machine.SelectedItem).IsDetected);
                Assert.True(Assert.IsType<DiskFormatChoice>(format.SelectedItem).IsDetected);
                Assert.True(machine.Items.Cast<DiskMachineChoice>().Single(item => item.DisplayName == "Amiga").IsDetected);

                format.SelectedItem = format.Items.Cast<DiskFormatChoice>().Single(item => item.Format.Id == "atarist.180");
                explorer.Clear(path, false);
                explorer.Display(Document(path, "atarist.180", ["atarist.180"]));
                Assert.Equal("atarist.180", selector.SelectedFormatId);

                machine.SelectedItem = machine.Items.Cast<DiskMachineChoice>().Single(item => item.DisplayName == "Amiga");
                Assert.Equal("amiga.amigados", selector.SelectedFormatId);
                machine.SelectedItem = machine.Items.Cast<DiskMachineChoice>().Single(item => item.DisplayName == "Atari ST");
                Assert.Equal("atarist.720", selector.SelectedFormatId);

                explorer.Clear(path, true);
                explorer.Display(Document(path, "amiga.amigados", ["amiga.amigados"]));
                Assert.Equal("amiga.amigados", selector.SelectedFormatId);
                Assert.Equal("Amiga", currentSystem.Text);
                Assert.Equal("amigados.ofs", currentFileSystem.Text);
                Assert.Equal(StorageSizeFormatter.FormatBytes(880 * 1024), currentCapacity.Text);
                Assert.False(machine.Items.Cast<DiskMachineChoice>().Single(item => item.DisplayName == "Atari ST").IsDetected);
            }
            catch (Exception exception) { failure = exception; }
            finally { Dispatcher.CurrentDispatcher.InvokeShutdown(); }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "The Explorer classification test timed out.");
        if (failure is not null) throw failure;

        static ExploredDiskImage Document(string path, string formatId, IEnumerable<string> detected)
        {
            var image = new GWGUI.MediaEngine.SectorImages.SectorImage(formatId, 512, 1, 1, 1,
                [new GWGUI.MediaEngine.SectorImages.SectorBlock(0, new(0, 0, 0), new byte[512], true)]);
            var atariVolume = new GWGUI.MediaEngine.FileSystems.FileSystemVolume("TEST", "fat12", 720 * 1024, 0, null, null, [], []);
            var amigaVolume = new GWGUI.MediaEngine.FileSystems.FileSystemVolume("AMIGA", "amigados.ofs", 880 * 1024, 0, null, null, [], []);
            var fileSystems = new[]
            {
                new GWGUI.MediaEngine.Exploration.Results.ExploredFileSystem("atarist.720", "fat12", image, atariVolume),
                new GWGUI.MediaEngine.Exploration.Results.ExploredFileSystem("amiga.amigados", "amigados.ofs", image, amigaVolume)
            }.Where(item => detected.Contains(item.FormatId, StringComparer.OrdinalIgnoreCase)).ToArray();
            var volume = formatId.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase) ? amigaVolume : atariVolume;
            var primaryFormatId = formatId.StartsWith("amiga.", StringComparison.OrdinalIgnoreCase) ? "amiga.amigados" : "atarist.720";
            var detectedImages = detected
                .Select(id => image.WithFormatId(id))
                .ToArray();
            return new(
                path,
                image,
                volume,
                new GWGUI.MediaEngine.Exploration.Metadata.DiskImageMetadata([], null),
                detectedFileSystems: fileSystems,
                detectedSectorImages: detectedImages,
                primaryFormatId: primaryFormatId);
        }
    }
}
