using GWGUI.App.Services.DiskImages;
using GWGUI.App.Views.Controls.Visualization;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Interfaces.Rendering.Scp;
using GWGUI.Tests.Application.TestInfrastructure;
using GWGUI.Tests.Interface.ExplorerViews;
using GWGUI.MediaEngine.Containers.Scp;
namespace GWGUI.Tests.Interface.VisualizerViews;
internal static class ViewportGeometryScenarios
{
    public static void Geometry()
    {
        var view = new ScpDiskView(ControlledDependencies.Simulate<IScpRenderer>((method, _) =>
            method.Name == "ClearCache" ? null : throw new InvalidOperationException(method.Name)));
        view.Measure(new Size(400, 300)); view.Arrange(new Rect(0, 0, 400, 300));
        var tracks = new[] { new ScpTrack(0, 0, 0, []), new ScpTrack(2, 1, 0, []), new ScpTrack(1, 0, 1, []) };
        var image = new ScpImage(ExplorerDocumentScenarios.Document("geometry").ScpImage!.Header, tracks, true, 688);
        view.SetImage(image, 0); view.PanBy(50, -20);
        var request = view.CreateRenderRequest(800, 600);
        Assert.Equal(500, request.Center.X); Assert.Equal(260, request.Center.Y);
        Assert.Equal(800, request.Width); Assert.Equal(600, request.Height); Assert.Same(image, request.Image);
        var selected = new List<ScpTrack?>(); view.TrackSelected += (_, track) => selected.Add(track);
        view.SelectTrackAt(new Point(250, 130)); Assert.Empty(selected); // Hub.
        view.SelectTrackAt(new Point(500, 130)); Assert.Empty(selected); // Outside the disk.
        view.SelectTrackAt(new Point(350, 130)); Assert.Same(tracks[0], view.SelectedTrack);
        view.SelectTrackAt(new Point(310, 130)); Assert.Same(tracks[1], view.SelectedTrack);
        Assert.Equal(new[] { tracks[0], tracks[1] }, selected);
        Assert.Same(tracks[1], view.CreateRenderRequest(800, 600).SelectedTrack);
        view.SetZoom(2); request = view.CreateRenderRequest(800, 600);
        Assert.Equal(2, request.Zoom); Assert.Equal(500, request.Center.X);
        view.ResetView(); request = view.CreateRenderRequest(800, 600);
        Assert.Equal(1, request.Zoom); Assert.Equal(400, request.Center.X); Assert.Equal(300, request.Center.Y);
        view.PanBy(-100, 80); view.SetZoom(3); view.SetImage(image, 1);
        request = view.CreateRenderRequest(800, 600);
        Assert.Null(request.SelectedTrack); Assert.Equal(1, request.Head); Assert.Equal(1, request.Zoom);
        Assert.Equal(400, request.Center.X); Assert.Equal(300, request.Center.Y);
        view.SelectTrackAt(new Point(300, 150)); Assert.Same(tracks[2], view.SelectedTrack);
        view.SetImage(null, 0); view.SelectTrackAt(new Point(300, 150));
        Assert.Null(view.SelectedTrack); Assert.Null(view.CreateRenderRequest(800, 600).Image);
    }
    public static void Zoom(bool linked)
    {
        var section = new VisualizerTabSection();
        using var scope = new DiskImageCancellationScope();
        _ = InspectorSelectionScenarios.Controller(section, scope);
        section.Header.LinkZoomCheckBox.IsChecked = linked;
        section.FirstSide.SetZoom(20, true);
        Assert.Equal(4, section.FirstSide.Zoom);
        Assert.Equal(linked ? 4 : 1, section.SecondSide.Zoom);
        section.SecondSide.SetZoom(-1, true);
        Assert.Equal(.65f, section.SecondSide.Zoom);
        Assert.Equal(linked ? .65f : 4, section.FirstSide.Zoom);
        section.Header.ResetButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Assert.Equal(1, section.FirstSide.Zoom); Assert.Equal(1, section.SecondSide.Zoom);
    }
}
