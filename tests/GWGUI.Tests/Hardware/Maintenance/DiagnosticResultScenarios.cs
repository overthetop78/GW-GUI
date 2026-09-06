using GWGUI.App.Localization.Extensions;
using GWGUI.App.Functions.Localization;
using GWGUI.Domain.Commands.Execution;
using System.Windows.Controls;
namespace GWGUI.Tests.Hardware.Maintenance;
internal static class DiagnosticResultScenarios
{
    public static void Info(bool empty)
    {
        var lines = empty ? Array.Empty<GwOutputLine>() : new[] { new GwOutputLine(DateTimeOffset.UnixEpoch, GwOutputStream.Standard, "Model: Synthetic\nFirmware: 1.2\nPort: COM_FAKE\nGitHub lookup failed") };
        var window = MaintenanceRequestScenarios.Window("info", _ => new(0, false, TimeSpan.Zero, lines));
        window.ExecuteAsync().GetAwaiter().GetResult();
        var expected = empty ? $"{LocExtension.Get("Tool.ControllerFallback")} · {LocExtension.Get("Tool.FirmwareUnknown")} · {LocExtension.Get("Tool.PortUnknown")}" : $"Synthetic · 1.2 · COM_FAKE\n{LocExtension.Get("Tool.NetworkWarning")}";
        Assert.Equal(expected, Assert.IsType<TextBlock>(window.FindName("Summary")).Text);
    }
    public static void Result(int exit)
    {
        var errors = new List<Exception>(); var exception = new IOException("synthetic missing controller");
        var window = MaintenanceRequestScenarios.Window("rpm", _ => exit < 0 ? throw exception : new(exit, false, TimeSpan.Zero, []), errors);
        window.ExecuteAsync().GetAwaiter().GetResult();
        Assert.Equal(exit < 0 ? LocExtension.Get("Error.Unexpected", ExceptionDescriptionFunctions.Describe(exception)) : exit == 0 ? LocExtension.Get("Operation.Succeeded") : LocExtension.Get("Operation.ExitCode", exit), Assert.IsType<TextBlock>(window.FindName("Summary")).Text);
        Assert.Equal(exit < 0 ? 1 : 0, errors.Count); Assert.Equal(LocExtension.Get("Common.Execute"), Assert.IsType<Button>(window.FindName("ExecuteButton")).Content);
    }
}
