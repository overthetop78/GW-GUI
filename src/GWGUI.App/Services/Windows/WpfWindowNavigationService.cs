using GWGUI.Domain.Commands.Building;
using GWGUI.Domain.Commands.Execution;
using GWGUI.Domain.HostTools;
using GWGUI.Domain.Settings;
using GWGUI.App.Contracts.Services.Navigation;
using GWGUI.App.Enums.Services.Navigation;
using GWGUI.App.Interfaces.Services.Navigation;
using GWGUI.App.Views.Windows.About;
using GWGUI.App.Views.Windows.Logs;
using GWGUI.App.Views.Windows.EmulationPreferences;
using GWGUI.App.Views.Windows.EmulationModuleOptions;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Views.Windows.Preferences;
using GWGUI.App.Views.Windows.Updates;
using GWGUI.App.Views.Windows.Tools;
using System.Windows;
using GWGUI.Infrastructure.Hardware;
using GWGUI.Infrastructure.Processes;
using GWGUI.App.Services.Updates;

namespace GWGUI.App.Services.Windows;

public sealed class WpfWindowNavigationService : IWindowNavigationService
{
    private readonly Window _owner;
    private readonly Func<AppSettings, PreferencesSection, Window> _preferences;
    private readonly Func<AppSettings, Window> _emulationPreferences;
    private readonly Func<AppSettings, string, Window> _emulationModuleOptions;
    private readonly Func<Window> _updates;
    private readonly Func<string, Window> _logs;
    private readonly Func<Window> _about;
    private readonly Func<GwToolWindowRequest, Window> _tool;
    private readonly Func<Window, Window, bool?> _show;

    public WpfWindowNavigationService(Window owner, IGwInstallationManager? hostTools = null,
        IGreaseweazleRunner? runner = null, IGwCommandBuilder? commandBuilder = null)
        : this(owner, hostTools, runner, commandBuilder, new PendingModuleInstallationStore())
    {
    }

    internal WpfWindowNavigationService(Window owner, IGwInstallationManager? hostTools,
        IGreaseweazleRunner? runner, IGwCommandBuilder? commandBuilder,
        PendingModuleInstallationStore pendingInstallations)
    {
        _owner = owner;
        var commandRunner = runner ?? new GreaseweazleRunner();
        var commands = commandBuilder ?? new GwCommandBuilder();
        _preferences = (settings, section) => new PreferencesWindow(settings,
            new GreaseweazleHardwareRegistry(new WindowsSerialDeviceDiscovery(), commandRunner, commands), hostTools, section);
        _emulationPreferences = settings => new EmulationPreferencesWindow(settings);
        _emulationModuleOptions = (_, moduleId) =>
        {
            var module = EmulationModuleRegistry.Modules.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, moduleId, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Emulation module '{moduleId}' is not loaded.");
            return new EmulationModuleOptionsWindow(module);
        };
        _updates = () => new UpdatesWindow(pendingInstallations);
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
        Func<AppSettings, PreferencesSection, Window> preferences,
        Func<AppSettings, Window> emulationPreferences,
        Func<AppSettings, string, Window> emulationModuleOptions,
        Func<Window> updates, Func<string, Window> logs,
        Func<Window> about, Func<GwToolWindowRequest, Window> tool, Func<Window, Window, bool?> show)
    {
        _owner = owner;
        _preferences = preferences;
        _emulationPreferences = emulationPreferences;
        _emulationModuleOptions = emulationModuleOptions;
        _updates = updates;
        _logs = logs;
        _about = about;
        _tool = tool;
        _show = show;
    }

    private void Show(Window window)
    {
        _show(window, _owner);
    }

    public bool ShowPreferences(AppSettings settings, PreferencesSection section = PreferencesSection.General)
    {
        Show(_preferences(settings, section));
        return true;
    }
    public void ShowEmulationPreferences(AppSettings settings) =>
        Show(_emulationPreferences(settings));
    public void ShowEmulationModuleOptions(AppSettings settings, string moduleId) =>
        Show(_emulationModuleOptions(settings, moduleId));
    public void ShowUpdates() => Show(_updates());
    public void ShowLogHistory(string logsDirectory) => Show(_logs(logsDirectory));
    public void ShowAbout() => Show(_about());
    public void ShowGwTool(GwToolWindowRequest request)
    {
        Show(_tool(request));
    }
}
