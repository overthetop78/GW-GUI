using GWGUI.Domain.Settings;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Contracts.Emulation.Machine;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Emulation.Configurations;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Services.Storage;
using GWGUI.App.Views.Controls.Emulation.Input;
using GWGUI.App.Views.Controls.Common;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GWGUI.Emulation;


namespace GWGUI.App.Views.Controls.Emulation.Options;

public sealed partial class EmulationPreferencesSection : UserControl
{
    public static event EventHandler<EmulationConfigurationSavedEventArgs>? ConfigurationSaved;
    public static event EventHandler<EmulationConfigurationSavedEventArgs>? VideoConfigurationChanged;
    internal event Func<IEmulationModule, IEmulationConfiguration, Task>? EditConfigurationRequested;

    private readonly IReadOnlyList<IEmulationModule> _modules = EmulationModuleRegistry.Modules;
    private readonly TextBlock _configurationBrandLabel = new();
    private readonly ComboBox _configurationBrand = new();
    private readonly ObservableCollection<EmulationModuleListItem> _configurationBrands = [];
    private IReadOnlyList<EmulationConfigurationTableRow> _configurationRows = [];
    private readonly EmulationConfigurationTable _configurationTable = new();
    private readonly InputBindingEditor _shortcuts = new();
    private readonly TextBox _storageFolder = new();
    private readonly TextBox _captureFolder = new();
    private readonly TextBox _stateFolder = new();
    private AppSettings? _settings;
    private Func<Task>? _persistSettings;
    private readonly TabControl _tabs;
    private readonly TabItem _generalTab;
    private readonly TabItem _shortcutsTab;
    private readonly List<(TabItem Tab, string ResourceKey)> _localizedTabs = [];
    private bool _configurationsLoaded;
    private bool _loadingConfigurations;

    public EmulationPreferencesSection()
    {
        _tabs = new TabControl
        {
            Margin = new Thickness(8),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch
        };
        _shortcuts.BindingsChanged += async (_, _) => await SaveShortcutsAsync();
        _configurationBrand.SelectionChanged += (_, _) => FilterConfigurationTable();
        _configurationTable.EditRequested += async row => await EditConfigurationAsync(row);
        _configurationTable.DeleteRequested += async row => await DeleteConfigurationAsync(row);
        _storageFolder.LostKeyboardFocus += async (_, _) => await SaveFoldersAsync();
        _captureFolder.LostKeyboardFocus += async (_, _) => await SaveFoldersAsync();
        _stateFolder.LostKeyboardFocus += async (_, _) => await SaveFoldersAsync();
        _generalTab = AddTab(_tabs, "\uE713", "Emulation.Tab.General", BuildGeneralTab());
        _shortcutsTab = AddTab(_tabs, "\uE765", "Emulation.Tab.Shortcuts", BuildShortcutsTab());
        AddTab(_tabs, "\uE8A5", "Emulation.Configuration", BuildConfigurationsTab());
        Content = _tabs;
        Loaded += async (_, _) => await LoadConfigurationsWhenVisibleAsync();
        IsVisibleChanged += async (_, _) => await LoadConfigurationsWhenVisibleAsync();
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

    internal Task ReloadConfigurationsForWindowAsync() => ReloadConfigurationsAsync();

    private async Task LoadConfigurationsWhenVisibleAsync()
    {
        if (!IsLoaded || !IsVisible || _configurationsLoaded || _loadingConfigurations) return;
        _loadingConfigurations = true;
        try
        {
            await ReloadConfigurationsAsync();
            _configurationsLoaded = true;
        }
        finally { _loadingConfigurations = false; }
    }

    internal static void RaiseConfigurationSaved(object sender, EmulationConfigurationSavedEventArgs args) =>
        ConfigurationSaved?.Invoke(sender, args);

    internal static void RaiseVideoConfigurationChanged(object sender, EmulationConfigurationSavedEventArgs args) =>
        VideoConfigurationChanged?.Invoke(sender, args);

    internal void RefreshLocalizedContent()
    {
        var shortcutValues = _shortcuts.Rows.ToDictionary(row => row.Id, row => row.Binding,
            StringComparer.Ordinal);
        foreach (var (tab, resourceKey) in _localizedTabs)
        {
            var text = LocExtension.Get(resourceKey);
            if (tab.Header is MainTabHeader header) header.Text = text;
        }
        _shortcuts.SetRows(GlobalShortcutDefinitions(), shortcutValues);
        _shortcuts.ConfigurePresentation(LocExtension.Get("Emulation.Input.Actions"),
            LocExtension.Get("Emulation.Input.Binding.Search"));
        _configurationBrandLabel.Text = LocExtension.Get("Emulation.Configuration.Brand");
        _configurationTable.RefreshLocalizedContent();
        if (_configurationsLoaded)
        {
            _configurationRows = EmulationConfigurationTablePresenter.CreateRows(
                _configurationRows.Select(row => (row.Module, row.Configuration)));
            RebuildConfigurationBrands();
            FilterConfigurationTable();
            _ = ReloadConfigurationsAsync();
        }
    }

}

