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

public partial class OptionsControllersSection : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly IGameInputControllerSource _source;
    private readonly Action<Exception, string> _errorLogger;
    private readonly ObservableCollection<ControllerInputRow> _controlRows = [];
    private readonly Dictionary<(GameInputControlType Type, int Index), ControllerInputRow> _controlRowsByKey = [];
    private readonly Dictionary<string, ControllerVisualModel> _visualOverrides =
        ControllerVisualProfileStore.GetModels().ToDictionary(item => item.Key, item => item.Value,
            StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<GameInputDeviceDescriptor> _devices = [];
    private GameInputDeviceDescriptor? _selectedDevice;
    private GameInputLiveState? _lastState;
    private bool _updatingSelectors;
    private bool _updatingAnalogSettings;
    private bool _refreshingState;
    private bool _testingRumble;
    private int _deviceRefreshVersion;
    private DateTime _nextDevicePollUtc;
    private string? _lastLoggedFailure;

    public OptionsControllersSection() : this(GameInputControllerSource.Instance) { }

    internal OptionsControllersSection(
        IGameInputControllerSource source,
        Action<Exception, string>? errorLogger = null)
    {
        _source = source;
        _errorLogger = errorLogger ?? ((exception, context) => ErrorLog.Write(exception, context));
        InitializeComponent();
        ControlsGrid.ItemsSource = _controlRows;
        _timer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(33)
        };
        _timer.Tick += Timer_Tick;
        _source.StartMonitoring();
        RefreshModelChoices();
    }

    internal void RefreshLocalizedContent()
    {
        RefreshModelChoices();
        UpdateDetectionStatus();
        UpdateDescriptor();
        if (_lastState is not null)
        {
            RebuildControlLabels(_lastState);
            AnalogValuesList.ItemsSource = GameInputDescriptorPresenter.Analog(_lastState);
        }
    }

    private async void Section_Loaded(object sender, RoutedEventArgs e)
    {
        if (!IsVisible) return;
        await RefreshDevicesAsync(force: false);
        if (IsVisible) _timer.Start();
    }

    private void Section_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        Interlocked.Increment(ref _deviceRefreshVersion);
        StopRumble();
    }

    private async void Section_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (!IsLoaded) return;
        if (IsVisible)
        {
            await RefreshDevicesAsync(force: false);
            if (IsVisible) _timer.Start();
        }
        else
        {
            _timer.Stop();
            Interlocked.Increment(ref _deviceRefreshVersion);
            StopRumble();
        }
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        if (!IsVisible) return;
        if (DateTime.UtcNow >= _nextDevicePollUtc)
        {
            _nextDevicePollUtc = DateTime.UtcNow.AddMilliseconds(250);
            RefreshDevicesFromCache();
        }
        await RefreshLiveStateAsync();
    }

    private async void Detect_Click(object sender, RoutedEventArgs e) =>
        await RefreshDevicesAsync(force: true);

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

    private static GameInputDeviceDescriptor ApplyStoredProfile(GameInputDeviceDescriptor device)
    {
        if (!ControllerVisualProfileStore.TryGet(device.Id, out var profile)) return device;
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
        var profile = ControllerAnalogDeadZoneProfileStore.Get(device.Id);
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
        ControllerAnalogDeadZoneProfileStore.Preview(_selectedDevice.Id, profile);
        RefreshLiveState();
    }

    private void AnalogDeadZoneSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
        SaveAnalogDeadZones();

    private void AnalogDeadZoneSlider_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) =>
        SaveAnalogDeadZones();

    private void SaveAnalogDeadZones()
    {
        if (_updatingAnalogSettings || _selectedDevice is null) return;
        ControllerAnalogDeadZoneProfileStore.Save(_selectedDevice.Id, ReadAnalogDeadZones());
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
            ControllerVisualProfileStore.Set(_selectedDevice.Id, model, displayName);
            ApplyDevices(_devices, force: true);
        }
        else
        {
            _visualOverrides.Remove(_selectedDevice.Id);
            ControllerVisualProfileStore.Remove(_selectedDevice.Id);
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
        DeviceIdentityText.Text = $"{device.VidPid} · {GameInputDisplayFormatter.Family(device.Family)}";
        UpdateRumbleControls();
        CapabilitiesList.ItemsSource = GameInputDescriptorPresenter.Capabilities(device);
        IdentityDetailsText.Text = GameInputDescriptorPresenter.Identity(device);
    }

    private async Task RefreshLiveStateAsync()
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

    private async void RumbleMotor_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string value } ||
            !Enum.TryParse<GameInputRumbleMotors>(value, out var motor)) return;
        await TestRumbleAsync(motor);
    }

    private void RumbleStrengthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (RumbleStrengthText is not null)
            RumbleStrengthText.Text = $"{Math.Round(e.NewValue):0} %";
    }

    internal async Task TestRumbleAsync(GameInputRumbleMotors motor)
    {
        const int pulseDurationMilliseconds = 250;

        var device = _selectedDevice;
        var motorBits = (uint)motor;
        if (device is null || motorBits == 0 || (motorBits & (motorBits - 1)) != 0 ||
            !device.RumbleMotors.HasFlag(motor)) return;

        var intensity = (float)(RumbleStrengthSlider.Value / 100d);
        var values = motor switch
        {
            GameInputRumbleMotors.LowFrequency => (intensity, 0f, 0f, 0f),
            GameInputRumbleMotors.HighFrequency => (0f, intensity, 0f, 0f),
            GameInputRumbleMotors.LeftTrigger => (0f, 0f, intensity, 0f),
            GameInputRumbleMotors.RightTrigger => (0f, 0f, 0f, intensity),
            _ => (0f, 0f, 0f, 0f)
        };

        _testingRumble = true;
        UpdateRumbleControls();
        TestStatusText.Text = LocExtension.Get("Controllers.TestRunning");
        var started = false;
        try
        {
            started = _source.SetRumble(device.Id, values.Item1, values.Item2, values.Item3, values.Item4);
            if (!started)
            {
                TestStatusText.Text = LocExtension.Get("Controllers.TestFailed");
                return;
            }
            await Task.Delay(pulseDurationMilliseconds);
            TestStatusText.Text = LocExtension.Get("Controllers.TestCompleted");
        }
        catch (Exception exception)
        {
            LogOnce(exception, "Testing GameInput rumble");
            TestStatusText.Text = LocExtension.Get("Controllers.TestFailed");
        }
        finally
        {
            if (started)
            {
                try { _source.SetRumble(device.Id, 0f, 0f, 0f, 0f); }
                catch (Exception exception) { LogOnce(exception, "Stopping GameInput rumble"); }
            }
            _testingRumble = false;
            UpdateRumbleControls();
        }
    }

    private void StopRumble()
    {
        var device = _selectedDevice;
        if (device is null || device.RumbleMotors == GameInputRumbleMotors.None) return;
        try { _source.SetRumble(device.Id, 0f, 0f, 0f, 0f); }
        catch (Exception exception) { LogOnce(exception, "Stopping GameInput rumble"); }
    }

    private void UpdateRumbleControls()
    {
        var motors = _selectedDevice?.RumbleMotors ?? GameInputRumbleMotors.None;
        RumblePanel.Visibility = motors == GameInputRumbleMotors.None
            ? Visibility.Collapsed
            : Visibility.Visible;
        var enabled = !_testingRumble;
        LowFrequencyRumbleButton.IsEnabled = enabled && motors.HasFlag(GameInputRumbleMotors.LowFrequency);
        HighFrequencyRumbleButton.IsEnabled = enabled && motors.HasFlag(GameInputRumbleMotors.HighFrequency);
        LeftTriggerRumbleButton.IsEnabled = enabled && motors.HasFlag(GameInputRumbleMotors.LeftTrigger);
        RightTriggerRumbleButton.IsEnabled = enabled && motors.HasFlag(GameInputRumbleMotors.RightTrigger);
        RumbleStrengthSlider.IsEnabled = enabled && motors != GameInputRumbleMotors.None;
    }

    private void LogOnce(Exception exception, string context)
    {
        var signature = $"{context}|{exception.GetType().FullName}|{exception.HResult:X8}";
        if (string.Equals(signature, _lastLoggedFailure, StringComparison.Ordinal)) return;
        _lastLoggedFailure = signature;
        _errorLogger(exception, context);
    }

    private sealed record ModelChoice(ControllerVisualModel? Model, string DisplayName);
}
