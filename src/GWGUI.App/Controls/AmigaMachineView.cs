using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Ellipse = System.Windows.Shapes.Ellipse;
using System.Windows.Threading;
using GWGUI.App.Localization;
using GWGUI.App.Input;
using GWGUI.App.Rendering;
using GWGUI.App.Services;
using GWGUI.Domain.Settings;
using GWGUI.Emulation;
using GWGUI.Emulation.Amiga;
using GWGUI.MediaEngine.Definitions;
using Microsoft.Win32;

namespace GWGUI.App.Controls;

public sealed class AmigaMachineView : UserControl
{
    private IAmigaMachine _machine;
    private readonly Func<IAmigaMachine> _machineFactory;
    private readonly AmigaInputConfiguration _input;
    private readonly IReadOnlyDictionary<EmulationKey, EmulationKey> _keyboardMap;
    private readonly IReadOnlyList<KeyboardShortcutBinding> _keyboardShortcuts;
    private readonly IReadOnlyList<GlobalShortcutBinding> _globalShortcuts;
    private readonly string _quickStatePath;
    private readonly string _captureFolder;
    private IEmulationVideoSurface _videoSurface;
    private FrameworkElement _display;
    private readonly Grid _videoHost = new() { Background = Brushes.Black };
    private readonly Border _screen;
    private readonly TextBlock _status = new() { VerticalAlignment = VerticalAlignment.Center };
    private readonly Button _audioStatus;
    private readonly TextBlock _controllerStatus = StatusIcon("\uE7FC");
    private readonly TextBlock _mouseStatus = StatusIcon("\uE962");
    private readonly TextBlock _rendererStatus = new() { VerticalAlignment = VerticalAlignment.Center, FontWeight = FontWeights.SemiBold };
    private readonly StackPanel _deviceStrip = new() { Orientation = Orientation.Horizontal };
    private readonly AmigaMachineConfiguration _configuration;
    private readonly HashSet<string> _insertedMedia = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _mountedMedia = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Ellipse> _deviceLeds = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DateTime> _deviceActivityUntil = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Button> _machineCommandButtons = [];
    private readonly HashSet<EmulationKey> _keys = [];
    private readonly HashSet<EmulationKey> _hostKeys = [];
    private readonly Dictionary<Key, EmulationKey> _pressedShortcutKeys = [];
    private readonly HashSet<Key> _pressedPhysicalKeys = [];
    private readonly HashSet<string> _activeGlobalShortcuts = new(StringComparer.Ordinal);
    private int _framePending;
    private long _frameRateWindowStarted = Stopwatch.GetTimestamp();
    private int _framesProducedInWindow;
    private double _measuredFramesPerSecond;
    private bool _disposed;
    private readonly RelativeMouseCapture _mouseCapture = new();
    private bool _poweredOff;
    private Button? _pauseButton;
    private Button? _powerButton;
    private readonly Grid _root;
    private readonly DockPanel _toolbar;
    private readonly Grid _displayHost;
    private readonly Border _bottomBar;
    private Window? _fullscreenWindow;
    private Grid? _fullscreenHost;
    private bool _closingFullscreen;
    private bool _audioMuted;
    private readonly DispatcherTimer _inputTimer = new() { Interval = ControlTechnicalConstants.EmulationInputPollingInterval };
    private HwndSource? _windowSource;

    public AmigaMachineView(IAmigaMachine machine, Func<IAmigaMachine> machineFactory,
        AmigaMachineConfiguration configuration,
        AmigaInputConfiguration? input = null,
        IReadOnlyDictionary<string, string>? globalShortcuts = null, string? quickStatePath = null,
        string? captureFolder = null)
    {
        _machine = machine;
        _machineFactory = machineFactory;
        _configuration = configuration;
        var mediaIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in configuration.Media ?? [])
        {
            var prefix = item.Kind == AmigaMediaKind.CompactDisc ? "CD" : item.Kind == AmigaMediaKind.Floppy ? "DF" : null;
            if (prefix is null) continue;
            var index = mediaIndexes.GetValueOrDefault(prefix);
            mediaIndexes[prefix] = index + 1;
            var deviceName = $"{prefix}{index}:";
            _insertedMedia.Add(deviceName);
            _mountedMedia[deviceName] = item.Path;
        }
        if (_mountedMedia.Count == 0 && !string.IsNullOrWhiteSpace(configuration.InitialDiskPath))
        {
            _insertedMedia.Add("DF0:");
            _mountedMedia["DF0:"] = configuration.InitialDiskPath;
        }
        _input = input ?? new AmigaInputConfiguration();
        _videoSurface = CreateVideoSurface(configuration.VideoRenderer);
        _display = _videoSurface.View;
        _keyboardMap = EmulationShortcutMap.KeyboardMap(_input.KeyboardMappings);
        _keyboardShortcuts = EmulationShortcutMap.KeyboardShortcuts(_input.KeyboardBindings);
        _globalShortcuts = EmulationShortcutMap.GlobalShortcuts(globalShortcuts);
        _quickStatePath = quickStatePath ?? Path.Combine(Path.GetTempPath(), "gwgui-amiga-quick.gwas");
        _captureFolder = captureFolder ?? Path.Combine(Path.GetTempPath(), "GW GUI", "Captures");
        _root = new Grid { Background = Brushes.Transparent };
        _root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _root.RowDefinitions.Add(new RowDefinition());
        _root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _toolbar = new DockPanel { Height = 34, LastChildFill = true, Margin = new Thickness(0, 0, 0, 2) };
        var left = new StackPanel { Orientation = Orientation.Horizontal };
        _powerButton = IconButton("\uE7E8", "Emulation.Shortcut.Power", TogglePowerAsync, requiresPower: false);
        _powerButton.Foreground = Brushes.LimeGreen;
        _pauseButton = IconButton("\uE769", "Emulation.Shortcut.PauseResume", TogglePauseAsync);
        _pauseButton.Foreground = Brushes.DarkOrange;
        var softReset = IconButton("\uE777", "Emulation.Shortcut.SoftReset", () => _machine.SoftResetAsync().AsTask());
        softReset.Foreground = new SolidColorBrush(Color.FromRgb(120, 160, 48));
        var hardReset = IconButton("\uE72C", "Emulation.Shortcut.HardReset", () => _machine.HardResetAsync().AsTask());
        hardReset.Foreground = new SolidColorBrush(Color.FromRgb(220, 92, 48));
        left.Children.Add(ToolbarGroup(_powerButton, _pauseButton, softReset, hardReset));
        left.Children.Add(ToolbarGroup(
            IconButton("\uE74E", "Emulation.Shortcut.QuickSave", QuickSave),
            IconButton("\uE8E5", "Emulation.Shortcut.QuickLoad", QuickLoad)));
        left.Children.Add(ToolbarGroup(IconButton("\uE722", "Emulation.Shortcut.Screenshot", () => { SaveScreenshot(); return Task.CompletedTask; })));
        left.Children.Add(ToolbarGroup(IconButton("\uE740", "Emulation.Shortcut.Fullscreen", () => { ToggleFullscreen(); return Task.CompletedTask; })));
        var stateShortcuts = ShortcutGroup(
            (EmulationShortcutDefaults.QuickSave, EmulationResourceKeys.QuickSave),
            (EmulationShortcutDefaults.QuickLoad, EmulationResourceKeys.QuickLoad));
        left.Children.Add(stateShortcuts);
        DockPanel.SetDock(left, Dock.Left);
        _toolbar.Children.Add(left);
        var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        _audioStatus = IconButton("\uE767", "Emulation.AudioTab", ToggleAudioMute, requiresPower: true);
        _audioStatus.Width = 28;
        var displayShortcuts = ShortcutGroup(
            (EmulationShortcutDefaults.ToggleFullscreen, EmulationResourceKeys.Fullscreen),
            (EmulationShortcutDefaults.ReleaseMouse, EmulationResourceKeys.ReleaseMouse));
        right.Children.Add(displayShortcuts);
        right.Children.Add(ToolbarGroup(_audioStatus, _controllerStatus, _mouseStatus));
        _status.Margin = new Thickness(7, 0, 7, 0);
        right.Children.Add(ToolbarGroup(_status));
        DockPanel.SetDock(right, Dock.Right); _toolbar.Children.Add(right);
        _rendererStatus.Text = RendererName(_videoSurface.Renderer);
        var rendererGroup = CenteredToolbarGroup(new TextBlock
        {
            Text = "\uE7F4", FontFamily = ControlVisualConstants.IconFont, FontSize = 15,
            Margin = new Thickness(4, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center
        }, _rendererStatus);
        rendererGroup.Padding = new Thickness(16, 1, 16, 1);
        rendererGroup.Margin = new Thickness(8, 1, 8, 1);
        _toolbar.Children.Add(rendererGroup);
        _toolbar.SizeChanged += (_, args) =>
        {
            var visibility = args.NewSize.Width >= 1450 ? Visibility.Visible : Visibility.Collapsed;
            stateShortcuts.Visibility = visibility;
            displayShortcuts.Visibility = visibility;
        };
        _root.Children.Add(_toolbar);
        _screen = new Border
        {
            Background = Brushes.Black,
            Child = _videoHost,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SnapsToDevicePixels = true
        };
        _videoHost.Children.Add(_display);
        _displayHost = new Grid { Background = new SolidColorBrush(Color.FromRgb(43, 46, 50)) };
        _displayHost.Children.Add(_screen);
        _displayHost.SizeChanged += (_, _) => FitScreen(_displayHost.ActualWidth, _displayHost.ActualHeight);
        Grid.SetRow(_displayHost, 1); _root.Children.Add(_displayHost);
        _bottomBar = new Border
        {
            Height = 24,
            BorderThickness = new Thickness(1, 1, 1, 0),
            BorderBrush = new SolidColorBrush(Color.FromRgb(215, 222, 231)),
            Child = _deviceStrip,
            Padding = new Thickness(4, 1, 4, 1)
        };
        BuildDeviceStrip();
        Grid.SetRow(_bottomBar, 2); _root.Children.Add(_bottomBar);
        Content = _root;

        _machine.VideoFrameReady += VideoFrameReady;
        AttachDisplayInputHandlers();
        _inputTimer.Tick += (_, _) => PublishInput();
        Loaded += (_, _) => AttachWindowHook();
        Unloaded += (_, _) => DetachWindowHook();
        PreviewKeyDown += DisplayKeyDown;
        PreviewKeyUp += DisplayKeyUp;
    }

    private void AttachDisplayInputHandlers()
    {
        _display.KeyDown += DisplayKeyDown;
        _display.KeyUp += DisplayKeyUp;
        _display.MouseMove += DisplayMouseMove;
        _display.MouseDown += MouseChanged;
        _display.MouseUp += MouseChanged;
        _display.MouseWheel += DisplayMouseWheel;
        _display.MouseDown += (_, _) =>
        {
            _display.Focus();
            if (_input.CaptureMouse && !_mouseCapture.IsCaptured) CaptureRelativeMouse();
        };
        if (_display is HwndHost host)
            host.MessageHook += NativeVideoMessage;
        else
            _display.LostKeyboardFocus += DisplayLostKeyboardFocus;
    }

    private IEmulationVideoSurface CreateVideoSurface(EmulationVideoRenderer renderer)
    {
        try { return EmulationVideoSurfaceFactory.Create(renderer); }
        catch { return EmulationVideoSurfaceFactory.Create(EmulationVideoRenderer.Wpf); }
    }

    private static TextBlock StatusIcon(string glyph) => new()
    {
        Text = glyph,
        FontFamily = ControlVisualConstants.IconFont,
        FontSize = 17,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(4, 0, 4, 0)
    };

    private static Border ToolbarGroup(params UIElement[] children) => ToolbarGroup(false, children);

    private static Border CenteredToolbarGroup(params UIElement[] children) => ToolbarGroup(true, children);

    private Border ShortcutGroup(params (string Action, string ResourceKey)[] shortcuts) =>
        ToolbarGroup(shortcuts.Select(ShortcutHint).ToArray());

    private UIElement ShortcutHint((string Action, string ResourceKey) shortcut)
    {
        var binding = _globalShortcuts.FirstOrDefault(item => item.Action == shortcut.Action)?.Chord;
        if (binding is null && EmulationShortcutDefaults.Values.TryGetValue(shortcut.Action, out var fallback))
            KeyboardChord.TryParse(fallback, out binding);
        var shortcutText = binding is null ? string.Empty : KeyboardChord.Format(binding.Modifiers, binding.Keys);
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0, 5, 0)
        };
        panel.Children.Add(new TextBlock
        {
            Text = LocExtension.Get(shortcut.ResourceKey),
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0)
        });
        var key = new Border
        {
            CornerRadius = new CornerRadius(3),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(5, 1, 5, 1),
            Child = new TextBlock { Text = shortcutText, FontSize = 11, FontWeight = FontWeights.SemiBold }
        };
        key.SetResourceReference(BackgroundProperty, "ControlBrush");
        key.SetResourceReference(BorderBrushProperty, "BorderBrush");
        panel.Children.Add(key);
        AutomationProperties.SetName(panel, $"{LocExtension.Get(shortcut.ResourceKey)} {shortcutText}");
        return panel;
    }

    private static Border ToolbarGroup(bool centered, params UIElement[] children)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = centered ? HorizontalAlignment.Center : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        foreach (var child in children) panel.Children.Add(child);
        var border = new Border
        {
            Child = panel,
            Height = 32,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(2, 1, 2, 1),
            Margin = new Thickness(2, 1, 2, 1),
            HorizontalAlignment = centered ? HorizontalAlignment.Center : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        border.SetResourceReference(BackgroundProperty, "CardBrush");
        border.SetResourceReference(BorderBrushProperty, "BorderBrush");
        return border;
    }

    private Button IconButton(string glyph, string tooltipKey, Func<Task> action, UIElement? indicator = null,
        bool requiresPower = true)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        if (indicator is not null) content.Children.Add(indicator);
        var icon = new TextBlock
        {
            Text = glyph, FontFamily = ControlVisualConstants.IconFont, FontSize = 17,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center
        };
        icon.SetBinding(TextBlock.ForegroundProperty, new Binding(nameof(Button.Foreground))
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Button), 1)
        });
        content.Children.Add(icon);
        var button = new Button
        {
            Content = content, ToolTip = LocExtension.Get(tooltipKey), Width = indicator is null ? 28 : 34,
            Height = 28, MinWidth = 0, MinHeight = 0, Padding = new Thickness(2), Margin = new Thickness(0, 0, 2, 0)
        };
        button.SetResourceReference(StyleProperty, "StatusIconButton");
        if (requiresPower) _machineCommandButtons.Add(button);
        button.Click += async (_, _) => await ButtonAsyncAction.RunAsync(
            button, action, ShowError, restoreEnabled: () => !_disposed);
        return button;
    }

    private void BuildDeviceStrip()
    {
        _deviceStrip.Children.Clear();
        _deviceLeds.Clear();
        var options = _configuration.Options ?? new Dictionary<string, string>();
        var floppyCount = int.TryParse(options.GetValueOrDefault("gwgui_floppy_drive_count"), out var floppies)
            ? Math.Clamp(floppies, 0, 4) : 1;
        var hardCount = int.TryParse(options.GetValueOrDefault("gwgui_hard_drive_count"), out var hard)
            ? Math.Clamp(hard, 0, 4) : _configuration.Media?.Count(item => item.Kind == AmigaMediaKind.HardDrive) ?? 0;
        for (var index = 0; index < floppyCount; index++) _deviceStrip.Children.Add(DeviceItem($"DF{index}:", "\uE7C3", index, true));
        for (var index = 0; index < hardCount; index++) _deviceStrip.Children.Add(DeviceItem($"DH{index}:", "\uEDA2", index, false));
        if (options.GetValueOrDefault("gwgui_cd_drive_enabled") == "enabled")
            _deviceStrip.Children.Add(DeviceItem("CD0:", "\uE958", 0, true));
    }

    private FrameworkElement DeviceItem(string name, string glyph, int index, bool removable)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var led = new Ellipse
        {
            Width = 10, Height = 10,
            Fill = _insertedMedia.Contains(name) || !removable ? Brushes.ForestGreen : Brushes.Gray,
            Margin = new Thickness(0, 0, 4, 0), Tag = name
        };
        _deviceLeds[name] = led;
        panel.Children.Add(led);
        var device = new Button
        {
            ToolTip = removable ? LocExtension.Get("Common.Browse") : name,
            Height = 20, MinHeight = 0, MinWidth = 0, Padding = new Thickness(2, 0, 2, 0), Margin = new Thickness(0),
            Content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children =
                {
                    new TextBlock { Text = glyph, FontFamily = ControlVisualConstants.IconFont, FontSize = 15, Margin = new Thickness(0, 0, 4, 0) },
                    new TextBlock { Text = name, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center }
                }
            }
        };
        device.Background = Brushes.Transparent;
        device.BorderBrush = Brushes.Transparent;
        if (removable) device.Click += async (_, _) =>
        {
            try { await InsertMedia(index, name.StartsWith("CD", StringComparison.Ordinal)); }
            catch (Exception error) { ShowError(error); }
        };
        panel.Children.Add(device);
        if (removable)
        {
            var eject = IconButton("\u23CF", "Common.Eject", async () =>
            {
                if (!_poweredOff && _machine.State is EmulationMachineState.Running or EmulationMachineState.Paused)
                {
                    if (index < _machine.DiskCount) await _machine.SelectDiskAsync(index);
                    await _machine.EjectMediaAsync();
                }
                _insertedMedia.Remove(name);
                _mountedMedia.Remove(name);
                BuildDeviceStrip();
            });
            _machineCommandButtons.Remove(eject);
            eject.Width = 22; eject.Height = 20; eject.MinWidth = 0; eject.MinHeight = 0;
            if (eject.Content is StackPanel ejectPanel && ejectPanel.Children.OfType<TextBlock>().LastOrDefault() is { } ejectIcon)
            {
                ejectIcon.FontFamily = new FontFamily("Segoe UI Symbol");
                ejectIcon.FontSize = 15;
            }
            eject.Margin = new Thickness(3, 0, 0, 0);
            eject.IsEnabled = _insertedMedia.Contains(name);
            panel.Children.Add(eject);
        }
        return new Border
        {
            Child = panel, Padding = new Thickness(4, 0, 4, 0), Margin = new Thickness(0, 0, 3, 0),
            BorderThickness = new Thickness(0, 0, 1, 0), BorderBrush = new SolidColorBrush(Color.FromRgb(215, 222, 231))
        };
    }

    private async Task InsertMedia(int index, bool compactDisc)
    {
        var dialog = new OpenFileDialog
        {
            Filter = LocExtension.Get("Emulation.AmigaMediaFilter")
        };
        if (dialog.ShowDialog() != true) return;
        var mediaPath = await AmigaRuntimeMedia.PrepareAsync(dialog.FileName);
        var deviceName = compactDisc ? $"CD{index}:" : $"DF{index}:";
        _mountedMedia[deviceName] = mediaPath;
        _insertedMedia.Add(deviceName);
        if (_poweredOff || _machine.State is not (EmulationMachineState.Running or EmulationMachineState.Paused))
        {
            BuildDeviceStrip();
            return;
        }
        if (index < _machine.DiskCount) await _machine.SelectDiskAsync(index);
        await _machine.InsertMediaAsync(mediaPath);
        BuildDeviceStrip();
    }

    private void UpdateDeviceLeds()
    {
        var leds = _machine.LedStates;
        var now = DateTime.UtcNow;
        foreach (var item in _deviceLeds)
        {
            var name = item.Key;
            var activityIndex = DeviceLedIndex(name);
            var active = leds.GetValueOrDefault(activityIndex);
            if (active) _deviceActivityUntil[name] = now.AddMilliseconds(140);
            var activityLatched = _deviceActivityUntil.GetValueOrDefault(name) > now;
            var present = _insertedMedia.Contains(name) || name.StartsWith("DH", StringComparison.OrdinalIgnoreCase);
            item.Value.Fill = activityLatched ? Brushes.LimeGreen : present ? Brushes.ForestGreen : Brushes.Gray;
        }
        _controllerStatus.Opacity = XInputControllerReader.ReadAll().Any(item => item != EmulationControllerState.Empty) ? 1 : 0.35;
    }

    private static int DeviceLedIndex(string name)
    {
        if (name.StartsWith("DF", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(name.AsSpan(2, 1), out var floppy)) return 3 + floppy;
        if (name.StartsWith("DH", StringComparison.OrdinalIgnoreCase)) return 7;
        return 8;
    }

    public event EventHandler? CloseRequested;

    private void FitScreen(double availableWidth, double availableHeight)
    {
        var fitted = EmulationVideoLayout.FitFourThree(availableWidth, availableHeight);
        if (fitted.IsEmpty) return;
        _screen.Width = fitted.Width;
        _screen.Height = fitted.Height;
    }

    public async Task StartAsync()
    {
        _status.Text = string.Empty;
        ResetFrameRateCounter();
        await _machine.StartAsync();
        _inputTimer.Start();
        _status.Text = string.Empty;
        SetPoweredState(true);
        _audioStatus.Opacity = _configuration.AudioEnabled ? 1 : 0.35;
        _controllerStatus.Opacity = XInputControllerReader.ReadAll().Any(item => item != EmulationControllerState.Empty) ? 1 : 0.35;
        _mouseStatus.Opacity = 0.35;
        _display.Focus();
    }

    public void ApplyVideoRenderer(EmulationVideoRenderer renderer)
    {
        if (_videoSurface.Renderer == renderer) return;
        ReleaseRelativeMouse();
        var replacement = CreateVideoSurface(renderer);
        var previous = _videoSurface;
        if (_display is HwndHost previousHost) previousHost.MessageHook -= NativeVideoMessage;
        _videoHost.Children.Clear();
        _videoSurface = replacement;
        _display = replacement.View;
        _videoHost.Children.Add(_display);
        AttachDisplayInputHandlers();
        previous.Dispose();
        _rendererStatus.Text = RendererName(_videoSurface.Renderer);
        if (_machine.LatestVideoFrame is { } frame) _videoSurface.Present(frame);
        _display.Focus();
    }

    public async Task StopAsync()
    {
        if (_disposed) return;
        if (_fullscreenWindow is not null) ExitFullscreen();
        try { await _machine.StopAsync(); }
        finally
        {
            _inputTimer.Stop();
            ReleaseRelativeMouse();
            await _machine.DisposeAsync();
            _machine.VideoFrameReady -= VideoFrameReady;
            _status.Text = string.Empty;
            _videoSurface.Dispose();
            _disposed = true;
        }
    }

    private async Task TogglePowerAsync()
    {
        ReleaseRelativeMouse();
        if (!_poweredOff)
        {
            _inputTimer.Stop();
            _machine.VideoFrameReady -= VideoFrameReady;
            await _machine.StopAsync();
            await _machine.DisposeAsync();
            _poweredOff = true;
            SetPoweredState(false);
            return;
        }
        _machine = _machineFactory();
        _machine.VideoFrameReady += VideoFrameReady;
        ResetFrameRateCounter();
        await _machine.StartAsync();
        _machine.SetAudioMuted(_audioMuted);
        await RestoreMountedMediaAsync();
        _inputTimer.Start();
        _poweredOff = false;
        SetPoweredState(true);
    }

    private void SetPoweredState(bool powered)
    {
        if (_powerButton is not null) _powerButton.Foreground = powered ? Brushes.LimeGreen : Brushes.Gray;
        if (powered && _pauseButton is not null)
        {
            SetIcon(_pauseButton, "\uE769", "Common.Pause");
            _pauseButton.Foreground = Brushes.DarkOrange;
        }
        foreach (var button in _machineCommandButtons) button.IsEnabled = powered;
        _videoHost.Visibility = powered ? Visibility.Visible : Visibility.Hidden;
        _audioStatus.Opacity = powered && _configuration.AudioEnabled ? 1 : 0.25;
        _controllerStatus.Opacity = powered ? _controllerStatus.Opacity : 0.25;
        _mouseStatus.Opacity = powered && _mouseCapture.IsCaptured ? 1 : 0.25;
        if (!powered) _status.Text = string.Empty;
    }

    private async Task RestoreMountedMediaAsync()
    {
        foreach (var item in _mountedMedia.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase))
        {
            var name = item.Key;
            var indexText = new string(name.SkipWhile(character => !char.IsDigit(character)).TakeWhile(char.IsDigit).ToArray());
            var index = int.TryParse(indexText, out var parsed) ? parsed : 0;
            if (index < _machine.DiskCount) await _machine.SelectDiskAsync(index);
            await _machine.InsertMediaAsync(item.Value);
        }
    }

    private Button AddButton(Panel panel, string key, Func<Task> action)
    {
        var button = new Button { Content = LocExtension.Get(key), MinWidth = 88, Margin = new Thickness(0, 0, 8, 0) };
        button.Click += async (_, _) => await ButtonAsyncAction.RunAsync(
            button, action, ShowError, restoreEnabled: () => !_disposed);
        panel.Children.Add(button);
        return button;
    }

    private void VideoFrameReady(object? sender, VideoFrame frame)
    {
        Interlocked.Increment(ref _framesProducedInWindow);
        if (Interlocked.Exchange(ref _framePending, 1) != 0) return;
        Dispatcher.BeginInvoke(() =>
        {
            try { Render(_machine.LatestVideoFrame ?? frame); }
            finally { Interlocked.Exchange(ref _framePending, 0); }
        });
    }

    private void Render(VideoFrame frame)
    {
        try
        {
            _videoSurface.Present(frame);
        }
        catch when (_videoSurface is not WpfVideoSurface)
        {
            _videoSurface.Dispose();
            _videoSurface = EmulationVideoSurfaceFactory.Create(EmulationVideoRenderer.Wpf);
            _display = _videoSurface.View;
            _videoHost.Children.Clear();
            _videoHost.Children.Add(_display);
            AttachDisplayInputHandlers();
            _videoSurface.Present(frame);
            _rendererStatus.Text = RendererName(_videoSurface.Renderer);
        }
        UpdateDeviceLeds();
        UpdateFrameRate();
        var hz = _machine.Configuration.Options?.GetValueOrDefault("puae_video_standard", "PAL")
            .StartsWith("NTSC", StringComparison.OrdinalIgnoreCase) == true ? 60d : 50d;
        _status.Text = $"{frame.Width} × {frame.Height} · {hz:0.0} Hz · {_measuredFramesPerSecond:0.0} FPS";
        _status.ToolTip = $"{LocExtension.Get("Emulation.RenderingSettings")} : {RendererName(_videoSurface.Renderer)}";
    }

    private void ResetFrameRateCounter()
    {
        Interlocked.Exchange(ref _framesProducedInWindow, 0);
        _frameRateWindowStarted = Stopwatch.GetTimestamp();
        _measuredFramesPerSecond = 0;
    }

    private void UpdateFrameRate()
    {
        var now = Stopwatch.GetTimestamp();
        var elapsed = Stopwatch.GetElapsedTime(_frameRateWindowStarted, now);
        if (elapsed < TimeSpan.FromSeconds(1)) return;
        var frames = Interlocked.Exchange(ref _framesProducedInWindow, 0);
        _measuredFramesPerSecond = frames / elapsed.TotalSeconds;
        _frameRateWindowStarted = now;
    }

    private static string RendererName(EmulationVideoRenderer renderer) => renderer switch
    {
        EmulationVideoRenderer.Direct3D11 => "Direct3D 11",
        EmulationVideoRenderer.Wpf => "WPF",
        _ => renderer.ToString()
    };

    private async Task InsertDisk()
    {
        var dialog = new OpenFileDialog
        {
            Filter = LocExtension.Get("Emulation.AmigaMediaFilter")
        };
        if (dialog.ShowDialog() == true) await _machine.InsertMediaAsync(dialog.FileName);
    }

    private async Task SaveState()
    {
        var dialog = new SaveFileDialog { Filter = $"{LocExtension.Get("Emulation.Shortcut.QuickSave")}|*.gwas", DefaultExt = ".gwas" };
        if (dialog.ShowDialog() == true) await _machine.SaveStateAsync(dialog.FileName);
    }

    private async Task QuickSave()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_quickStatePath)!);
        await _machine.SaveStateAsync(_quickStatePath);
    }

    private Task QuickLoad() => File.Exists(_quickStatePath)
        ? _machine.LoadStateAsync(_quickStatePath).AsTask() : Task.CompletedTask;

    private async Task TogglePauseAsync()
    {
        if (_machine.State == EmulationMachineState.Running)
        {
            ReleaseRelativeMouse();
            await _machine.PauseAsync();
            SetIcon(_pauseButton, "\uE768", "Common.Continue");
            if (_pauseButton is not null) _pauseButton.Foreground = Brushes.LimeGreen;
        }
        else if (_machine.State == EmulationMachineState.Paused)
        {
            await _machine.ResumeAsync();
            SetIcon(_pauseButton, "\uE769", "Common.Pause");
            if (_pauseButton is not null) _pauseButton.Foreground = Brushes.DarkOrange;
        }
    }

    private Task ToggleAudioMute()
    {
        _audioMuted = !_audioMuted;
        _machine.SetAudioMuted(_audioMuted);
        SetIcon(_audioStatus, _audioMuted ? "\uE74F" : "\uE767", "Emulation.Shortcut.Mute");
        _audioStatus.Opacity = _configuration.AudioEnabled ? 1 : 0.35;
        return Task.CompletedTask;
    }

    private static void SetIcon(Button? button, string glyph, string tooltipKey)
    {
        if (button?.Content is StackPanel panel && panel.Children.OfType<TextBlock>().LastOrDefault() is { } icon)
            icon.Text = glyph;
        if (button is not null) button.ToolTip = LocExtension.Get(tooltipKey);
    }

    private async Task LoadState()
    {
        var dialog = new OpenFileDialog { Filter = $"{LocExtension.Get("Emulation.Shortcut.QuickLoad")}|*.gwas" };
        if (dialog.ShowDialog() == true) await _machine.LoadStateAsync(dialog.FileName);
    }

    private void ShowError(Exception error)
        => ControlErrorPresenter.ShowUnexpected(this, error,
            ControlErrorContexts.AmigaEmulatorCommand, ControlVisualConstants.AmigaTitle);

    private void DisplayKeyDown(object sender, KeyEventArgs e)
    {
        var source = e.Key == Key.System ? e.SystemKey : e.Key;
        e.Handled = HandleKeyDown(source);
    }

    private bool HandleKeyDown(Key source)
    {
        if (InputBindingSyntax.IsReservedShortcut(source, Keyboard.Modifiers)) return false;
        if (!KeyboardChord.IsModifierKey(source)) _pressedPhysicalKeys.Add(source);
        var global = _globalShortcuts.FirstOrDefault(binding =>
            binding.Chord.Matches(Keyboard.Modifiers, _pressedPhysicalKeys));
        if (global is not null)
        {
            if (_activeGlobalShortcuts.Add(global.Action)) _ = ExecuteGlobalShortcutAsync(global.Action);
            return true;
        }
        if (_globalShortcuts.Any(binding => binding.Chord.Modifiers == Keyboard.Modifiers && binding.Chord.Contains(source)))
        {
            return true;
        }
        var shortcut = _keyboardShortcuts.FirstOrDefault(binding =>
            binding.Chord.Matches(Keyboard.Modifiers, _pressedPhysicalKeys));
        if (shortcut is not null)
        {
            _pressedShortcutKeys[source] = shortcut.AmigaKey;
            _keys.Add(shortcut.AmigaKey);
            PublishInput();
            return true;
        }
        else if (AmigaKeyMapper.TryMap(source, out var key))
        {
            _hostKeys.Add(key);
            _keys.Add(_keyboardMap.GetValueOrDefault(key, key));
            PublishInput();
            return true;
        }
        return false;
    }

    private void DisplayKeyUp(object sender, KeyEventArgs e)
    {
        var source = e.Key == Key.System ? e.SystemKey : e.Key;
        e.Handled = HandleKeyUp(source);
    }

    private bool HandleKeyUp(Key source)
    {
        _pressedPhysicalKeys.Remove(source);
        _activeGlobalShortcuts.RemoveWhere(action =>
        {
            var binding = _globalShortcuts.FirstOrDefault(item => item.Action == action);
            return binding is null || !binding.Chord.Matches(Keyboard.Modifiers, _pressedPhysicalKeys);
        });
        if (_pressedShortcutKeys.Remove(source, out var shortcutKey))
        {
            _keys.Remove(shortcutKey);
            PublishInput();
            return true;
        }
        else if (AmigaKeyMapper.TryMap(source, out var key))
        {
            _hostKeys.Remove(key);
            _keys.Remove(_keyboardMap.GetValueOrDefault(key, key));
            PublishInput();
            return true;
        }
        return false;
    }

    private void DisplayLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        ReleaseRelativeMouse();
        _keys.Clear();
        _hostKeys.Clear();
        _pressedShortcutKeys.Clear();
        _pressedPhysicalKeys.Clear();
        _activeGlobalShortcuts.Clear();
        PublishInput();
    }

    private async Task ExecuteGlobalShortcutAsync(string action)
    {
        try
        {
            switch (action)
            {
                case EmulationShortcutDefaults.ReleaseMouse:
                    ReleaseRelativeMouse();
                    break;
                case EmulationShortcutDefaults.PauseResume:
                    await TogglePauseAsync();
                    break;
                case EmulationShortcutDefaults.ToggleFullscreen:
                    ToggleFullscreen();
                    break;
                case EmulationShortcutDefaults.Power:
                    CloseRequested?.Invoke(this, EventArgs.Empty);
                    break;
                case EmulationShortcutDefaults.HardReset:
                    await _machine.HardResetAsync();
                    break;
                case EmulationShortcutDefaults.QuickSave:
                    Directory.CreateDirectory(Path.GetDirectoryName(_quickStatePath)!);
                    await _machine.SaveStateAsync(_quickStatePath);
                    break;
                case EmulationShortcutDefaults.QuickLoad when File.Exists(_quickStatePath):
                    await _machine.LoadStateAsync(_quickStatePath);
                    break;
                case EmulationShortcutDefaults.Screenshot:
                    SaveScreenshot();
                    break;
                default:
                    _status.Text = LocExtension.Get("Emulation.ShortcutUnavailable");
                    break;
            }
        }
        catch (Exception error) { ShowError(error); }
    }

    private void ToggleFullscreen()
    {
        ReleaseRelativeMouse();
        if (_fullscreenWindow is not null)
        {
            ExitFullscreen();
            return;
        }

        _displayHost.Children.Remove(_screen);
        _fullscreenHost = new Grid { Background = Brushes.Black };
        _fullscreenHost.Children.Add(_screen);
        _fullscreenHost.SizeChanged += FullscreenHostSizeChanged;
        var owner = Window.GetWindow(this);
        _fullscreenWindow = new Window
        {
            Content = _fullscreenHost,
            Background = Brushes.Black,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = owner,
            Title = "Amiga"
        };
        _fullscreenWindow.Closing += FullscreenWindowClosing;
        _fullscreenWindow.PreviewKeyDown += DisplayKeyDown;
        _fullscreenWindow.PreviewKeyUp += DisplayKeyUp;
        _fullscreenWindow.Show();
        _fullscreenWindow.WindowState = WindowState.Maximized;
        _fullscreenWindow.Activate();
        _display.Focus();
    }

    private void FullscreenHostSizeChanged(object sender, SizeChangedEventArgs e) =>
        FitScreen(e.NewSize.Width, e.NewSize.Height);

    private void FullscreenWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_closingFullscreen) return;
        e.Cancel = true;
        ExitFullscreen();
    }

    private void ExitFullscreen()
    {
        if (_fullscreenWindow is null) return;
        var window = _fullscreenWindow;
        if (_fullscreenHost is not null)
        {
            _fullscreenHost.SizeChanged -= FullscreenHostSizeChanged;
            _fullscreenHost.Children.Remove(_screen);
        }
        _displayHost.Children.Add(_screen);
        _fullscreenHost = null;
        _fullscreenWindow = null;
        _closingFullscreen = true;
        try
        {
            window.Closing -= FullscreenWindowClosing;
            window.PreviewKeyDown -= DisplayKeyDown;
            window.PreviewKeyUp -= DisplayKeyUp;
            window.Close();
        }
        finally { _closingFullscreen = false; }
        FitScreen(_displayHost.ActualWidth, _displayHost.ActualHeight);
        _display.Focus();
    }

    private void SaveScreenshot()
    {
        var snapshot = _videoSurface.Snapshot;
        if (snapshot is null) return;
        Directory.CreateDirectory(_captureFolder);
        var path = Path.Combine(_captureFolder, $"Amiga-{DateTime.Now:yyyyMMdd-HHmmss}.png");
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(snapshot));
        using var stream = File.Create(path);
        encoder.Save(stream);
        _status.Text = Path.GetFileName(path);
    }

    private void DisplayMouseMove(object sender, MouseEventArgs e)
    {
        if (!_mouseCapture.IsCaptured) return;
        ProcessRelativePointer();
    }

    private void ProcessRelativePointer()
    {
        _mouseCapture.ProcessMovement(_screen, (deltaX, deltaY) => PublishInput(deltaX, deltaY));
    }

    private IntPtr NativeVideoMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        switch (message)
        {
            case WindowsInputMessages.KeyDown:
            case WindowsInputMessages.SystemKeyDown:
                handled = HandleKeyDown(KeyInterop.KeyFromVirtualKey(unchecked((int)wParam.ToInt64())));
                break;
            case WindowsInputMessages.KeyUp:
            case WindowsInputMessages.SystemKeyUp:
                handled = HandleKeyUp(KeyInterop.KeyFromVirtualKey(unchecked((int)wParam.ToInt64())));
                break;
            case WindowsInputMessages.LeftButtonDown:
            case WindowsInputMessages.RightButtonDown:
            case WindowsInputMessages.MiddleButtonDown:
            case WindowsInputMessages.XButtonDown:
                RelativeMouseCapture.FocusNative(hwnd);
                if (_input.CaptureMouse && !_mouseCapture.IsCaptured) CaptureRelativeMouse();
                if (_mouseCapture.IsCaptured) PublishInput();
                break;
            case WindowsInputMessages.LeftButtonUp:
            case WindowsInputMessages.RightButtonUp:
            case WindowsInputMessages.MiddleButtonUp:
            case WindowsInputMessages.XButtonUp:
                if (_mouseCapture.IsCaptured) PublishInput();
                break;
            case WindowsInputMessages.MouseMove when _mouseCapture.IsCaptured:
                ProcessRelativePointer();
                break;
            case WindowsInputMessages.MouseWheel when _mouseCapture.IsCaptured:
                PublishInput(wheel: unchecked((short)((wParam.ToInt64() >> WindowsInputMessages.WheelHighWordShift)
                    & WindowsInputMessages.UnsignedWordMask)));
                break;
            case WindowsInputMessages.MouseHorizontalWheel when _mouseCapture.IsCaptured:
                PublishInput(horizontalWheel: unchecked((short)((wParam.ToInt64() >> WindowsInputMessages.WheelHighWordShift)
                    & WindowsInputMessages.UnsignedWordMask)));
                break;
            case WindowsInputMessages.SetCursor when _mouseCapture.IsCaptured:
                RelativeMouseCapture.HideNativeCursor();
                handled = true;
                break;
        }
        return IntPtr.Zero;
    }

    private void MouseChanged(object sender, MouseButtonEventArgs e)
    {
        if (_mouseCapture.IsCaptured) PublishInput();
    }

    private void DisplayMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_mouseCapture.IsCaptured) PublishInput(wheel: e.Delta);
    }

    private void AttachWindowHook()
    {
        if (_windowSource is not null || Window.GetWindow(this) is not Window window) return;
        _windowSource = PresentationSource.FromVisual(window) as HwndSource;
        _windowSource?.AddHook(WindowMessageHook);
    }

    private void DetachWindowHook()
    {
        _windowSource?.RemoveHook(WindowMessageHook);
        _windowSource = null;
    }

    private IntPtr WindowMessageHook(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message != WindowsInputMessages.MouseHorizontalWheel || !_mouseCapture.IsCaptured || !_display.IsMouseOver) return IntPtr.Zero;
        var delta = unchecked((short)((wParam.ToInt64() >> WindowsInputMessages.WheelHighWordShift)
            & WindowsInputMessages.UnsignedWordMask));
        if (delta != WindowsInputMessages.NeutralWheelDelta) PublishInput(horizontalWheel: delta);
        return IntPtr.Zero;
    }

    private Task ToggleMouseCapture()
    {
        if (_mouseCapture.IsCaptured) ReleaseRelativeMouse();
        else CaptureRelativeMouse();
        return Task.CompletedTask;
    }

    private void CaptureRelativeMouse()
    {
        _mouseCapture.Capture(_display, _screen, _videoSurface.InputHandle);
        _mouseStatus.Opacity = 1;
    }

    private void ReleaseRelativeMouse()
    {
        if (!_mouseCapture.IsCaptured) return;
        _mouseCapture.Release(_display, _videoSurface.InputHandle);
        _mouseStatus.Opacity = 0.35;
        _keys.Remove(EmulationKey.LeftControl);
        _keys.Remove(EmulationKey.RightControl);
        _keys.Remove(EmulationKey.LeftAlt);
        _keys.Remove(EmulationKey.RightAlt);
        if (!_disposed) PublishInput();
    }

    private void PublishInput(int deltaX = RelativeMouseCaptureConstants.NoMovement,
        int deltaY = RelativeMouseCaptureConstants.NoMovement,
        int wheel = WindowsInputMessages.NeutralWheelDelta,
        int horizontalWheel = WindowsInputMessages.NeutralWheelDelta)
    {
        var mouseActive = _mouseCapture.IsCaptured;
        var physical = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            ["Left"] = mouseActive && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.LeftMouseVirtualKey),
            ["Right"] = mouseActive && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.RightMouseVirtualKey),
            ["Middle"] = mouseActive && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.MiddleMouseVirtualKey),
            ["XButton1"] = mouseActive && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.FirstExtendedMouseVirtualKey),
            ["XButton2"] = mouseActive && RelativeMouseCapture.IsButtonPressed(WindowsInputMessages.SecondExtendedMouseVirtualKey),
            ["WheelUp"] = mouseActive && wheel > WindowsInputMessages.NeutralWheelDelta,
            ["WheelDown"] = mouseActive && wheel < WindowsInputMessages.NeutralWheelDelta,
            ["WheelLeft"] = mouseActive && horizontalWheel < WindowsInputMessages.NeutralWheelDelta,
            ["WheelRight"] = mouseActive && horizontalWheel > WindowsInputMessages.NeutralWheelDelta
        };
        var actions = _input.MouseButtonMappings ?? new Dictionary<string, AmigaMouseAction>
        {
            ["Mouse:Left"] = AmigaMouseAction.LeftButton,
            ["Mouse:Right"] = AmigaMouseAction.RightButton
        };
        var controllers = XInputControllerReader.ReadAll();
        var primaryController = controllers.FirstOrDefault() ?? EmulationControllerState.Empty;
        bool IsPressed(AmigaMouseAction action) => actions.Any(mapping => mapping.Value == action
            && IsControllerSourcePressed(mapping.Key, ControllerInputMap.ControllerForSource(mapping.Key, controllers, primaryController), physical));
        _machine.SetInput(new EmulationInputSnapshot(new HashSet<EmulationKey>(_keys),
            new EmulationPointerState(mouseActive ? deltaX : RelativeMouseCaptureConstants.NoMovement,
                mouseActive ? deltaY : RelativeMouseCaptureConstants.NoMovement,
                mouseActive ? wheel : WindowsInputMessages.NeutralWheelDelta,
                IsPressed(AmigaMouseAction.LeftButton),
                IsPressed(AmigaMouseAction.RightButton), IsPressed(AmigaMouseAction.MiddleButton)),
            MapControllers(controllers, physical)));
    }

    private IReadOnlyList<EmulationControllerState> MapControllers(IReadOnlyList<EmulationControllerState> physical,
        IReadOnlyDictionary<string, bool> mouseButtons)
    {
        var result = new EmulationControllerState[4];
        for (var port = 0; port < result.Length; port++)
        {
            var binding = _input.ControllerBindings?.FirstOrDefault(item => item.Port == port);
            var sourcePort = ControllerInputMap.ParseXInputPort(binding?.DeviceId, port);
            var source = sourcePort < physical.Count ? physical[sourcePort] : EmulationControllerState.Empty;
            if (binding?.ButtonMappings is not { Count: > 0 })
            {
                result[port] = source;
                continue;
            }
            uint buttons = 0;
            foreach (var mapping in binding.ButtonMappings)
            {
                var target = Array.IndexOf(ControllerInputMap.LegacyButtonNames, mapping.Value);
                if (target >= 0 && IsControllerSourcePressed(mapping.Key, source, mouseButtons)) buttons |= 1u << target;
            }
            result[port] = source with { Buttons = buttons };
        }
        return result;
    }

    private bool IsControllerSourcePressed(string sourceName, EmulationControllerState controller,
        IReadOnlyDictionary<string, bool> mouseButtons)
    {
        if (InputBindingSyntax.TryRemovePrefix(sourceName, InputBindingSyntax.ControllerPrefix, out var controllerSource))
            return ControllerInputMap.IsModernSourcePressed(controllerSource, controller);
        var controllerIndex = Array.IndexOf(ControllerInputMap.LegacyButtonNames, sourceName);
        if (controllerIndex >= 0) return (controller.Buttons & (1u << controllerIndex)) != 0;
        if (InputBindingSyntax.TryRemovePrefix(sourceName, InputBindingSyntax.KeyboardPrefix, out var keyboardSource)
            && Enum.TryParse<EmulationKey>(keyboardSource, true, out var key)) return _hostKeys.Contains(key);
        if (InputBindingSyntax.TryRemovePrefix(sourceName, InputBindingSyntax.MousePrefix, out var mouseSource))
            return mouseButtons.GetValueOrDefault(mouseSource);
        return false;
    }

}
