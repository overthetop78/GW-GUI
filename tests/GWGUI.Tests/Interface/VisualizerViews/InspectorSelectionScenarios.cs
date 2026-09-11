using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Visualization;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.MediaEngine.Decoding;
using GWGUI.Tests.Interface.ExplorerViews;
using System.Windows;
using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Formats.Floppy.Scp;

namespace GWGUI.Tests.Interface.VisualizerViews;
internal static class InspectorSelectionScenarios
{
    public static async Task Selection(int head)
    {
        var section = new VisualizerTabSection();
        using var scope = new DiskImageCancellationScope();
        var controller = new ScpInspectorController(new Window(), section, new FluxDecoderRegistry(), scope,
            _ => Task.CompletedTask, () => { }, (key, args) => key + "|" + string.Join("|", args));
        var encoded = new IsoMfmTrackEncoder().Encode(new(2, head, [new TrackSector(3, new byte[128])]));
        var first = new ScpRevolution(8_000_000, 2, [80, 160]);
        var second = new ScpRevolution(encoded.Revolution, (uint)encoded.Revolution.FluxIntervals.Count);
        var track = new ScpTrack((byte)(4 + head), 2, head, [first, second]);
        var image = new ScpImage(ExplorerDocumentScenarios.Document("capture").ScpImage!.Header, [track], true, 688);
        controller.SetImage(image);
        await controller.SelectTrackAsync(track);
        var model = Assert.IsType<MediaInspectorModel>(section.CommonInspector.DataContext);
        Assert.Equal("Visual.Title|", model.Title);
        Assert.Equal($"Visual.TrackTooltip|{head}|2|2", model.SelectedElement);
        var summary = model.Sections.Single(item => item.Title == "Visual.SummaryTab|");
        Assert.Equal(head.ToString(), summary.Entries.Single(item => item.Label == "Visual.SideLabel|").Value);
        Assert.Equal("2", summary.Entries.Single(item => item.Label == "Visual.TrackLabel|").Value);
        Assert.Equal((4 + head).ToString(), summary.Entries.Single(item => item.Label == "Visual.ScpEntryLabel|").Value);
        Assert.Equal("2", summary.Entries.Single(item => item.Label == "Visual.RevolutionsTitle|").Value);
        var revolutions = model.Sections.Single(item => item.Title == "Visual.RevolutionsTitle|").Entries;
        Assert.Equal(2, revolutions.Count);
        Assert.Contains("2", revolutions[0].Value);
        Assert.Contains("200.00 ms", revolutions[0].Value);
        Assert.Contains("300.00 RPM", revolutions[0].Value);
        Assert.Contains(model.Sections, item => item.Title == "Visual.AnalysisTitle|");
        Assert.Contains(model.Sections, item => item.Title == "Visual.StructuresTitle|");
        var sector = Assert.Single(model.Sections.Single(item => item.Title == "Visual.SectorsTitle|").Entries);
        Assert.StartsWith($"Visual.SectorDetail|2|{head}|3|128|", sector.Value);
        Assert.True(section.IsInspectorVisible);
        await controller.SelectTrackAsync(null); Assert.Null(section.CommonInspector.DataContext);
        var pending = controller.SelectTrackAsync(track);
        controller.SetImage(ExplorerDocumentScenarios.Document("replacement").ScpImage!);
        await pending; Assert.Null(section.CommonInspector.DataContext);
        controller.SetImage(image);
        pending = controller.SelectTrackAsync(track);
        controller.ClearImage(); await pending; Assert.Null(section.CommonInspector.DataContext);
    }
    internal static ScpInspectorController Controller(VisualizerTabSection section, DiskImageCancellationScope scope) =>
        new(new Window(), section, new FluxDecoderRegistry(), scope, _ => Task.CompletedTask, () => { }, (key, _) => key);
    public static void Clear()
    {
        var section = new VisualizerTabSection();
        using var scope = new DiskImageCancellationScope();
        var controller = Controller(section, scope);
        section.CommonInspector.DataContext = new object();
        controller.SetImage(ExplorerDocumentScenarios.Document("first").ScpImage!);
        Assert.Null(section.CommonInspector.DataContext);
        section.CommonInspector.DataContext = new object();
        controller.ClearImage(); Assert.Null(section.CommonInspector.DataContext);
        controller.RefreshInspector(); Assert.Null(section.CommonInspector.DataContext);
    }
}
