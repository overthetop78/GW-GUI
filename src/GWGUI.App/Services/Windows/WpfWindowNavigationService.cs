using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
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
    private readonly Func<AppSettings, OptionsSection, Window> _options;
    private readonly Func<string, Window> _logs;
    private readonly Func<Window> _about;
    private readonly Func<GwToolWindowRequest, Window> _tool;
    private readonly Func<Window, Window, bool?> _show;

    public WpfWindowNavigationService(Window owner, IGwInstallationManager? hostTools = null, IGreaseweazleRunner? runner = null, IGwCommandBuilder? commandBuilder = null)
    {
        _owner = owner;
        var commandRunner = runner ?? new GreaseweazleRunner();
        var commands = commandBuilder ?? new GwCommandBuilder();
        _options = (settings, section) => new OptionsWindow(settings,
            new GreaseweazleHardwareRegistry(new WindowsSerialDeviceDiscovery(), commandRunner, commands), hostTools, section);
        _logs = directory => new LogHistoryWindow(directory);
        _about = () => new AboutWindow();
        _tool = request => new GwToolWindow(request.Executable, request.Verb, request.Device, request.Drive,
            commandRunner, commands, new ConsoleLogSession(request.LogsDirectory, () => request.Logging));
        _show = (window, dialogOwner) =>
        {
            window.Owner = dialogOwner;
            return window.ShowDialog();
        };
    }

    internal WpfWindowNavigationService(Window owner,
        Func<AppSettings, OptionsSection, Window> options, Func<string, Window> logs,
        Func<Window> about, Func<GwToolWindowRequest, Window> tool, Func<Window, Window, bool?> show)
    {
        _owner = owner;
        _options = options;
        _logs = logs;
        _about = about;
        _tool = tool;
        _show = show;
    }

    private void Show(Window window)
    {
        _show(window, _owner);
    }

    public bool ShowOptions(AppSettings settings, OptionsSection section = OptionsSection.General)
    {
        Show(_options(settings, section));
        return true;
    }
    public void ShowLogHistory(string logsDirectory) => Show(_logs(logsDirectory));
    public void ShowAbout() => Show(_about());
    public void ShowGwTool(GwToolWindowRequest request)
    {
        Show(_tool(request));
    }
}
