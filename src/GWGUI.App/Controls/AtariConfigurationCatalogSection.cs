using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GWGUI.App.Localization;
using GWGUI.Emulation.Atari;

namespace GWGUI.App.Controls;

public sealed class AtariConfigurationCatalogSection : UserControl
{
    private readonly AtariConfigurationCatalogController _controller;
    private readonly ObservableCollection<AtariConfigurationItem> _configurations = [];
    private readonly IReadOnlyList<AtariModelItem> _models = AtariConfigurationCatalogFunctions.Models();
    private readonly ListBox _list = new() { MinWidth = 260, DisplayMemberPath = nameof(AtariConfigurationItem.DisplayName) };
    private readonly AtariGeneralSettingsSection _general = new();
    private readonly AtariHardwareSettingsSection _hardware;
    private AtariMachineConfiguration? _current;
    private bool _loading;

    public AtariConfigurationCatalogSection() : this(new AtariConfigurationStore(
        StoragePaths.AtariConfigurationsDirectory, StoragePaths.DataDirectory)) { }

    internal AtariConfigurationCatalogSection(AtariConfigurationStore store)
    {
        _hardware = new AtariHardwareSettingsSection(_general);
        _general.ModelChanged += (_, configuration) =>
            _ = ExecuteAsync(_hardware, () => ModelChangedAsync(configuration));
        _general.SaveRequested += async (_, _) => await ExecuteAsync(_general, SaveConfiguration);
        _controller = new AtariConfigurationCatalogController(store);
        _list.ItemsSource = _configurations;
        AtariAccessibilityFunctions.Configure(_list,
            LocExtension.Get(AtariAccessibilityConstants.ConfigurationListResource),
            tabIndex: AtariAccessibilityConstants.ConfigurationListTabIndex);
        _list.SelectionChanged += ConfigurationSelected;
        Content = _hardware;
        AtariAccessibilityFunctions.ConfigureFlowDirection(this);
        Loaded += (_, _) => _ = ExecuteAsync(this, ReloadAsync);
    }

    public event EventHandler<AtariMachineConfiguration>? ConfigurationSaved;

    internal IReadOnlyList<AtariConfigurationItem> ConfigurationItems => _configurations.ToArray();

    public void ConfigureActiveCheck(Func<Guid, bool>? isActive)
    {
        _controller.ConfigureActiveCheck(isActive);
        UpdateEditorAvailability();
    }

    public async Task ReloadAsync()
    {
        var selectedId = _current?.Id;
        var configurations = await _controller.LoadAsync();
        AtariMachineConfiguration? configurationToLoad;
        _loading = true;
        try
        {
            _configurations.Clear();
            foreach (var configuration in configurations)
            {
                _configurations.Add(new AtariConfigurationItem(configuration,
                    AtariConfigurationCatalogFunctions.DisplayName(configuration,
                        AtariConfigurationCatalogFunctions.ModelName(configuration.Model))));
            }
            _list.SelectedItem = _configurations.FirstOrDefault(item => item.Configuration.Id == selectedId)
                ?? _configurations.FirstOrDefault();
            configurationToLoad = (_list.SelectedItem as AtariConfigurationItem)?.Configuration;
            _current = configurationToLoad;
        }
        finally
        {
            _loading = false;
        }

        if (configurationToLoad is null)
        {
            await NewConfiguration();
            return;
        }

        await _general.LoadAsync(configurationToLoad);
        await _hardware.LoadAsync(configurationToLoad);
        UpdateEditorAvailability();
    }

    internal async Task SelectConfigurationAsync(Guid id)
    {
        var selected = _configurations.FirstOrDefault(item => item.Configuration.Id == id);
        if (selected is null) return;
        _list.SelectedItem = selected;
        _current = selected.Configuration;
        await _general.LoadAsync(_current);
        await _hardware.LoadAsync(_current);
        UpdateEditorAvailability();
    }

    internal async Task DeleteConfigurationAsync(Guid id)
    {
        if (_controller.IsActive(id)) return;
        _controller.Delete(id);
        if (_current?.Id == id) _current = null;
        await ReloadAsync();
    }

    private static async Task ExecuteAsync(FrameworkElement owner, Func<Task> action)
    {
        try { await action(); }
        catch (Exception error) { ControlErrorPresenter.ShowUnexpected(owner, error,
            ControlErrorContexts.AtariConfiguration, AtariConfigurationCatalogConstants.AtariTitle); }
    }

    private void ConfigurationSelected(object sender, SelectionChangedEventArgs args)
    {
        if (_loading || _list.SelectedItem is not AtariConfigurationItem selected) return;
        _ = ExecuteAsync(this, async () =>
        {
            _current = selected.Configuration;
            await _general.LoadAsync(_current);
            await _hardware.LoadAsync(_current);
            UpdateEditorAvailability();
        });
    }

    private async Task NewConfiguration()
    {
        _current = new AtariMachineConfiguration(AtariMachineModel.St);
        _list.SelectedItem = null;
        await _general.LoadAsync(_current);
        await _hardware.LoadAsync(_current);
        UpdateEditorAvailability();
    }

    private async Task ModelChangedAsync(AtariMachineConfiguration requested)
    {
        var configuration = AtariConfigurationCatalogFunctions.ConfigurationForModel(_current,
            requested.Model, _configurations.Select(item => item.Configuration));
        _current = configuration;
        var saved = _configurations.FirstOrDefault(item => item.Configuration.Id == configuration.Id);
        _loading = true;
        try { _list.SelectedItem = saved; }
        finally { _loading = false; }
        if (saved is not null) await _general.LoadAsync(configuration);
        await _hardware.LoadAsync(configuration);
        UpdateEditorAvailability();
    }

    private async Task SaveConfiguration()
    {
        if (_current is null || _controller.IsActive(_current.Id)) return;
        var configuration = _hardware.Apply(_general.BuildConfiguration());
        await _controller.SaveAsync(configuration);
        _current = configuration;
        ConfigurationSaved?.Invoke(this, configuration);
        await ReloadAsync();
    }

    private void UpdateEditorAvailability()
    {
        var editable = _current is null || !_controller.IsActive(_current.Id);
        _general.IsEnabled = editable;
        _hardware.IsEnabled = editable;
    }
}
