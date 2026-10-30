using GWGUI.App.Localization.Extensions;
using GWGUI.App.Services.Input.GameInput;
using GWGUI.App.Services.Logging;
using GWGUI.App.Views.Controls.Options.ControllerPresentation;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GWGUI.App.Views.Controls.Options;

public partial class OptionsControllersSection
{
    internal async Task RefreshLiveStateAsync()
    {
        var device = _selectedDevice;
        if (device is null)
        {
            ClearControls();
            Visualizer.State = null;
            return;
        }
        if (_refreshingState) return;
        _refreshingState = true;
        try
        {
            var state = await Task.Run(() => _source.ReadState(device.Id));
            if (_selectedDevice?.Id != device.Id) return;
            ApplyLiveState(state);
        }
        catch (Exception exception)
        {
            LogOnce(exception, "Reading GameInput controller state");
            DetectionStatus.Text = LocExtension.Get("Controllers.ReadFailed");
        }
        finally { _refreshingState = false; }
    }

    private void RefreshLiveState()
    {
        var device = _selectedDevice;
        if (device is null)
        {
            ClearControls();
            Visualizer.State = null;
            return;
        }
        if (_refreshingState) return;
        _refreshingState = true;
        try
        {
            var state = _source.ReadState(device.Id);
            if (_selectedDevice?.Id != device.Id) return;
            ApplyLiveState(state);
        }
        catch (Exception exception)
        {
            LogOnce(exception, "Reading GameInput controller state");
            DetectionStatus.Text = LocExtension.Get("Controllers.ReadFailed");
        }
        finally { _refreshingState = false; }
    }

    private void ApplyLiveState(GameInputLiveState state)
    {
        _lastState = state;
        Visualizer.State = state;
        UpdateControls(state);
        AnalogValuesList.ItemsSource = GameInputDescriptorPresenter.Analog(state);
        if (DetectionStatus.Text == LocExtension.Get("Controllers.ReadFailed")) UpdateDetectionStatus();
        _lastLoggedFailure = null;
    }

    private void UpdateControls(GameInputLiveState state)
    {
        var controls = state.Controls.Where(control => control.Type != GameInputControlType.RawByte).ToArray();
        var activeKeys = controls.Select(control => (control.Type, control.Index)).ToHashSet();
        for (var index = _controlRows.Count - 1; index >= 0; index--)
        {
            var row = _controlRows[index];
            if (activeKeys.Contains(row.Key)) continue;
            _controlRows.RemoveAt(index);
            _controlRowsByKey.Remove(row.Key);
        }
        foreach (var control in controls)
        {
            var key = (control.Type, control.Index);
            if (_controlRowsByKey.TryGetValue(key, out var row)) row.Update(control);
            else
            {
                row = new ControllerInputRow(control);
                _controlRowsByKey.Add(key, row);
                _controlRows.Add(row);
            }
        }
    }

    private void RebuildControlLabels(GameInputLiveState state)
    {
        foreach (var control in state.Controls.Where(control => control.Type != GameInputControlType.RawByte))
            if (_controlRowsByKey.TryGetValue((control.Type, control.Index), out var row))
                row.RefreshLabel(control);
    }

    private void ClearControls()
    {
        _controlRows.Clear();
        _controlRowsByKey.Clear();
        AnalogValuesList.ItemsSource = Array.Empty<ControllerAnalogRow>();
    }

}
