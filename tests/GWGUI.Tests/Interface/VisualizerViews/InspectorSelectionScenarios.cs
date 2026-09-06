using GWGUI.App.Services.DiskImages;
using GWGUI.App.Services.Visualization;
using GWGUI.App.Views.Controls.Visualization;
using GWGUI.MediaEngine.Decoding;
using GWGUI.Tests.Interface.ExplorerViews;
using System.Windows;
using GWGUI.App.Contracts.ViewModels.Visualization;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;
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
        var model = Assert.IsType<ScpInspectorModel>(section.Inspector.DataContext);
        Assert.Equal(head, model.Head); Assert.Equal(2, model.Cylinder); Assert.Equal(4 + head, model.ScpEntry);
        Assert.Equal(2, model.RevolutionCount); Assert.Equal(2 + encoded.Revolution.FluxIntervals.Count, model.TotalTransitions);
        Assert.Equal(200, model.Revolutions[0].DurationMilliseconds); Assert.Equal(300, model.Revolutions[0].Rpm);
        Assert.Equal(new[] { 1, 2 }, model.Revolutions.Select(item => item.Number));
        Assert.Equal(1, model.SectorCount);
        Assert.StartsWith($"Visual.SectorDetail|2|{head}|3|128|", Assert.Single(model.Sectors));
        Assert.NotNull(model.Decode); Assert.NotEmpty(model.Structures);
        Assert.Equal(Visibility.Visible, section.Inspector.Visibility);
        await controller.SelectTrackAsync(null); Assert.Null(section.Inspector.DataContext);
        var pending = controller.SelectTrackAsync(track);
        controller.SetImage(ExplorerDocumentScenarios.Document("replacement").ScpImage!);
        await pending; Assert.Null(section.Inspector.DataContext);
        controller.SetImage(image);
        pending = controller.SelectTrackAsync(track);
        controller.ClearImage(); await pending; Assert.Null(section.Inspector.DataContext);
    }
    internal static ScpInspectorController Controller(VisualizerTabSection section, DiskImageCancellationScope scope) =>
        new(new Window(), section, new FluxDecoderRegistry(), scope, _ => Task.CompletedTask, () => { }, (key, _) => key);
    public static void Clear()
    {
        var section = new VisualizerTabSection();
        using var scope = new DiskImageCancellationScope();
        var controller = Controller(section, scope);
        section.Inspector.DataContext = new object();
        controller.SetImage(ExplorerDocumentScenarios.Document("first").ScpImage!);
        Assert.Null(section.Inspector.DataContext);
        section.Inspector.DataContext = new object();
        controller.ClearImage(); Assert.Null(section.Inspector.DataContext);
        controller.RefreshInspector(); Assert.Null(section.Inspector.DataContext);
    }
}
