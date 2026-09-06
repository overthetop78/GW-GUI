using GWGUI.App.Views.Controls.Explorer;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.App.ViewModels.Explorer;
using System.Windows.Controls;
using GWGUI.Domain.Formats;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.App.Localization.Extensions;
namespace GWGUI.Tests.Interface.ExplorerViews;
internal static class ExplorerDocumentScenarios
{
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
            detectedFileSystems: [new("test.first", "first-reader", firstImage, first.Volume), new("test.second", "second-reader", secondImage, second.Volume)],
            scpImage: first.ScpImage);
        section.Clear(); section.Display(image);
        Assert.Equal(new string?[] { null, "test.first", "test.second" }, section.FormatChoices.Select(item => item.Id));
        Assert.Equal("test.first", section.SelectedFormatId);
        var summary = Find<TextBlock>(section, "DetectedFormatsText");
        Assert.Contains("first-format", summary.Text); Assert.Contains("second-format", summary.Text);
        Assert.Equal(summary.Text, summary.ToolTip);
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
