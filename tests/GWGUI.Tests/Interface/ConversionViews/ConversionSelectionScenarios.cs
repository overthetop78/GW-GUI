using GWGUI.App.ViewModels.Conversion;
using GWGUI.Domain.Formats;
using GWGUI.Domain.Formats.Detection;
using GWGUI.Domain.Settings;
using GWGUI.Domain.Settings.Engines;
using GWGUI.App.Controllers.MainWindow;
using GWGUI.App.Interfaces.Services.Dialogs;
using GWGUI.App.Presenters.Conversion;
using GWGUI.App.ViewModels.Main;
using GWGUI.App.Views.Controls.Conversion;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
namespace GWGUI.Tests.Interface.ConversionViews;
internal static class ConversionSelectionScenarios
{
    public static async Task Parameters(bool tags)
    {
        var context = new ConversionOperationScenarios.Context();
        context.Settings.Conversion.TagPattern = "[{EXTENSION}] ";
        context.Model.Conversion.OutputName = " edited "; context.Model.Conversion.AddTags = tags;
        context.Model.Conversion.SetFormat("ibm.160", true, [".ima"]);
        context.Model.Conversion.Tracks.Enabled = true; context.Model.Conversion.Tracks.Value = "c=2-4:h=1";
        context.Model.Conversion.OutputTracks.Enabled = true; context.Model.Conversion.OutputTracks.Value = "c=0-2:h=0";
        context.Model.Conversion.Reverse.Enabled = true; context.Model.Conversion.ExpertArguments = "--raw";
        await Dispatcher.Yield(DispatcherPriority.ContextIdle);
        var output = Assert.Single(context.Controller.Plan());
        var expected = Path.Combine("virtual-folder", (tags ? "[IMA] " : "") + "edited.ima");
        Assert.Equal(expected, output.OutputPath); Assert.Equal("ibm.160", output.FormatId);
        var running = context.Controller.ExecuteAsync(); await context.WaitUntilStarted(running);
        var arguments = Assert.Single(context.Commands).Arguments.ToList();
        Assert.Equal(expected, arguments.Last());
        Assert.Equal("c=2-4:h=1", arguments[arguments.IndexOf("--tracks") + 1]);
        Assert.Equal("c=0-2:h=0", arguments[arguments.IndexOf("--out-tracks") + 1]);
        Assert.Contains("--reverse", arguments); Assert.Contains("--raw", arguments);
        context.Pending.SetResult(new(0, false, TimeSpan.Zero, [])); await running;
        Assert.Single(context.Commands); Assert.Equal("Status.Success", context.Model.OperationText);
    }
    public static async Task SourceChanges()
    {
        var view = new ConversionTabSection(); var model = new MainWindowViewModel("synthetic", "synthetic"); view.DataContext = model;
        var catalog = new BuiltInImageFormatCatalog(key => key); var detector = new ImageFormatDetector(catalog, _ => throw new InvalidOperationException());
        var responses = new Queue<string?>(["virtual.adf", "unknown.synthetic", null]); var analyzed = new List<string>(); var failures = new List<Exception>();
        var settings = new AppSettings(); settings.Engines.Conversion = OperationEngine.Internal;
        model.Conversion.SetFormat("amiga.amigados", true, new HashSet<string> { ".adf" });
        model.Conversion.SetFormat("atarist.720", true, new HashSet<string> { ".st" });
        var controller = new ConversionTabController(new Window(), view, model, null!, new ConversionFormatPresenter(), () => catalog, () => detector, () => settings, null!, null!,
            ControlledDependencies.Simulate<IFileDialogService>((method, _) => { Assert.Equal("OpenFile", method.Name); return responses.Dequeue(); }),
            ControlledDependencies.Reject<IBusinessDialogService>(), ControlledDependencies.Reject<IMessageDialogService>(), null!, null!, null!, null!, new TextBox { Text = "virtual-folder" }, new TextBox(), new TextBox(), () => 0, _ => { }, null!, () => { }, (error, _) => failures.Add(error), () => { }, Dispatcher.CurrentDispatcher,
            _ => throw new InvalidOperationException(), path => detector.Detect(path, 901120), path => { analyzed.Add(path); return Task.CompletedTask; });
        await controller.BrowseSourceAsync();
        Assert.Equal("virtual.adf", model.Conversion.SourcePath); Assert.Equal("virtual", model.Conversion.OutputName);
        Assert.Equal("amiga.amigados", Assert.Single(model.Conversion.SelectedFormats));
        Assert.DoesNotContain(model.Conversion.BuildSelections(catalog.Formats), choice => choice.FormatId == "atarist.720");
        Assert.Equal(".st", Assert.Single(model.Conversion.ExplicitExtensions["atarist.720"]));
        Assert.Equal("Format.amiga.amigados", view.OutputBlock.SourceInformation.Text); Assert.Equal(Visibility.Collapsed, view.SourceBlock.ActionButton.Visibility);
        model.Conversion.AddTags = false; model.Conversion.OutputName = " edited ";
        var output = Assert.Single(controller.Plan()); Assert.Equal(Path.Combine("virtual-folder", "edited.adf"), output.OutputPath); Assert.Equal("amiga.amigados", output.FormatId);
        await controller.BrowseSourceAsync(); Assert.Empty(model.Conversion.SelectedFormats); Assert.Empty(model.Conversion.BuildSelections(catalog.Formats)); Assert.Empty(controller.Plan());
        var info = view.OutputBlock.SourceInformation.Text;
        await controller.BrowseSourceAsync(); Assert.Equal("unknown.synthetic", model.Conversion.SourcePath); Assert.Equal(info, view.OutputBlock.SourceInformation.Text);
        Assert.Equal(new[] { "virtual.adf", "unknown.synthetic" }, analyzed); Assert.Empty(failures);
    }
    public static void Selection()
    {
        var model=new ConversionOperationViewModel();
        model.SetFormat("synthetic",true,[".SCP",".scp"," "]);
        Assert.Single(model.ExplicitExtensions["synthetic"]);
        var selection=Assert.Single(model.BuildSelections([new DiskFormat("synthetic","test","test",[]),new DiskFormat("other","test","test",[])]));
        Assert.Equal("synthetic",selection.FormatId);
        model.SetFormat("synthetic",false,[]);
        Assert.Empty(model.SelectedFormats);Assert.Empty(model.ExplicitExtensions);
        model.ApplyProfile(new HashSet<string>{"tags","format:synthetic","reverse"},new Dictionary<string,string>{{"extensions:synthetic",".scp, .hfe"}});
        Assert.True(model.AddTags);
        Assert.Contains("format:synthetic",model.CaptureProfileEnabled());
        Assert.Equal(2,model.ExplicitExtensions["synthetic"].Count);
        var settings=new HashSet<string>{"synthetic"};
        var extensions=new Dictionary<string,HashSet<string>>{{"synthetic",new HashSet<string>{".scp"}}};
        model.ApplySettings(false,settings,extensions,new HashSet<string>(),new Dictionary<string,string>());
        settings.Clear();extensions["synthetic"].Clear();
        Assert.Contains("synthetic",model.SelectedFormats);
        Assert.Contains(".scp",model.ExplicitExtensions["synthetic"]);
    }
}
