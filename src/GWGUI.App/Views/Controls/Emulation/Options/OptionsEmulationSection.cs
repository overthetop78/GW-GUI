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

public sealed partial class OptionsEmulationSection : UserControl
{
    public static event EventHandler<EmulationConfigurationSavedEventArgs>? ConfigurationSaved;
    internal event Action<EmulationMachineEditingContext?>? EditingContextChanged;

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
    private readonly Dictionary<TabItem, IEmulationModule> _moduleTabs = [];
    private readonly Dictionary<TabItem, EmulationModuleSettingsSection> _moduleSections = [];
    private bool _configurationsLoaded;
    private bool _loadingConfigurations;

    public OptionsEmulationSection()
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
        foreach (var module in _modules)
        {
            var tab = AddTab(_tabs, "\uE7FC", module.DisplayResourceKey, new Grid());
            _moduleTabs.Add(tab, module);
        }
        _tabs.SelectionChanged += ModuleTabSelectionChanged;
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

    private async void ModuleTabSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, _tabs)
            || _tabs.SelectedItem is not TabItem tab) return;
        if (!_moduleTabs.TryGetValue(tab, out var module))
        {
            EditingContextChanged?.Invoke(null);
            return;
        }

        var sectionAlreadyExists = _moduleSections.ContainsKey(tab);
        var section = GetOrCreateModuleSection(module);
        if (!sectionAlreadyExists)
            return;

        await section.ReloadWhenOpenedAsync();
    }

    private EmulationModuleSettingsSection GetOrCreateModuleSection(IEmulationModule module)
    {
        var tab = _moduleTabs.First(entry => ReferenceEquals(entry.Value, module)).Key;
        if (_moduleSections.TryGetValue(tab, out var section))
            return section;

        section = new EmulationModuleSettingsSection(module);
        section.ConfigurationSaved += ModuleConfigurationSaved;
        section.EditingContextChanged += ModuleEditingContextChanged;
        _moduleSections.Add(tab, section);
        tab.Content = section;
        return section;
    }

    private void ModuleEditingContextChanged(object? sender, EmulationMachineEditingContext context)
    {
        if (sender is not EmulationModuleSettingsSection section
            || _tabs.SelectedItem is not TabItem tab
            || !_moduleSections.TryGetValue(tab, out var active)
            || !ReferenceEquals(active, section)) return;
        EditingContextChanged?.Invoke(context);
    }

    internal void RefreshLocalizedContent()
    {
        var shortcutValues = _shortcuts.Rows.ToDictionary(row => row.Id, row => row.Binding,
            StringComparer.Ordinal);
        foreach (var (tab, resourceKey) in _localizedTabs)
        {
            var text = LocExtension.Get(resourceKey);
            if (tab.Header is MainTabHeader header) header.Text = text;
        }
        foreach (var section in _moduleSections.Values) section.RefreshLocalizedContent();
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
