using GWGUI.App.Views.Controls.Explorer;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.App.ViewModels.Explorer;
using System.Windows.Controls;
using System.Windows;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Enums;
using GWGUI.App.Localization.Extensions;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Formats.Floppy.Scp;

using GWGUI.MediaEngine.Representations.Sectors;
using GWGUI.App.Views.Controls.Common;
using GWGUI.Tests.Interface.VisualizerViews;
using System.IO;

namespace GWGUI.Tests.Interface.ExplorerViews;
internal static class ExplorerDocumentScenarios
{
    public static async Task ExtensionDoesNotChooseTheMediaPipeline()
    {
        var temporaryDirectory = Path.Combine(Path.GetTempPath(), $"gwgui-media-pipeline-{Guid.NewGuid():N}");
        var scpPath = Path.Combine(temporaryDirectory, "sector-source.scp");
        var imgPath = Path.Combine(temporaryDirectory, "sector-source.img");
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            File.Copy(typeof(ExplorerDocumentScenarios).Assembly.Location, scpPath);
            File.Copy(typeof(ExplorerDocumentScenarios).Assembly.Location, imgPath);
            using var workspace = new VisualizerDocumentScenarios.Workspace(
                withVisualizationProviders: true,
                withMediaServices: true,
                useDefaultExplorer: true,
                sectorCylinders: 1);

            await workspace.Controller.LoadAsync(scpPath);
            Assert.Equal(MediaRepresentationKind.Sectors, workspace.Visualizer.ActiveRepresentationKind);
            Assert.Equal(1, workspace.MediaReadCount);

            await workspace.Controller.LoadAsync(imgPath);
            Assert.Equal(MediaRepresentationKind.Sectors, workspace.Visualizer.ActiveRepresentationKind);
            Assert.Equal(2, workspace.MediaReadCount);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, true);
        }
    }

    public static void ScpMultiFormatSelector()
    {
        var section = new ExplorerSection();
        var label = Find<TextBlock>(section, "DetectedFormatLabel");
        var selector = Find<ComboBox>(section, "DetectedFormatSelector");
        Assert.Equal(Visibility.Collapsed, label.Visibility);
        Assert.Equal(Visibility.Collapsed, selector.Visibility);
        section.SetFormats(new BuiltInImageFormatCatalog(key => key).Formats, null);
        var source = Document("MULTI");
        var amiga = new SectorImage(DiskImageFormatIds.AmigaDos, 512, 80, 2, 11, []);
        var ibm = new SectorImage(DiskImageFormatIds.Ibm720, 512, 80, 2, 9, []);
        var atari = new SectorImage(DiskImageFormatIds.AtariSt720, 512, 80, 2, 9, []);
        var document = new ExploredDiskImage(
            source.SourcePath,
            amiga,
            source.Volume,
            source.Metadata,
            detectedFileSystems: [new("amiga", amiga, source.Volume)],
            detectedSectorImages: [ibm, atari],
            scpImage: source.ScpImage);

        section.Display(document);

        Assert.Equal(
            [DiskImageFormatIds.AmigaDos, DiskImageFormatIds.Ibm720, DiskImageFormatIds.AtariSt720],
            selector.Items.Cast<ExplorerFormatChoice>().Select(choice => choice.Id));
        Assert.Equal(Visibility.Visible, selector.Visibility);
        Assert.Equal(Visibility.Visible, label.Visibility);

        var single = new ExploredDiskImage(
            source.SourcePath,
            amiga,
            source.Volume,
            source.Metadata,
            detectedFileSystems: [new("amiga", amiga, source.Volume)],
            scpImage: source.ScpImage);
        section.Display(single);
        Assert.Equal(Visibility.Collapsed, selector.Visibility);
        Assert.Equal(Visibility.Collapsed, label.Visibility);
    }

    public static void AutomaticDetectionRestoresInitialMultiformatChoice()
    {
        var section = new ExplorerSection();
        var formats = new BuiltInImageFormatCatalog(key => key).Formats;
        section.SetFormats(formats, null);
        var source = Document("AUTO");
        var amiga = new SectorImage(DiskImageFormatIds.AmigaDos, 512, 80, 2, 11, []);
        var ibm = new SectorImage(DiskImageFormatIds.Ibm720, 512, 80, 2, 9, []);
        var document = new ExploredDiskImage(
            source.SourcePath,
            amiga,
            source.Volume,
            source.Metadata,
            detectedFileSystems: [new("amiga", amiga, source.Volume)],
            detectedSectorImages: [ibm],
            scpImage: source.ScpImage);
        section.Display(document);

        var changes = 0;
        section.FormatChanged += (_, _) => changes++;
        var automatic = Find<CheckBox>(section, "AutomaticDetection");
        automatic.IsChecked = false;
        Assert.Equal(0, changes);

        var classification = section.ClassificationSelector;
        var ibmFamily = new DiskClassificationCatalog(formats).ResolveFormat(DiskImageFormatIds.Ibm720)!.Family;
        var machines = Assert.IsType<ComboBox>(classification.FindName("Machine"));
        machines.SelectedItem = machines.Items.Cast<GWGUI.App.Contracts.Storage.DiskMachineChoice>()
            .Single(item => item.DisplayName == ibmFamily);
        Assert.Equal(DiskImageFormatIds.Ibm720, classification.SelectedFormatId);
        var changesAfterManualSelection = changes;

        automatic.IsChecked = true;

        Assert.Equal(changesAfterManualSelection + 1, changes);
        Assert.Equal(DiskImageFormatIds.AmigaDos, classification.SelectedFormatId);
        Assert.Equal(
            DiskImageFormatIds.AmigaDos,
            Assert.IsType<ExplorerFormatChoice>(Find<ComboBox>(section, "DetectedFormatSelector").SelectedItem).Id);
    }

    public static void AtariEightBitUsesFiveAndQuarterFloppyIcon()
    {
        var section = new ExplorerSection();
        var source = Document("ATARI");
        var image = source.Image.WithFormatId(DiskImageFormatIds.Atari90);
        section.Display(new ExploredDiskImage(source.SourcePath, image, source.Volume, source.Metadata));

        var identity = Find<MediaDocumentIdentity>(section, "DocumentIdentity");
        Assert.Equal("floppy-5.25", identity.MediaIconKind);
    }

    public static void ManualFormatSelectionRemainsAvailableWhileAnalysisRuns()
    {
        var section = new ExplorerSection();
        var formats = new[]
        {
            new DiskFormat("test.detected", "detected-family", "detected-format", [new(".one", "one", true)]),
            new DiskFormat("test.manual.first", "manual-family", "manual-first", [new(".two", "two", true)]),
            new DiskFormat("test.manual.second", "manual-family", "manual-second", [new(".three", "three", true)])
        };
        section.SetFormats(formats, null);
        var source = Document("MANUAL");
        var detectedImage = source.Image.WithFormatId("test.detected");
        section.Display(new ExploredDiskImage(source.SourcePath, detectedImage, source.Volume, source.Metadata,
            detectedFileSystems: [new("detected-reader", detectedImage, source.Volume)], scpImage: source.ScpImage));
        var changes = 0;
        section.FormatChanged += (_, _) => changes++;

        var classification = section.ClassificationSelector;
        var machines = Assert.IsType<ComboBox>(classification.FindName("Machine"));
        machines.SelectedItem = machines.Items.Cast<GWGUI.App.Contracts.Storage.DiskMachineChoice>()
            .Single(item => item.DisplayName == "manual-family");
        var choices = Assert.IsType<ComboBox>(classification.FindName("Format"));
        choices.SelectedItem = choices.Items.Cast<GWGUI.App.Contracts.Storage.DiskFormatChoice>()
            .Single(item => item.Format.Id == "test.manual.second");

        Assert.False(Find<CheckBox>(section, "AutomaticDetection").IsChecked);
        Assert.Equal("test.manual.second", section.SelectedFormatId);
        Assert.Equal(2, changes);
        section.SetLoading(true);
        Assert.True(Assert.IsType<Button>(section.FindName("ManualOptionsButton")).IsEnabled);
    }

    public static void Interpretations()
    {
        var section = new ExplorerSection();
        var first = Document("FIRST");
        var second = Document("SECOND", false);
        var firstImage = new SectorImage("test.first", 512, 1, 1, 1, []);
        var secondImage = new SectorImage("test.second", 512, 1, 1, 1, []);
        var formats = new[] {
            new DiskFormat("test.first", "first-family", "first-format", [new(".one", "one", true)]),
            new DiskFormat("test.second", "second-family", "second-format", [new(".two", "two", true)])
        };
        section.SetFormats(formats, null);
        var image = new ExploredDiskImage(first.SourcePath, firstImage, first.Volume, first.Metadata,
            detectedFileSystems: [new("first-reader", firstImage, first.Volume), new("second-reader", secondImage, second.Volume)],
            scpImage: first.ScpImage);
        section.Clear(); section.Display(image);
        Assert.Equal(new string?[] { null, "test.first", "test.second" }, section.FormatChoices.Select(item => item.Id));
        Assert.Equal("test.first", section.SelectedFormatId);
        var identity = Find<GWGUI.App.Views.Controls.Common.MediaDocumentIdentity>(section, "DocumentIdentity");
        var summary = identity.SummaryText;
        Assert.Contains("first-format", summary.Text); Assert.Contains("second-format", summary.Text);
        var interpretation = Assert.IsType<ExploredDiskImage>(image.SelectFormat("test.second"));
        section.Clear(newImage: false); section.Display(interpretation);
        Assert.Equal("SECOND", Find<TextBlock>(section, "VolumeNameText").Text);
        Assert.Empty(Find<ListView>(section, "ContentsList").Items);
        Assert.Null(image.SelectFormat("missing"));
        var unknown = new ExploredDiskImage("virtual/unknown.scp", new("unknown", 512, 1, 1, 1, []),
            second.Volume, second.Metadata, fileSystemRecognized: false, scpImage: first.ScpImage);
        section.Clear(); section.Display(unknown);
        Assert.Equal(LocExtension.Get("Explorer.Unknown"), Find<TextBlock>(section, "VolumeNameText").Text);
        Assert.Equal(LocExtension.Get("Explorer.PhysicalSectorsNoFileSystem"), Find<TextBlock>(section, "FileSystemText").Text);
        Assert.Empty(Find<ListView>(section, "ContentsList").Items);
        Assert.DoesNotContain("first-format", summary.Text); Assert.DoesNotContain("second-format", summary.Text);
        Assert.Null(section.FindName("DetectedFormatsText"));
    }
    internal static ExploredDiskImage Document(string name, bool populated = true)
    {
        var file = new FileSystemEntry("FILE.BIN", FileSystemEntryKind.File, 2, null, "note", 0, 0, true, [], [42, 93]);
        var folder = new FileSystemEntry("DIR", FileSystemEntryKind.Directory, 0, null, "", 0, 0, true, [file]);
        var scp = new ScpImage(new ScpHeader(0x24,0,1,0,0,ScpFlags.None,ScpBitCellEncoding.Default16Bit,ScpHeadSelection.Side0,0,0), [], true, 688);
        return new("virtual/" + name + ".scp", new("synthetic", 512, 1, 1, 1, []), new(name, "synthetic", 512, 256, null, null, populated ? [folder] : [], []), new([], null), scpImage: scp);
    }
    internal static T Find<T>(ExplorerSection section, string name) where T : class => Assert.IsType<T>(section.FindName(name));
    public static void Replace()
    {
        var section = new ExplorerSection();
        section.Clear(); section.Display(Document("FIRST"));
        Assert.Equal("virtual/FIRST.scp", Find<TextBox>(section, "PathText").Text);
        Assert.Equal("FIRST", Find<TextBlock>(section, "VolumeNameText").Text);
        Assert.Equal("2", Find<TextBlock>(section, "EntryCountText").Text);
        Assert.Equal("DIR", Assert.IsType<ExplorerContentItem>(Assert.Single(Find<ListView>(section, "ContentsList").Items.Cast<object>())).Entry.Name);
        section.Clear(); section.Display(Document("SECOND", false));
        Assert.Equal("SECOND", Find<TextBlock>(section, "VolumeNameText").Text);
        Assert.Equal("0", Find<TextBlock>(section, "EntryCountText").Text);
        Assert.Empty(Find<ListView>(section, "ContentsList").Items);
        Assert.Null(section.FormatIdForNewImage);
    }
}
