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
    internal async Task RefreshDevicesAsync(bool force)
    {
        var refreshVersion = Interlocked.Increment(ref _deviceRefreshVersion);
        var restartTimer = _timer.IsEnabled;
        if (force)
        {
            _timer.Stop();
            DetectButton.IsEnabled = false;
            DetectionStatus.Text = LocExtension.Get("Controllers.Detecting");
        }

        try
        {
            if (force) await Task.Run(_source.Refresh);
            if (refreshVersion != _deviceRefreshVersion) return;
            ApplyDevices(_source.GetConnectedDevices(), force);
            _lastLoggedFailure = null;
        }
        catch (Exception exception)
        {
            if (refreshVersion != _deviceRefreshVersion) return;
            LogOnce(exception, "Detecting GameInput controllers");
            ApplyDevices([], force: true);
            DetectionStatus.Text = LocExtension.Get("Controllers.DetectionFailed");
        }
        finally
        {
            if (refreshVersion == _deviceRefreshVersion && force)
            {
                DetectButton.IsEnabled = true;
                if (restartTimer && IsVisible) _timer.Start();
            }
        }
    }

    private void RefreshDevicesFromCache()
    {
        try
        {
            ApplyDevices(_source.GetConnectedDevices(), force: false);
            _lastLoggedFailure = null;
        }
        catch (Exception exception)
        {
            LogOnce(exception, "Reading cached GameInput controllers");
        }
    }

    private void ApplyDevices(IReadOnlyList<GameInputDeviceDescriptor> devices, bool force)
    {
        devices = devices.Select(ApplyStoredProfile).ToArray();
        if (!force && _devices.Select(device => device.Id)
                .SequenceEqual(devices.Select(device => device.Id), StringComparer.OrdinalIgnoreCase))
        {
            UpdateDetectionStatus();
            return;
        }

        var selectedId = _selectedDevice?.Id;
        _devices = devices;
        DeviceSelector.ItemsSource = _devices;
        _updatingSelectors = true;
        DeviceSelector.SelectedItem = _devices.FirstOrDefault(device =>
                string.Equals(device.Id, selectedId, StringComparison.OrdinalIgnoreCase))
            ?? _devices.FirstOrDefault();
        _updatingSelectors = false;
        SelectDevice(DeviceSelector.SelectedItem as GameInputDeviceDescriptor);
        UpdateDetectionStatus();
    }

    private GameInputDeviceDescriptor ApplyStoredProfile(GameInputDeviceDescriptor device)
    {
        if (_profiles.GetVisual(device.Id) is not { } profile) return device;
        return device with
        {
            ProductName = ControllerVisualProfileStore.DisplayName(profile.Model, profile.DisplayName),
            SuggestedVisualModel = profile.Model,
            IsExactVisualModelMatch = false
        };
    }

    private void DeviceSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_updatingSelectors)
            SelectDevice(DeviceSelector.SelectedItem as GameInputDeviceDescriptor);
    }

    private void SelectDevice(GameInputDeviceDescriptor? device)
    {
        _selectedDevice = device;
        _lastState = null;
        TestStatusText.Text = string.Empty;
        _updatingSelectors = true;
        if (device is null)
        {
            ModelSelectorPanel.Visibility = Visibility.Collapsed;
            AnalogDeadZonePanel.Visibility = Visibility.Collapsed;
            ModelSelector.SelectedIndex = -1;
            Visualizer.State = null;
        }
        else
        {
            ModelSelectorPanel.Visibility = device.IsExactVisualModelMatch
                ? Visibility.Collapsed
                : Visibility.Visible;
            var hasOverride = _visualOverrides.TryGetValue(device.Id, out var overrideModel);
            var model = hasOverride ? overrideModel : device.SuggestedVisualModel;
            ModelSelector.SelectedItem = ModelSelector.Items.Cast<ModelChoice>()
                .FirstOrDefault(choice => choice.Model == (hasOverride ? model : null));
            Visualizer.Model = model;
            LoadAnalogDeadZones(device);
        }
        _updatingSelectors = false;
        ClearControls();
        UpdateDescriptor();
        if (IsLoaded && IsVisible) _ = RefreshLiveStateAsync();
    }

    private void LoadAnalogDeadZones(GameInputDeviceDescriptor device)
    {
        AnalogDeadZonePanel.Visibility = device.StandardCapabilities.HasGamepad
            || (device.SupportedInput & GameInputKind.Gamepad) != 0
            ? Visibility.Visible : Visibility.Collapsed;
        var profile = _profiles.GetAnalog(device.Id);
        _updatingAnalogSettings = true;
        StickDeadZoneSlider.Value = profile.StickPercent;
        TriggerDeadZoneSlider.Value = profile.TriggerPercent;
        OuterDeadZoneSlider.Value = profile.OuterPercent;
        _updatingAnalogSettings = false;
        UpdateAnalogDeadZoneLabels(profile);
    }

    private ControllerAnalogDeadZoneProfile ReadAnalogDeadZones() => new(
        (int)Math.Round(StickDeadZoneSlider.Value),
        (int)Math.Round(TriggerDeadZoneSlider.Value),
        (int)Math.Round(OuterDeadZoneSlider.Value));

    private void AnalogDeadZoneSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_updatingAnalogSettings || _selectedDevice is null) return;
        var profile = ReadAnalogDeadZones().Normalize();
        UpdateAnalogDeadZoneLabels(profile);
        _profiles.PreviewAnalog(_selectedDevice.Id, profile);
        RefreshLiveState();
    }

    private void AnalogDeadZoneSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
        SaveAnalogDeadZones();

    private void AnalogDeadZoneSlider_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) =>
        SaveAnalogDeadZones();

    private void SaveAnalogDeadZones()
    {
        if (_updatingAnalogSettings || _selectedDevice is null) return;
        _profiles.SaveAnalog(_selectedDevice.Id, ReadAnalogDeadZones());
    }

    private void UpdateAnalogDeadZoneLabels(ControllerAnalogDeadZoneProfile profile)
    {
        StickDeadZoneText.Text = $"{profile.StickPercent} %";
        TriggerDeadZoneText.Text = $"{profile.TriggerPercent} %";
        OuterDeadZoneText.Text = $"{profile.OuterPercent} %";
    }

    private void ModelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_updatingSelectors || ModelSelector.SelectedItem is not ModelChoice choice) return;
        if (_selectedDevice is null || _selectedDevice.IsExactVisualModelMatch) return;
        if (choice.Model is ControllerVisualModel model)
        {
            _visualOverrides[_selectedDevice.Id] = model;
            var displayName = ControllerVisualProfileStore.DisplayName(model, choice.DisplayName);
            _profiles.SetVisual(_selectedDevice.Id, model, displayName);
            ApplyDevices(_devices, force: true);
        }
        else
        {
            _visualOverrides.Remove(_selectedDevice.Id);
            _profiles.RemoveVisual(_selectedDevice.Id);
            Visualizer.Model = _selectedDevice.SuggestedVisualModel;
        }
    }

    private void RefreshModelChoices()
    {
        var selected = ModelSelector.SelectedItem is ModelChoice choice ? choice.Model : null;
        _updatingSelectors = true;
        ModelSelector.ItemsSource = new[]
            {
                new ModelChoice(null, LocExtension.Get("Controllers.Model.Auto"))
            }
            .Concat(GameInputDeviceModelCatalog.AllVisualModels
                .Select(model => new ModelChoice((ControllerVisualModel?)model,
                    LocExtension.Get($"Controllers.Model.{model}"))))
            .ToArray();
        ModelSelector.SelectedItem = ModelSelector.Items.Cast<ModelChoice>()
            .FirstOrDefault(item => item.Model == selected) ?? ModelSelector.Items[0];
        _updatingSelectors = false;
    }

    private void UpdateDetectionStatus()
    {
        DetectionStatus.Text = _devices.Count == 0
            ? LocExtension.Get("Controllers.NoneDetected")
            : LocExtension.Get("Controllers.DetectedCount", _devices.Count);
    }

    private void UpdateDescriptor()
    {
        var device = _selectedDevice;
        if (device is null)
        {
            ProductNameText.Text = LocExtension.Get("Controllers.NoneDetected");
            DeviceIdentityText.Text = string.Empty;
            CapabilitiesList.ItemsSource = Array.Empty<ControllerDetailRow>();
            IdentityDetailsText.Text = string.Empty;
            AnalogValuesList.ItemsSource = Array.Empty<ControllerAnalogRow>();
            UpdateRumbleControls();
            return;
        }

        ProductNameText.Text = device.ProductName;
        DeviceIdentityText.Text = $"{device.VidPid} Â· {GameInputDisplayFormatter.Family(device.Family)}";
        UpdateRumbleControls();
        CapabilitiesList.ItemsSource = GameInputDescriptorPresenter.Capabilities(device);
        IdentityDetailsText.Text = GameInputDescriptorPresenter.Identity(device);
    }

}
