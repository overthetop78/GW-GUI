using GWGUI.App.Contracts.Emulation.Configurations;
using GWGUI.App.Contracts.Emulation.Machine;
using GWGUI.App.Contracts.Emulation.Settings;
using GWGUI.App.Controllers.Emulation.Options;
using GWGUI.App.Functions.Views.Emulation.Settings;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Emulation;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace GWGUI.App.Views.Controls.Emulation.Options;

internal sealed partial class EmulationModuleSettingsSection
{
    private async void MachineChanged(object sender, SelectionChangedEventArgs args)
    {
        if (_loading || _machines.SelectedItem is not EmulationMachineChoice selected) return;
        await ExecuteAsync(async () =>
        {
            _configuration = _saved.FirstOrDefault(item => item.MachineId == selected.Definition.Id)
                ?? (EmulationConfigurationDraftStore.TryGet(_module.Id, selected.Definition.Id, out var draft)
                    ? draft : _module.CreateConfiguration(selected.Definition.Id));
            RebuildEditor();
            if (_emulatorManagement is not null) await _emulatorManagement.RefreshAsync();
            NotifyEditingContextChanged();
        });
    }

    private async Task ExecuteAsync(Func<Task> action)
    {
        try { await action(); }
        catch (Exception error) { _showError(error); }
    }

    private Task ExecuteUserChangeAsync() => ExecuteAsync(ApplyUserChangeAsync);

    private async Task ApplyUserChangeAsync()
    {
        _profiles.Get(_module.Id, _configuration.Id);
        CaptureEditorValues();
        if (!_saved.Any(configuration => configuration.MachineId == _configuration.MachineId))
        {
            await EmulationConfigurationPersistenceFunctions.PersistAsync(
                _module, _configuration, hasSavedConfiguration: false, profiles: _profiles);
            return;
        }
        await _saveInputGate.WaitAsync();
        try
        {
            CaptureEditorValues();
            var configuration = _configuration;
            if (await EmulationConfigurationPersistenceFunctions.PersistAsync(
                    _module, configuration, hasSavedConfiguration: true, profiles: _profiles))
                ConfigurationSaved?.Invoke(this, new EmulationConfigurationSavedEventArgs(configuration));
        }
        finally { _saveInputGate.Release(); }
    }

    private async Task SaveAsync()
    {
        _profiles.Get(_module.Id, _configuration.Id);
        var values = _fieldControls.ToDictionary(item => item.Key, item => ReadValue(item.Value), StringComparer.Ordinal);
        _configuration = _module.ApplySettings(_configuration, values);
        if (_inputSettings is not null) _configuration = _inputSettings.Apply(_configuration);
        if (_storageSettings is not null) _configuration = _storageSettings.Apply(_configuration);
        var configuration = _configuration;
        await _profiles.SaveAsync(_module.Id, configuration.Id);
        await _module.SaveConfigurationAsync(configuration);
        EmulationConfigurationDraftStore.Remove(_module.Id, configuration.MachineId);
        ConfigurationSaved?.Invoke(this, new EmulationConfigurationSavedEventArgs(configuration));
        await ReloadAsync();
    }

    private Task SaveVideoProfileAsync()
    {
        var id = _configuration.Id;
        return _saved.Any(item => item.Id == id) ? _profiles.SaveAsync(_module.Id, id) : Task.CompletedTask;
    }

    private static string? ReadValue(FrameworkElement control) => control switch
    {
        ComboBox { SelectedItem: EmulationSettingsChoiceView selected } => selected.Choice.Id,
        CheckBox { Tag: EmulationSettingsField field } toggle => toggle.IsChecked == true
            ? field.EnabledValue : field.DisabledValue,
        TextBox text => text.Text,
        Grid { Tag: TextBox path } => path.Text,
        _ => null
    };

    private void SelectMachine(string machineId)
    {
        _loading = true;
        _machines.SelectedItem = _machines.Items.Cast<EmulationMachineChoice>()
            .First(item => item.Definition.Id == machineId);
        _loading = false;
    }

    private void RebuildEditor()
    {
        EmulationSettingsLayout.DetachReusableElement(_machines);
        EmulationSettingsLayout.DetachReusableElement(_videoProcessing);
        Content = null;
        Content = BuildEditor();
    }

    private void SetConfiguration(IEmulationConfiguration configuration)
    {
        _configuration = configuration;
        RebuildEditor();
    }

    private string CurrentMachineId() => _configuration.MachineId;

    private void NotifyEditingContextChanged()
    {
        var machine = (_machines.SelectedItem as EmulationMachineChoice)?.DisplayName ?? _configuration.MachineId;
        EditingContextChanged?.Invoke(this, new EmulationMachineEditingContext(
            LocExtension.GetForModule(_module, _module.DisplayResourceKey), machine));
    }

    private Task TabActivatedAsync(EmulationMachineTab tab) => RememberTabAndActivateAsync(tab);

    private async Task RememberTabAndActivateAsync(EmulationMachineTab tab)
    {
        _selectedTab = tab;
        if (tab == EmulationMachineTab.Rom && _firmwareManagement is not null)
            await _firmwareManagement.RefreshAsync();
    }

    internal void RefreshLocalizedContent()
    {
        CaptureEditorValues();
        var choices = _module.Machines.Select(machine => new EmulationMachineChoice(machine,
            LocExtension.GetForModule(_module, machine.DisplayResourceKey),
            _saved.Any(configuration => configuration.MachineId == machine.Id))).ToArray();
        _machines.ItemsSource = choices;
        SelectMachine(_configuration.MachineId);
        FlowDirection = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        RebuildEditor();
        NotifyEditingContextChanged();
    }

    private void CaptureEditorValues()
    {
        if (_fieldControls.Count != 0)
            _configuration = _module.ApplySettings(_configuration,
                _fieldControls.ToDictionary(item => item.Key, item => ReadValue(item.Value), StringComparer.Ordinal));
        if (_inputSettings is not null) _configuration = _inputSettings.Apply(_configuration);
        if (_storageSettings is not null) _configuration = _storageSettings.Apply(_configuration);
    }
}
