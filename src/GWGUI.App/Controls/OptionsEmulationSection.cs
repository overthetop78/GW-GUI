using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Domain.Settings;
using GWGUI.Emulation;

namespace GWGUI.App.Controls;

public sealed partial class OptionsEmulationSection : UserControl
{
    public static event EventHandler<EmulationConfigurationSavedEventArgs>? ConfigurationSaved;

    private readonly IReadOnlyList<IEmulationModule> _modules = EmulationModuleRegistry.Modules;
    private readonly ObservableCollection<EmulationConfigurationListItem> _configurations = [];
    private readonly ListBox _configurationList = new() { MinWidth = 360 };
    private readonly InputBindingEditor _shortcuts = new();
    private readonly TextBox _storageFolder = new();
    private readonly TextBox _captureFolder = new();
    private readonly TextBox _stateFolder = new();
    private AppSettings? _settings;
    private Func<Task>? _persistSettings;

    public OptionsEmulationSection()
    {
        var tabs = new TabControl
        {
            Margin = new Thickness(8),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        AddTab(tabs, "\uE713", LocExtension.Get("Emulation.Tab.General"), BuildGeneralTab());
        AddTab(tabs, "\uE765", LocExtension.Get("Emulation.Tab.Shortcuts"), BuildShortcutsTab());
        AddTab(tabs, "\uE8A5", LocExtension.Get("Emulation.Configurations"), BuildConfigurationsTab());
        foreach (var module in _modules)
        {
            var section = new EmulationModuleSettingsSection(module);
            section.ConfigurationSaved += ModuleConfigurationSaved;
            AddTab(tabs, "\uE7FC", LocExtension.Get(module.DisplayResourceKey), section);
        }
        Content = tabs;
        Loaded += async (_, _) => await ReloadConfigurationsAsync();
    }

    public void Configure(AppSettings settings, Func<Task> persistSettings)
    {
        _settings = settings;
        _persistSettings = persistSettings;
        _storageFolder.Text = settings.EmulationStorageFolder;
        _captureFolder.Text = settings.EmulationCaptureFolder;
        _stateFolder.Text = settings.EmulationStateFolder;
        _shortcuts.SetRows(GlobalShortcutDefinitions(), settings.EmulationShortcuts);
        StoragePaths.ConfigureEmulationStorageDirectory(settings.EmulationStorageFolder);
        StoragePaths.ConfigureEmulationCaptureDirectory(settings.EmulationCaptureFolder);
        StoragePaths.ConfigureEmulationStateDirectory(settings.EmulationStateFolder);
    }
}
