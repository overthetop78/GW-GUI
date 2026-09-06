using GWGUI.App.Views.Windows.Tools;
using GWGUI.App.Localization.Extensions;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Tests.Application.TestInfrastructure;
using System.Windows.Controls;
namespace GWGUI.Tests.Hardware.FirmwareUpdate;
internal static class FirmwareUpdateScenarios
{
    internal static GwToolWindow Window(Func<GwCommand, GwExecutionResult> run, Func<string, string, bool>? confirm = null, Action<Exception, string>? log = null) => new("virtual", "update", "controller", "B",
        ControlledDependencies.Simulate<IGreaseweazleRunner>((method, args) => method.Name switch {
            "get_IsRunning" => false,
            "RunAsync" => Task.FromResult(run((GwCommand)args[0]!)),
            _ => throw new InvalidOperationException(method.Name)
        }), confirm: confirm ?? ((_, _) => throw new InvalidOperationException("Unexpected confirmation")), logError: log ?? ((error, _) => throw new InvalidOperationException("Unexpected error", error)));
    public static void Confirmation(bool bootloader, bool accepted)
    {
        var calls = 0; var prompts = 0;
        var window = Window(command => {
            calls++; Assert.Equal("update", command.Verb);
            Assert.Equal(bootloader ? new[] { "--bootloader", "--device", "controller" } : new[] { "--device", "controller" }, command.Arguments);
            return new(0, false, TimeSpan.Zero, []);
        }, (message, title) => { Assert.Equal(LocExtension.Get("Tool.BootloaderWarning"), message); Assert.Equal(LocExtension.Get("Tool.BootloaderTitle"), title); prompts++; return accepted; });
        var parameters = Assert.IsType<WrapPanel>(window.FindName("ParametersPanel"));
        Assert.IsType<CheckBox>(Assert.Single(parameters.Children.Cast<object>())).IsChecked = bootloader;
        window.ExecuteAsync().GetAwaiter().GetResult();
        Assert.Equal(bootloader ? 1 : 0, prompts);
        Assert.Equal(!bootloader || accepted ? 1 : 0, calls);
        Assert.Equal(LocExtension.Get("Common.Execute"), Assert.IsType<Button>(window.FindName("ExecuteButton")).Content);
    }
    public static void Outcome(int exit, bool cancelled)
    {
        var window = Window(_ => new(exit, cancelled, TimeSpan.Zero, []));
        window.ExecuteAsync().GetAwaiter().GetResult();
        Assert.Equal(cancelled ? LocExtension.Get("Operation.Cancelled") : exit == 0 ? LocExtension.Get("Operation.Succeeded") : LocExtension.Get("Operation.ExitCode", exit), Assert.IsType<TextBlock>(window.FindName("Summary")).Text);
    }
    public static void Error()
    {
        var error = new IOException("synthetic disconnect"); var calls = 0;
        var window = Window(_ => throw error, log: (actual, _) => { Assert.Same(error, actual); calls++; });
        window.ExecuteAsync().GetAwaiter().GetResult();
        Assert.Equal(1, calls);
        Assert.Equal(LocExtension.Get("Error.Unexpected", GWGUI.App.Functions.Localization.ExceptionDescriptionFunctions.Describe(error)), Assert.IsType<TextBlock>(window.FindName("Summary")).Text);
        Assert.Equal(LocExtension.Get("Common.Execute"), Assert.IsType<Button>(window.FindName("ExecuteButton")).Content);
    }
}
