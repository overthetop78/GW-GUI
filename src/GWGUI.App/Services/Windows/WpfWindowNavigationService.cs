using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.Hardware;
using GWGUI.Domain.HostTools;
using GWGUI.Domain.Settings;
using GWGUI.App.Contracts.Services.Navigation;
using GWGUI.App.Enums.Services.Navigation;
using GWGUI.App.Interfaces.Services.Navigation;
using GWGUI.App.Views.Windows.About;
using GWGUI.App.Views.Windows.Logs;
using GWGUI.App.Views.Windows.Options;
using GWGUI.App.Views.Windows.Tools;
using System.Windows;
using GWGUI.Infrastructure.Hardware;
using GWGUI.Infrastructure.Processes;

namespace GWGUI.App.Services.Windows;

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
