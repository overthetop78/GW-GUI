using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using GWGUI.App.Input;
using GWGUI.App.Localization;
using GWGUI.App.Rendering;
using GWGUI.App.Services;
using GWGUI.Domain.Settings;
using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using Microsoft.Win32;
using Ellipse = System.Windows.Shapes.Ellipse;

namespace GWGUI.App.Controls;

public sealed class AtariMachineView : UserControl
{
    private IAtariMachine _machine;
    private readonly Func<IAtariMachine> _machineFactory;
    private readonly AtariMachineConfiguration _configuration;
    private readonly IReadOnlyList<GlobalShortcutBinding> _globalShortcuts;
    private readonly string _quickStatePath;
    private readonly string _captureFolder;
    private readonly Dictionary<EmulationMediaSlot, AtariMediaConfiguration> _mountedMedia = [];
    private readonly Dictionary<EmulationMediaSlot, Ellipse> _mediaLeds = [];
    private readonly List<Button> _machineButtons = [];
    private readonly HashSet<EmulationKey> _keys = [];
    private readonly HashSet<Key> _pressedPhysicalKeys = [];
    private readonly HashSet<string> _activeGlobalShortcuts = new(StringComparer.Ordinal);
    private readonly AtariMachineShortcutActions _shortcutActions;
    private readonly RelativeMouseCapture _mouseCapture = new();
    private readonly DispatcherTimer _inputTimer = new()
    {
        Interval = ControlTechnicalConstants.EmulationInputPollingInterval
    };
    private IEmulationVideoSurface _videoSurface;
    private FrameworkElement _display;
    private readonly Grid _videoHost = new() { Background = Brushes.Black };
    private readonly Border _screen;
    private readonly Grid _displayHost;
    private readonly TextBlock _status = new() { VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock _renderer = new() { VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBlock _controller = StatusIcon(AtariMachineViewConstants.ControllerGlyph);
    private readonly TextBlock _mouse = StatusIcon(AtariMachineViewConstants.MouseGlyph);
    private readonly StackPanel _mediaStrip = new() { Orientation = Orientation.Horizontal };
    private readonly Button _power;
    private readonly Button _pause;
    private readonly Button _audio;
    private readonly Button _quickSave;
    private readonly Button _quickLoad;
    private Window? _fullscreenWindow;
    private Grid? _fullscreenHost;
    private bool _poweredOff;
    private bool _disposed;
    private bool _audioMuted;
    private bool _joyMouseSwitchPressed;
    private int _framePending;
    private int _framesInWindow;
    private long _frameWindowStarted = Stopwatch.GetTimestamp();
    private double _measuredFramesPerSecond;

    public AtariMachineView(IAtariMachine machine, Func<IAtariMachine> machineFactory,
        AtariMachineConfiguration configuration, IReadOnlyDictionary<string, string>? globalShortcuts,
        string quickStatePath, string captureFolder)
    {
        _machine = machine;
        _machineFactory = machineFactory;
        _configuration = configuration;
        _globalShortcuts = EmulationShortcutMap.GlobalShortcuts(globalShortcuts);
        _quickStatePath = quickStatePath;
        _captureFolder = captureFolder;
        AtariAccessibilityFunctions.ConfigureFlowDirection(this);
        _shortcutActions = new AtariMachineShortcutActions(TogglePowerAsync, TogglePauseAsync,
            () => _machine.SoftResetAsync().AsTask(), () => _machine.HardResetAsync().AsTask(),
            QuickSaveAsync, QuickLoadAsync, SaveScreenshotAsync, ToggleFullscreenAsync,
            ReleaseMouse, ToggleAudioAsync);
        foreach (var media in configuration.Media.Where(item => item.IsInserted)) _mountedMedia[media.Slot] = media;
        _videoSurface = CreateVideoSurface(configuration.VideoRenderer);
        _display = _videoSurface.View;

        var root = new Grid { Background = Brushes.Transparent };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var toolbar = new DockPanel { Height = 34, LastChildFill = true, Margin = new Thickness(0, 0, 0, 2) };
        AtariAccessibilityFunctions.Configure(toolbar, L(AtariEmulationConstants.MachinesAutomationResource));
        var left = new StackPanel { Orientation = Orientation.Horizontal };
        _power = IconButton(AtariMachineViewConstants.PowerGlyph, AtariMachineViewConstants.PowerResource,
            TogglePowerAsync, false);
        _power.Foreground = Brushes.LimeGreen;
        _pause = IconButton(AtariMachineViewConstants.PauseGlyph, AtariMachineViewConstants.PauseResource,
            TogglePauseAsync);
        left.Children.Add(ToolbarGroup(_power, _pause,
            IconButton(AtariMachineViewConstants.SoftResetGlyph, AtariMachineViewConstants.SoftResetResource,
                () => _machine.SoftResetAsync().AsTask()),
            IconButton(AtariMachineViewConstants.HardResetGlyph, AtariMachineViewConstants.HardResetResource,
                () => _machine.HardResetAsync().AsTask())));
        _quickSave = IconButton(AtariMachineViewConstants.QuickSaveGlyph,
            AtariMachineViewConstants.QuickSaveResource, QuickSaveAsync);
        _quickLoad = IconButton(AtariMachineViewConstants.QuickLoadGlyph,
            AtariMachineViewConstants.QuickLoadResource, QuickLoadAsync);
        left.Children.Add(ToolbarGroup(_quickSave, _quickLoad));
        left.Children.Add(ToolbarGroup(
            IconButton(AtariMachineViewConstants.ScreenshotGlyph, AtariMachineViewConstants.ScreenshotResource,
                SaveScreenshotAsync),
            IconButton(AtariMachineViewConstants.FullscreenGlyph, AtariMachineViewConstants.FullscreenResource,
                ToggleFullscreenAsync, false)));
        var stateShortcuts = EmulationShortcutViewFunctions.CreateGroup(_globalShortcuts,
            (EmulationShortcutDefaults.QuickSave, EmulationResourceKeys.QuickSave),
            (EmulationShortcutDefaults.QuickLoad, EmulationResourceKeys.QuickLoad));
        left.Children.Add(stateShortcuts);
        DockPanel.SetDock(left, Dock.Left);
        toolbar.Children.Add(left);

        var right = new StackPanel { Orientation = Orientation.Horizontal };
        var displayShortcuts = EmulationShortcutViewFunctions.CreateGroup(_globalShortcuts,
            (EmulationShortcutDefaults.ToggleFullscreen, EmulationResourceKeys.Fullscreen),
            (EmulationShortcutDefaults.ReleaseMouse, EmulationResourceKeys.ReleaseMouse));
        right.Children.Add(displayShortcuts);
        _audio = IconButton(AtariMachineViewConstants.AudioGlyph, AtariMachineViewConstants.AudioResource,
            ToggleAudioAsync);
        _audio.IsEnabled = configuration.AudioEnabled;
        var inputStatus = new List<UIElement> { _audio };
        if (configuration.Core == AtariCoreKind.Hatari)
            inputStatus.Add(IconButton(AtariMachineViewConstants.JoyMouseSwitchGlyph,
                "Emulation.Controller.Action.SwitchJoystickMouse", SwitchJoystickMouseAsync));
        inputStatus.Add(_controller);
        inputStatus.Add(_mouse);
        right.Children.Add(ToolbarGroup(inputStatus.ToArray()));
        right.Children.Add(ToolbarGroup(_status));
        DockPanel.SetDock(right, Dock.Right);
        toolbar.Children.Add(right);
        _renderer.Text = AtariMachineViewFunctions.RendererName(_videoSurface.Renderer);
        AtariAccessibilityFunctions.Configure(_renderer, L(AtariMachineViewConstants.RenderingResource));
        AtariAccessibilityFunctions.Configure(_status, L(AtariEmulationConstants.MachinesAutomationResource));
        AtariAccessibilityFunctions.Configure(_controller,
            L(AtariInputSettingsConstants.ControllersTabResource));
        AtariAccessibilityFunctions.Configure(_mouse, L(AtariInputSettingsConstants.MouseTabResource));
        AutomationProperties.SetItemStatus(_controller,
            L(AtariVideoAudioSettingsConstants.DisabledResource));
        AutomationProperties.SetItemStatus(_mouse,
            L(AtariVideoAudioSettingsConstants.DisabledResource));
        var rendererGroup = ToolbarGroup(new TextBlock
        {
            Text = AtariMachineViewConstants.RendererGlyph,
            FontFamily = ControlVisualConstants.IconFont,
            VerticalAlignment = VerticalAlignment.Center
        }, _renderer);
        rendererGroup.Padding = new Thickness(16, 1, 16, 1);
        rendererGroup.Margin = new Thickness(12, 1, 12, 1);
        toolbar.Children.Add(rendererGroup);
        toolbar.SizeChanged += (_, args) =>
        {
            var visible = args.NewSize.Width >= AtariMachineViewConstants.WideToolbarMinimumWidth
                ? Visibility.Visible : Visibility.Collapsed;
            stateShortcuts.Visibility = visible;
            displayShortcuts.Visibility = visible;
        };
        root.Children.Add(toolbar);

        _videoHost.Children.Add(_display);
        _screen = new Border
        {
            Background = Brushes.Black,
            Child = _videoHost,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SnapsToDevicePixels = true
        };
        _displayHost = new Grid { Background = new SolidColorBrush(Color.FromRgb(43, 46, 50)) };
        _displayHost.FlowDirection = FlowDirection.LeftToRight;
        AtariAccessibilityFunctions.Configure(_displayHost,
            L(AtariEmulationConstants.MachinesAutomationResource));
        _displayHost.Children.Add(_screen);
        _displayHost.SizeChanged += (_, _) => FitScreen();
        Grid.SetRow(_displayHost, 1);
        root.Children.Add(_displayHost);

        var bottom = new Border
        {
            Height = 24, Child = _mediaStrip, Padding = new Thickness(4, 1, 4, 1),
            BorderThickness = new Thickness(1, 1, 1, 0)
        };
        bottom.SetResourceReference(Border.BorderBrushProperty, AtariMachineViewConstants.BorderBrushResource);
        BuildMediaStrip();
        Grid.SetRow(bottom, 2);
        root.Children.Add(bottom);
        Content = root;

        AttachMachine();
        AttachInput();
        _inputTimer.Tick += InputTimerTick;
        PreviewKeyDown += KeyDownHandler;
        PreviewKeyUp += KeyUpHandler;
        Unloaded += (_, _) => ReleaseMouse();
    }

    public async Task StartAsync()
    {
        ResetFrameRate();
        await _machine.StartAsync();
        _inputTimer.Start();
        SetPowered(true);
        _display.Focus();
    }

    public async Task StopAsync()
    {
        if (_disposed) return;
        if (_fullscreenWindow is not null) ExitFullscreen();
        _inputTimer.Stop();
        ReleaseMouse();
        DetachMachine();
        try { await _machine.StopAsync(); }
        finally
        {
            await _machine.DisposeAsync();
            _videoSurface.Dispose();
            _disposed = true;
        }
    }

    public void ApplyVideoRenderer(EmulationVideoRenderer renderer)
    {
        if (_videoSurface.Renderer == renderer) return;
        ReleaseMouse();
        var replacement = CreateVideoSurface(renderer);
        var previous = _videoSurface;
        DetachDisplayInput();
        _videoHost.Children.Clear();
        _videoSurface = replacement;
        _display = replacement.View;
        _videoHost.Children.Add(_display);
        AttachInput();
        previous.Dispose();
        _renderer.Text = AtariMachineViewFunctions.RendererName(_videoSurface.Renderer);
        if (_machine.LatestVideoFrame is { } frame) _videoSurface.Present(frame);
    }

    private void BuildMediaStrip()
    {
        _mediaStrip.Children.Clear();
        _mediaLeds.Clear();
        foreach (var view in AtariMachineViewFunctions.Media(_configuration))
            _mediaStrip.Children.Add(MediaItem(view));
    }

    private FrameworkElement MediaItem(AtariMachineMediaView view)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };
        var led = new Ellipse
        {
            Width = 10, Height = 10,
            Fill = _mountedMedia.ContainsKey(view.Configuration.Slot) || !view.Removable
                ? Brushes.ForestGreen : Brushes.Gray,
            Margin = new Thickness(0, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center
        };
        _mediaLeds[view.Configuration.Slot] = led;
        AtariAccessibilityFunctions.Configure(led, view.Label);
        AutomationProperties.SetItemStatus(led,
            L(_mountedMedia.ContainsKey(view.Configuration.Slot)
                ? AtariVideoAudioSettingsConstants.EnabledResource
                : AtariVideoAudioSettingsConstants.DisabledResource));
        panel.Children.Add(led);
        var media = new Button
        {
            ToolTip = view.Removable ? L(AtariMachineViewConstants.BrowseResource) : view.Label,
            Height = 20, MinHeight = 0, MinWidth = 0, Padding = new Thickness(2, 0, 2, 0),
            Background = Brushes.Transparent, BorderBrush = Brushes.Transparent,
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock { Text = view.Glyph, FontFamily = ControlVisualConstants.IconFont,
                        Margin = new Thickness(0, 0, 4, 0) },
                    new TextBlock { Text = view.Label, FontWeight = FontWeights.SemiBold }
                }
            }
        };
        if (view.Removable) media.Click += async (_, _) => await RunAsync(() => InsertMediaAsync(view.Configuration));
        panel.Children.Add(media);
        if (view.Removable)
        {
            var eject = IconButton(AtariMachineViewConstants.EjectGlyph, AtariMachineViewConstants.EjectResource,
                () => EjectMediaAsync(view.Configuration.Slot));
            _machineButtons.Remove(eject);
            eject.Width = 22;
            eject.Height = 20;
            eject.IsEnabled = _mountedMedia.ContainsKey(view.Configuration.Slot);
            panel.Children.Add(eject);
        }
        return new Border { Child = panel, Padding = new Thickness(4, 0, 4, 0), Margin = new Thickness(0, 0, 3, 0) };
    }

    private async Task InsertMediaAsync(AtariMediaConfiguration template)
    {
        var dialog = new OpenFileDialog { Filter = L(AtariMachineViewConstants.MediaFilterResource) };
        if (dialog.ShowDialog() != true) return;
        var media = template with { Path = dialog.FileName, IsInserted = true };
        _mountedMedia[media.Slot] = media;
        if (!_poweredOff) await _machine.InsertMediaAsync(media);
        BuildMediaStrip();
    }

    private async Task EjectMediaAsync(EmulationMediaSlot slot)
    {
        if (!_poweredOff) await _machine.EjectMediaAsync(slot);
        _mountedMedia.Remove(slot);
        BuildMediaStrip();
    }

    private async Task RestoreMountedMediaAsync()
    {
        foreach (var media in _mountedMedia.Values.OrderBy(item => item.Slot))
            await _machine.InsertMediaAsync(media with { IsInserted = true });
    }

    private void VideoFrameReady(object? sender, VideoFrame frame)
    {
        Interlocked.Increment(ref _framesInWindow);
        if (Interlocked.Exchange(ref _framePending, AtariMachineViewConstants.ActiveFramePending) !=
            AtariMachineViewConstants.InactiveFramePending) return;
        Dispatcher.BeginInvoke(() =>
        {
            try { Render(_machine.LatestVideoFrame ?? frame); }
            finally { Interlocked.Exchange(ref _framePending, AtariMachineViewConstants.InactiveFramePending); }
        });
    }

    private void Render(VideoFrame frame)
    {
        try { _videoSurface.Present(frame); }
        catch when (_videoSurface.Renderer != EmulationVideoRenderer.Wpf)
        {
            ApplyVideoRenderer(EmulationVideoRenderer.Wpf);
            _videoSurface.Present(frame);
        }
        UpdateFrameRate();
        var status = AtariMachineViewFunctions.Status(_machine.RuntimeStatus, frame,
            _measuredFramesPerSecond, _audioMuted, _mouseCapture.IsCaptured,
            XInputControllerReader.ReadAll().Any(item => item != EmulationControllerState.Empty));
        _status.Text = status.Text;
        AutomationProperties.SetItemStatus(_status, status.Text);
        _audio.Opacity = status.AudioActive ? AtariMachineViewConstants.ActiveOpacity : AtariMachineViewConstants.InactiveOpacity;
        _mouse.Opacity = status.MouseAvailable ? AtariMachineViewConstants.ActiveOpacity : AtariMachineViewConstants.InactiveOpacity;
        _controller.Opacity = status.ControllerAvailable ? AtariMachineViewConstants.ActiveOpacity : AtariMachineViewConstants.InactiveOpacity;
        AutomationProperties.SetItemStatus(_audio, L(status.AudioActive
            ? AtariVideoAudioSettingsConstants.EnabledResource : AtariVideoAudioSettingsConstants.DisabledResource));
        AutomationProperties.SetItemStatus(_mouse, L(status.MouseAvailable
            ? AtariVideoAudioSettingsConstants.EnabledResource : AtariVideoAudioSettingsConstants.DisabledResource));
        AutomationProperties.SetItemStatus(_controller, L(status.ControllerAvailable
            ? AtariVideoAudioSettingsConstants.EnabledResource : AtariVideoAudioSettingsConstants.DisabledResource));
        foreach (var led in _mediaLeds)
        {
            led.Value.Fill = status.MediaActivity.GetValueOrDefault(led.Key)
                ? Brushes.LimeGreen : _mountedMedia.ContainsKey(led.Key) ? Brushes.ForestGreen : Brushes.Gray;
            AutomationProperties.SetItemStatus(led.Value, L(status.MediaActivity.GetValueOrDefault(led.Key)
                ? AtariVideoAudioSettingsConstants.EnabledResource
                : AtariVideoAudioSettingsConstants.DisabledResource));
        }
        FitScreen(status.AspectRatio);
    }

    internal async Task TogglePowerAsync()
    {
        ReleaseMouse();
        if (!_poweredOff)
        {
            _inputTimer.Stop();
            DetachMachine();
            await _machine.StopAsync();
            await _machine.DisposeAsync();
            _poweredOff = true;
            SetPowered(false);
            return;
        }
        _machine = _machineFactory();
        AttachMachine();
        await _machine.StartAsync();
        _machine.SetAudioMuted(_audioMuted);
        await RestoreMountedMediaAsync();
        ResetFrameRate();
        _inputTimer.Start();
        _poweredOff = false;
        SetPowered(true);
    }

    private async Task TogglePauseAsync()
    {
        if (_machine.State == EmulationMachineState.Running)
        {
            ReleaseMouse();
            await _machine.PauseAsync();
            SetIcon(_pause, AtariMachineViewConstants.ContinueGlyph, AtariMachineViewConstants.ContinueResource);
        }
        else if (_machine.State == EmulationMachineState.Paused)
        {
            await _machine.ResumeAsync();
            SetIcon(_pause, AtariMachineViewConstants.PauseGlyph, AtariMachineViewConstants.PauseCommandResource);
        }
    }

    private Task ToggleAudioAsync()
    {
        _audioMuted = !_audioMuted;
        _machine.SetAudioMuted(_audioMuted);
        SetIcon(_audio, _audioMuted ? AtariMachineViewConstants.MutedGlyph : AtariMachineViewConstants.AudioGlyph,
            AtariMachineViewConstants.MuteResource);
        return Task.CompletedTask;
    }

    private async Task QuickSaveAsync()
    {
        if (!_machine.SupportsSaveStates) return;
        Directory.CreateDirectory(Path.GetDirectoryName(_quickStatePath)!);
        await _machine.SaveStateAsync(_quickStatePath);
        _quickLoad.IsEnabled = true;
    }

    private Task QuickLoadAsync() => _machine.SupportsSaveStates && File.Exists(_quickStatePath)
        ? _machine.LoadStateAsync(_quickStatePath).AsTask() : Task.CompletedTask;

    private Task SaveScreenshotAsync()
    {
        var snapshot = _videoSurface.Snapshot;
        if (snapshot is null) throw new InvalidOperationException(L(AtariMachineViewConstants.UnavailableResource));
        AtariMachineViewFunctions.SaveScreenshot(snapshot, _captureFolder, DateTime.Now);
        return Task.CompletedTask;
    }

    internal Task ToggleFullscreenAsync()
    {
        if (_fullscreenWindow is null) EnterFullscreen(); else ExitFullscreen();
        return Task.CompletedTask;
    }

    internal bool IsFullscreen => _fullscreenWindow is not null;

    private void EnterFullscreen()
    {
        _displayHost.Children.Remove(_screen);
        _fullscreenHost = new Grid { Background = Brushes.Black };
        _fullscreenHost.Children.Add(_screen);
        _fullscreenHost.SizeChanged += (_, _) => FitScreen();
        _fullscreenWindow = new Window
        {
            Title = AtariMachineViewConstants.AtariTitle,
            Content = _fullscreenHost,
            WindowStyle = WindowStyle.None,
            WindowState = WindowState.Maximized,
            Background = Brushes.Black
        };
        _fullscreenWindow.Closed += (_, _) => ExitFullscreen();
        _fullscreenWindow.Show();
        FitScreen();
    }

    private void ExitFullscreen()
    {
        if (_fullscreenWindow is null) return;
        var window = _fullscreenWindow;
        _fullscreenHost?.Children.Remove(_screen);
        _fullscreenWindow = null;
        _fullscreenHost = null;
        _displayHost.Children.Add(_screen);
        if (window.IsVisible) window.Close();
        FitScreen();
    }

    private void FitScreen(double? aspectRatio = null)
    {
        var host = _fullscreenHost ?? _displayHost;
        var frame = _machine.LatestVideoFrame;
        var fitted = AtariMachineViewFunctions.Fit(host.ActualWidth, host.ActualHeight,
            (float)(aspectRatio ?? frame?.AspectRatio ?? AtariMachineViewConstants.DefaultAspectRatio));
        if (fitted.IsEmpty) return;
        _screen.Width = fitted.Width;
        _screen.Height = fitted.Height;
    }

    private void AttachMachine() => _machine.VideoFrameReady += VideoFrameReady;
    private void DetachMachine() => _machine.VideoFrameReady -= VideoFrameReady;

    private void AttachInput()
    {
        _display.KeyDown += KeyDownHandler;
        _display.KeyUp += KeyUpHandler;
        _display.MouseDown += DisplayMouseDown;
        _display.MouseMove += DisplayMouseMove;
        _display.MouseWheel += DisplayMouseWheel;
    }

    private void DetachDisplayInput()
    {
        _display.KeyDown -= KeyDownHandler;
        _display.KeyUp -= KeyUpHandler;
        _display.MouseDown -= DisplayMouseDown;
        _display.MouseMove -= DisplayMouseMove;
        _display.MouseWheel -= DisplayMouseWheel;
    }

    private void DisplayMouseDown(object sender, MouseButtonEventArgs args)
    {
        _display.Focus();
        if (_configuration.Input.CaptureMouse && !_mouseCapture.IsCaptured)
            _mouseCapture.Capture(_display, _screen, _videoSurface.InputHandle);
        PublishInput();
    }

    private void DisplayMouseMove(object sender, MouseEventArgs args) =>
        _mouseCapture.ProcessMovement(_screen, (x, y) => PublishInput(x, y));

    private void DisplayMouseWheel(object sender, MouseWheelEventArgs args) =>
        PublishInput(wheel: args.Delta);

    private void KeyDownHandler(object sender, KeyEventArgs args)
    {
        var source = args.Key == Key.System ? args.SystemKey : args.Key;
        if (!KeyboardChord.IsModifierKey(source)) _pressedPhysicalKeys.Add(source);
        var global = EmulationShortcutFunctions.ResolveGlobal(_globalShortcuts, Keyboard.Modifiers,
            _pressedPhysicalKeys, source, _activeGlobalShortcuts);
        if (global.Kind == EmulationShortcutMatchKind.Global)
        {
            if (global.ShouldExecute && global.Action is not null && _activeGlobalShortcuts.Add(global.Action))
                _ = ExecuteGlobalShortcutAsync(global.Action);
            args.Handled = true;
            return;
        }
        if (global.Kind == EmulationShortcutMatchKind.ReservedForGlobal)
        {
            args.Handled = true;
            return;
        }
        if (!AtariMachineInputFunctions.TryMap(source, out var key)) return;
        _keys.Add(AtariMachineInputFunctions.Resolve(key, _configuration.Input.KeyboardMappings));
        PublishInput();
        args.Handled = true;
    }

    private void KeyUpHandler(object sender, KeyEventArgs args)
    {
        var source = args.Key == Key.System ? args.SystemKey : args.Key;
        _pressedPhysicalKeys.Remove(source);
        EmulationShortcutFunctions.ReleaseInactive(_activeGlobalShortcuts, _globalShortcuts,
            Keyboard.Modifiers, _pressedPhysicalKeys);
        if (!AtariMachineInputFunctions.TryMap(source, out var key)) return;
        _keys.Remove(AtariMachineInputFunctions.Resolve(key, _configuration.Input.KeyboardMappings));
        PublishInput();
        args.Handled = true;
    }

    private async Task ExecuteGlobalShortcutAsync(string action)
    {
        try
        {
            await AtariMachineViewFunctions.ExecuteShortcutAsync(action, _shortcutActions);
        }
        catch (Exception error) { ShowError(error); }
    }

    private void InputTimerTick(object? sender, EventArgs args) => PublishInput();

    private void PublishInput(int deltaX = RelativeMouseCaptureConstants.NoMovement,
        int deltaY = RelativeMouseCaptureConstants.NoMovement,
        int wheel = RelativeMouseCaptureConstants.NoMovement)
    {
        if (_poweredOff || _disposed) return;
        var controllers = XInputControllerReader.ReadAll();
        var snapshot = AtariMachineInputFunctions.Snapshot(_keys, deltaX, deltaY, wheel,
            _mouseCapture.IsCaptured, controllers, _configuration.Input, _configuration.Model);
        if (_joyMouseSwitchPressed)
        {
            var mapped = snapshot.Controllers.ToArray();
            if (mapped.Length == 0) mapped = [EmulationControllerState.Empty];
            mapped[0] = mapped[0] with { Buttons = mapped[0].Buttons | (1u << 2) };
            snapshot = snapshot with { Controllers = mapped };
        }
        _machine.SetInput(snapshot);
    }

    private async Task SwitchJoystickMouseAsync()
    {
        _joyMouseSwitchPressed = true;
        PublishInput();
        await Task.Delay(100);
        _joyMouseSwitchPressed = false;
        if (!_disposed) PublishInput();
    }

    private void ReleaseMouse()
    {
        if (_mouseCapture.IsCaptured) _mouseCapture.Release(_display, _videoSurface.InputHandle);
        _mouse.Opacity = AtariMachineViewConstants.InactiveOpacity;
    }

    private void SetPowered(bool powered)
    {
        _power.Foreground = powered ? Brushes.LimeGreen : Brushes.Gray;
        AutomationProperties.SetItemStatus(_power, L(powered
            ? AtariVideoAudioSettingsConstants.EnabledResource
            : AtariVideoAudioSettingsConstants.DisabledResource));
        foreach (var button in _machineButtons) button.IsEnabled = powered;
        _quickSave.IsEnabled = powered && _machine.SupportsSaveStates;
        _quickLoad.IsEnabled = powered && _machine.SupportsSaveStates && File.Exists(_quickStatePath);
        _audio.IsEnabled = powered && _configuration.AudioEnabled;
        _videoHost.Visibility = powered ? Visibility.Visible : Visibility.Hidden;
        if (!powered) _status.Text = string.Empty;
    }

    private void ResetFrameRate()
    {
        Interlocked.Exchange(ref _framesInWindow, AtariMachineViewConstants.InactiveFramePending);
        _frameWindowStarted = Stopwatch.GetTimestamp();
        _measuredFramesPerSecond = AtariMachineViewConstants.EmptyMeasurement;
    }

    private void UpdateFrameRate()
    {
        var now = Stopwatch.GetTimestamp();
        var elapsed = Stopwatch.GetElapsedTime(_frameWindowStarted, now);
        if (elapsed < TimeSpan.FromSeconds(AtariMachineViewConstants.FrameRateWindowSeconds)) return;
        var frames = Interlocked.Exchange(ref _framesInWindow, AtariMachineViewConstants.InactiveFramePending);
        _measuredFramesPerSecond = frames / elapsed.TotalSeconds;
        _frameWindowStarted = now;
    }

    private Button IconButton(string glyph, string tooltipResource, Func<Task> action, bool requiresPower = true)
    {
        var button = new Button
        {
            Content = new TextBlock { Text = glyph, FontFamily = ControlVisualConstants.IconFont },
            ToolTip = L(tooltipResource), Width = 28, Height = 28, MinWidth = 0, MinHeight = 0,
            Padding = new Thickness(2), Margin = new Thickness(0, 0, 2, 0)
        };
        button.SetResourceReference(StyleProperty, AtariMachineViewConstants.StatusIconButtonStyleResource);
        AtariAccessibilityFunctions.Configure(button, L(tooltipResource));
        if (requiresPower) _machineButtons.Add(button);
        button.Click += async (_, _) => await ButtonAsyncAction.RunAsync(
            button, action, ShowError, restoreEnabled: () => !_disposed);
        return button;
    }

    private async Task RunAsync(Func<Task> action)
    {
        try { await action(); }
        catch (Exception error) { ShowError(error); }
    }

    private void ShowError(Exception error) => ControlErrorPresenter.ShowDetailed(this, error,
        AtariErrorLocalizationFunctions.Describe(error),
        AtariMachineViewConstants.CommandErrorContext, AtariMachineViewConstants.AtariTitle);

    private static Border ToolbarGroup(params UIElement[] children)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        foreach (var child in children) panel.Children.Add(child);
        var border = new Border
        {
            Child = panel, Height = 32, BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6),
            Padding = new Thickness(2, 1, 2, 1), Margin = new Thickness(2, 1, 2, 1),
            VerticalAlignment = VerticalAlignment.Center
        };
        border.SetResourceReference(Border.BackgroundProperty, AtariMachineViewConstants.CardBrushResource);
        border.SetResourceReference(Border.BorderBrushProperty, AtariMachineViewConstants.BorderBrushResource);
        return border;
    }

    private static TextBlock StatusIcon(string glyph) => new()
    {
        Text = glyph, FontFamily = ControlVisualConstants.IconFont, VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(4, 0, 4, 0), Opacity = AtariMachineViewConstants.InactiveOpacity
    };

    private static void SetIcon(Button button, string glyph, string tooltipResource)
    {
        if (button.Content is TextBlock icon) icon.Text = glyph;
        var label = L(tooltipResource);
        button.ToolTip = label;
        AutomationProperties.SetName(button, label);
    }

    private static IEmulationVideoSurface CreateVideoSurface(EmulationVideoRenderer renderer)
    {
        try { return EmulationVideoSurfaceFactory.Create(renderer); }
        catch { return EmulationVideoSurfaceFactory.Create(EmulationVideoRenderer.Wpf); }
    }

    private static string L(string resource) => LocExtension.Get(resource);
}
