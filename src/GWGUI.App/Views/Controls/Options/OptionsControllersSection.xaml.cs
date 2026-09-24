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
    private readonly Dictionary<string, ControllerVisualModel> _visualOverrides;
    private readonly IControllerProfileStore _profiles;
    private readonly Func<int, Task> _delay;
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
        Action<Exception, string>? errorLogger = null,
        IControllerProfileStore? profiles = null,
        Func<int, Task>? delay = null)
    {
        _source = source;
        _profiles = profiles ?? new ControllerProfileStore();
        _delay = delay ?? Task.Delay;
        _visualOverrides = _profiles.GetModels().ToDictionary(item => item.Key, item => item.Value,
            StringComparer.OrdinalIgnoreCase);
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

    private sealed record ModelChoice(ControllerVisualModel? Model, string DisplayName);
}
