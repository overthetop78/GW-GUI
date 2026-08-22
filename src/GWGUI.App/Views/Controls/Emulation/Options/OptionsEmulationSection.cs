using GWGUI.Domain.Settings;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Services.Storage;
using GWGUI.App.Views.Controls.Emulation.Input;
using GWGUI.App.Views.Controls.Common;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation;


namespace GWGUI.App.Views.Controls.Emulation.Options;

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
    private readonly TabControl _tabs;
    private readonly TabItem _generalTab;
    private readonly TabItem _shortcutsTab;
    private readonly Button _removeConfiguration = new();
    private readonly List<(TabItem Tab, string ResourceKey)> _localizedTabs = [];
    private readonly List<EmulationModuleSettingsSection> _moduleSections = [];

    public OptionsEmulationSection()
    {
        _tabs = new TabControl
        {
            Margin = new Thickness(8),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        _shortcuts.BindingsChanged += async (_, _) => await SaveShortcutsAsync();
        _storageFolder.LostKeyboardFocus += async (_, _) => await SaveFoldersAsync();
        _captureFolder.LostKeyboardFocus += async (_, _) => await SaveFoldersAsync();
        _stateFolder.LostKeyboardFocus += async (_, _) => await SaveFoldersAsync();
        _configurationList.SelectionChanged += (_, _) =>
            _removeConfiguration.IsEnabled = _configurationList.SelectedItem is EmulationConfigurationListItem;
        _generalTab = AddTab(_tabs, "\uE713", "Emulation.Tab.General", BuildGeneralTab());
        _shortcutsTab = AddTab(_tabs, "\uE765", "Emulation.Tab.Shortcuts", BuildShortcutsTab());
        AddTab(_tabs, "\uE8A5", "Emulation.Configuration", BuildConfigurationsTab());
        foreach (var module in _modules)
        {
            var section = new EmulationModuleSettingsSection(module);
            _moduleSections.Add(section);
            section.ConfigurationSaved += ModuleConfigurationSaved;
            AddTab(_tabs, "\uE7FC", module.DisplayResourceKey, section);
        }
        Content = _tabs;
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

    internal void RefreshLocalizedContent()
    {
        foreach (var (tab, resourceKey) in _localizedTabs)
        {
            var text = LocExtension.Get(resourceKey);
            if (tab.Header is MainTabHeader header) header.Text = text;
        }
        foreach (var section in _moduleSections) section.RefreshLocalizedContent();
        _generalTab.Content = null;
        _generalTab.Content = BuildGeneralTab();
        _shortcutsTab.Content = null;
        _shortcutsTab.Content = BuildShortcutsTab();
        _removeConfiguration.Content = LocExtension.Get("Common.Delete");
        _ = ReloadConfigurationsAsync();
    }
}
