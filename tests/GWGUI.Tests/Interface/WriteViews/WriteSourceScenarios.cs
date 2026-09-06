using GWGUI.App.Controllers.MainWindow;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Write;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Windows;
using System.Windows.Controls;
namespace GWGUI.Tests.Interface.WriteViews;
internal static class WriteSourceScenarios
{
    public static async Task Selection()
    {
        var view = new WriteTabSection(); var model = new MainWindowViewModel("synthetic", "synthetic"); view.DataContext = model;
        var settings = new AppSettings(); settings.Engines.PhysicalWrite = OperationEngine.Internal;
        var catalog = new BuiltInImageFormatCatalog(key => key);
        var detector = new ImageFormatDetector(catalog, _ => throw new InvalidOperationException("No disk reads"));
        var responses = new Queue<string?>(["virtual.adf", "ambiguous.adf", "unknown.synthetic", null]);
        var analyzed = new List<string>(); var failures = new List<Exception>(); var detections = new List<string>();
        var controller = new WriteTabController(view, model, null!, () => catalog, () => detector, () => settings, null!,
            ControlledDependencies.Simulate<IFileDialogService>((method, _) => { Assert.Equal("OpenFile", method.Name); return responses.Dequeue(); }),
            ControlledDependencies.Reject<IMessageDialogService>(), null!, null!, null!, null!, null!, null!, new TextBox(), new TextBox(), new TextBox(),
            () => 0, _ => { }, () => null, () => null, () => true, () => null, () => { }, null!, null!, null!, (error, _) => failures.Add(error), () => { },
            _ => throw new InvalidOperationException("No existence queries"),
            path => { detections.Add(path); return detector.Detect(path, path == "virtual.adf" ? 901120 : 123); },
            path => { analyzed.Add(path); return path == "unknown.synthetic" ? Task.FromException(new InvalidDataException("synthetic unknown")) : Task.CompletedTask; });
        await controller.BrowseSourceAsync();
        Assert.Equal("virtual.adf", model.Write.SourcePath); Assert.Equal("amiga.amigados", Assert.IsType<DiskFormat>(view.FormatBlock.FormatCombo.SelectedItem).Id);
        Assert.Equal(Visibility.Collapsed, view.FormatBlock.FormatCombo.Visibility); Assert.True(view.FormatBlock.VisualizeTracksButton.IsEnabled);
        Assert.Contains("Format.amiga.amigados", view.FormatBlock.DetectionText.Text);
        await controller.BrowseSourceAsync();
        Assert.Null(view.FormatBlock.FormatCombo.SelectedItem); Assert.Equal(Visibility.Visible, view.FormatBlock.FormatCombo.Visibility);
        Assert.Equal(3, view.FormatBlock.FormatCombo.Items.Count);
        view.FormatBlock.FormatCombo.SelectedIndex = 0;
        await controller.BrowseSourceAsync();
        Assert.Equal("unknown.synthetic", model.Write.SourcePath); Assert.Null(view.FormatBlock.FormatCombo.SelectedItem);
        Assert.Equal(catalog.Formats.Count, view.FormatBlock.FormatCombo.Items.Count); Assert.Single(failures);
        var information = view.FormatBlock.DetectionText.Text;
        await controller.BrowseSourceAsync();
        Assert.Equal("unknown.synthetic", model.Write.SourcePath); Assert.Equal(information, view.FormatBlock.DetectionText.Text);
        Assert.Equal(new[] { "virtual.adf", "ambiguous.adf", "unknown.synthetic" }, analyzed); Assert.Equal(analyzed, detections);
    }
}
