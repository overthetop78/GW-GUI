using System.Windows;
using GWGUI.Domain.Settings;
using GWGUI.Domain.HostTools;
using GWGUI.Domain.Commands;
using GWGUI.Domain.Hardware;
using GWGUI.Infrastructure.Hardware;
using GWGUI.Infrastructure.Processes;

namespace GWGUI.App.Services;

public sealed record GwToolWindowRequest(string Executable, string Verb, string? Device, string? Drive, string LogsDirectory, OperationLogSettings Logging);
public enum OptionsSection { General, Logs, HostTools, Hardware, Engines, Profiles }

public interface IWindowNavigationService
{
    bool ShowOptions(AppSettings settings, OptionsSection section = OptionsSection.General);
    void ShowLogHistory(string logsDirectory);
    void ShowAbout();
    void ShowGwTool(GwToolWindowRequest request);
}

public sealed class WpfWindowNavigationService : IWindowNavigationService
{
    private readonly Window _owner;
    private readonly IGwInstallationManager? _hostTools;
    private readonly IGreaseweazleRunner _runner;
    private readonly IGwCommandBuilder _commandBuilder;

    public WpfWindowNavigationService(Window owner, IGwInstallationManager? hostTools = null, IGreaseweazleRunner? runner = null, IGwCommandBuilder? commandBuilder = null)
    {
        _owner = owner;
        _hostTools = hostTools;
        _runner = runner ?? new GreaseweazleRunner();
        _commandBuilder = commandBuilder ?? new GwCommandBuilder();
    }

    public bool ShowOptions(AppSettings settings, OptionsSection section = OptionsSection.General)
    {
        IHardwareRegistry hardware = new GreaseweazleHardwareRegistry(new WindowsSerialDeviceDiscovery(), _runner, _commandBuilder);
        new OptionsWindow(settings, hardware, _hostTools, section) { Owner = _owner }.ShowDialog();
        return true;
    }
    public void ShowLogHistory(string logsDirectory) => new LogHistoryWindow(logsDirectory) { Owner = _owner }.ShowDialog();
    public void ShowAbout() => new AboutWindow { Owner = _owner }.ShowDialog();
    public void ShowGwTool(GwToolWindowRequest request)
    {
        new GwToolWindow(request.Executable, request.Verb, request.Device, request.Drive, _runner, _commandBuilder, new ConsoleLogSession(request.LogsDirectory, () => request.Logging)) { Owner = _owner }.ShowDialog();
    }
}
