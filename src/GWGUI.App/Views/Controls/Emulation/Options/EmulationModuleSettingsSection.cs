using GWGUI.App.Constants.Controls.Visual;
using GWGUI.App.Constants.Emulation.Errors;
using GWGUI.App.Constants.Localization;
using GWGUI.App.Constants.Storage;
using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Contracts.Emulation.Machine;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Contracts.Views.Emulation.Settings;
using GWGUI.App.Controllers.Emulation.Firmware;
using GWGUI.App.Controllers.Emulation.Input;
using GWGUI.App.Controllers.Emulation.Options;
using GWGUI.App.Controllers.Emulation.Storage;
using GWGUI.App.Functions.Views.Emulation.Machine;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Common;
using GWGUI.App.Services.Audio;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Services.Storage;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;
using GWGUI.Emulation;
using Microsoft.Win32;
using System.IO;


namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed partial class EmulationModuleSettingsSection : UserControl, IAsyncDisposable
{
    private readonly IEmulationModule _module;
    private readonly GWGUI.VideoPresentation.Services.VideoPresentationProfileStore _profiles;
    private readonly Action<Exception> _showError;
    private readonly ListBox _machines = new()
    {
        MinWidth = 220,
        BorderThickness = new Thickness(0),
        Background = System.Windows.Media.Brushes.Transparent,
        HorizontalContentAlignment = HorizontalAlignment.Stretch
    };
    private readonly EmulationVideoProcessingSettingsSection _videoProcessing = new();
    private readonly Dictionary<string, FrameworkElement> _fieldControls = new(StringComparer.Ordinal);
    private readonly Dictionary<FrameworkElement, Func<Task>> _userChangeHandlers = [];
    private readonly EmulationEmulatorManagementController? _emulatorManagement;
    private readonly EmulationFirmwareManagementController? _firmwareManagement;
    private readonly EmulationInputSettingsController? _inputSettings;
    private readonly EmulationStorageSettingsController? _storageSettings;
    private IReadOnlyList<IEmulationConfiguration> _saved = [];
    private IEmulationConfiguration _configuration;
    private bool _loading;
    private bool _disposed;
    private readonly SemaphoreSlim _saveInputGate = new(1, 1);
    private EmulationMachineTab _selectedTab = EmulationMachineTab.General;

    internal EmulationModuleSettingsSection(IEmulationModule module,
        GWGUI.VideoPresentation.Services.VideoPresentationProfileStore? profiles = null,
        Action<Exception>? showError = null)
    {
        FlowDirection = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
        _module = module;
        _profiles = profiles ?? EmulationVideoPresentationProfiles.Store;
        _showError = showError ?? (error => ControlErrorPresenter.ShowEmulation(this, error,
            ControlErrorContexts.EmulationConfigurationManagement, LocExtension.GetForModule(_module, _module.DisplayResourceKey)));
        _machines.ItemContainerStyle = EmulationMachineChoiceLayout.CreateListItemContainerStyle();
        _machines.ItemTemplate = EmulationMachineChoiceLayout.CreateTemplate();
        var choices = module.Machines.Select(machine => new EmulationMachineChoice(machine,
            LocExtension.GetForModule(_module, machine.DisplayResourceKey), false)).ToArray();
        _machines.ItemsSource = choices;
        _machines.SelectedIndex = 0;
        _configuration = module.CreateConfiguration(choices[0].Definition.Id);
        if (module is IEmulationEmulatorManager manager)
        {
            _emulatorManagement = new EmulationEmulatorManagementController(manager,
                () => _configuration,
                SetConfiguration,
                () => _saved.Any(configuration => configuration.MachineId == _configuration.MachineId),
                module as IEmulationModuleLocalization);
            _emulatorManagement.ConfigurationChanged += EmulatorConfigurationChanged;
        }
        if (module is IEmulationFirmwareManager firmwareManager)
        {
            _firmwareManagement = new EmulationFirmwareManagementController(_module, firmwareManager,
                () => _configuration, SetConfiguration);
            _firmwareManagement.ConfigurationChanged += async (_, _) => await ExecuteUserChangeAsync();
        }
        if (module is IEmulationInputSettingsManager inputManager)
        {
            _inputSettings = new EmulationInputSettingsController(inputManager);
            _inputSettings.SettingsChanged += async (_, _) => await ExecuteUserChangeAsync();
        }
        if (module is IEmulationStorageSettingsManager storageManager)
        {
            _storageSettings = new EmulationStorageSettingsController(_module.Id, storageManager, DefaultFolder);
            _storageSettings.SettingsChanged += async (_, _) => await ExecuteUserChangeAsync();
        }
        _machines.SelectionChanged += MachineChanged;
        _videoProcessing.ConfigurationChanged += async (_, _) =>
        {
            var profile = _profiles.Get(_module.Id, _configuration.Id);
            _profiles.Set(_module.Id, _configuration.Id,
                profile with { Processing = _videoProcessing.Configuration });
            if (!_saved.Any(item => item.Id == _configuration.Id))
                EmulationConfigurationDraftStore.Set(_module.Id, _configuration);
            VideoConfigurationChanged?.Invoke(this,
                new EmulationConfigurationSavedEventArgs(_configuration));
            await ExecuteAsync(SaveVideoProfileAsync);
        };
        Content = BuildEditor();
        Loaded += async (_, _) => await ExecuteAsync(ReloadAsync);
    }

    internal event EventHandler<EmulationConfigurationSavedEventArgs>? ConfigurationSaved;
    internal event EventHandler<EmulationConfigurationSavedEventArgs>? VideoConfigurationChanged;
    internal event EventHandler<EmulationMachineEditingContext>? EditingContextChanged;
    internal IEmulationConfiguration CurrentConfiguration => _configuration;

    private async void EmulatorConfigurationChanged(object? sender, EventArgs args) =>
        await ExecuteUserChangeAsync();

    internal void SetVideoShaderLoading(string moduleId, Guid configurationId, bool isLoading)
    {
        if (!string.Equals(_module.Id, moduleId, StringComparison.Ordinal)
            || _configuration.Id != configurationId) return;
        _videoProcessing.SetShaderLoading(isLoading);
    }

    internal Task ReloadWhenOpenedAsync() => ExecuteAsync(ReloadAsync);

    internal async Task ReloadAfterConfigurationDeletedAsync(
        Guid configurationId,
        string machineId)
    {
        if (_configuration.Id == configurationId)
            EmulationConfigurationDraftStore.Remove(_module.Id, machineId);

        await ReloadAsync();
    }

    internal async Task EditConfigurationAsync(IEmulationConfiguration configuration)
    {
        _saved = await _module.LoadConfigurationsAsync();
        _configuration = configuration;
        SelectMachine(configuration.MachineId);
        _selectedTab = EmulationMachineTab.General;
        RebuildEditor();
        if (_emulatorManagement is not null)
            await _emulatorManagement.RefreshAsync();
        NotifyEditingContextChanged();
    }

    private async Task ReloadAsync()
    {
        _saved = await _module.LoadConfigurationsAsync();
        var machineId = _configuration.MachineId;
        var selected = _saved.FirstOrDefault(item => item.Id == _configuration.Id)
            ?? _saved.FirstOrDefault(item => item.MachineId == machineId);
        _configuration = selected
            ?? (EmulationConfigurationDraftStore.TryGet(_module.Id, machineId, out var draft)
                ? draft : _module.CreateConfiguration(machineId));
        var choices = _module.Machines.Select(machine => new EmulationMachineChoice(machine,
            LocExtension.GetForModule(_module, machine.DisplayResourceKey),
            _saved.Any(configuration => configuration.MachineId == machine.Id))).ToArray();
        _machines.ItemsSource = choices;
        SelectMachine(machineId);
        RebuildEditor();
        if (_emulatorManagement is not null) await _emulatorManagement.RefreshAsync();
        NotifyEditingContextChanged();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            if (_emulatorManagement is not null)
            {
                _emulatorManagement.ConfigurationChanged -= EmulatorConfigurationChanged;
                await _emulatorManagement.DisposeAsync();
            }
        }
        finally
        {
            _machines.SelectionChanged -= MachineChanged;
            Content = null;
        }
    }

}
