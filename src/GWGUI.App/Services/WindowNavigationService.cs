using System.Windows;
using GWGUI.Domain.Settings;
using GWGUI.Infrastructure.Processes;

namespace GWGUI.App.Services;

public sealed record GwToolWindowRequest(string Executable, string Verb, string? Device, string? Drive, string LogsDirectory);

public interface IWindowNavigationService
{
    bool ShowOptions(AppSettings settings);
    void ShowLogHistory(string logsDirectory);
    void ShowAbout();
    void ShowGwTool(GwToolWindowRequest request);
}

public sealed class WpfWindowNavigationService(Window owner) : IWindowNavigationService
{
    public bool ShowOptions(AppSettings settings) => new OptionsWindow(settings) { Owner = owner }.ShowDialog() == true;
    public void ShowLogHistory(string logsDirectory) => new LogHistoryWindow(logsDirectory) { Owner = owner }.ShowDialog();
    public void ShowAbout() => new AboutWindow { Owner = owner }.ShowDialog();
    public void ShowGwTool(GwToolWindowRequest request)
    {
        var runner = new GreaseweazleRunner(new RotatingOperationLogWriter(request.LogsDirectory));
        new GwToolWindow(request.Executable, request.Verb, request.Device, request.Drive, runner) { Owner = owner }.ShowDialog();
    }
}
