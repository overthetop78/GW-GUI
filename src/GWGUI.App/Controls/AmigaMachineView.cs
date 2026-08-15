using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
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
using GWGUI.MediaEngine.Composition;
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
    private bool _mouseCaptured;
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
    private readonly DispatcherTimer _inputTimer = new() { Interval = TimeSpan.FromMilliseconds(16) };
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
        _keyboardMap = BuildKeyboardMap(_input.KeyboardMappings);
        _keyboardShortcuts = BuildKeyboardShortcuts(_input.KeyboardBindings);
        _globalShortcuts = BuildGlobalShortcuts(globalShortcuts);
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
        DockPanel.SetDock(left, Dock.Left);
        _toolbar.Children.Add(left);
        var right = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        _audioStatus = IconButton("\uE767", "Emulation.AudioTab", ToggleAudioMute, requiresPower: true);
        _audioStatus.Width = 28;
        right.Children.Add(ToolbarGroup(_audioStatus, _controllerStatus, _mouseStatus));
        _status.Margin = new Thickness(7, 0, 7, 0);
        right.Children.Add(ToolbarGroup(_status));
        DockPanel.SetDock(right, Dock.Right); _toolbar.Children.Add(right);
        _rendererStatus.Text = RendererName(_videoSurface.Renderer);
        _toolbar.Children.Add(CenteredToolbarGroup(new TextBlock
        {
            Text = "\uE7F4", FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 15,
            Margin = new Thickness(4, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center
        }, _rendererStatus));
        _root.Children.Add(_toolbar);
        _screen = new Border
        {
            Background = Brushes.Black,
            Child = _videoHost,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            SnapsToDevicePixels = true
        };
        _screen.AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(ScreenMouseDown), true);
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
        _display.LostKeyboardFocus += DisplayLostKeyboardFocus;
        _display.MouseMove += DisplayMouseMove;
        _display.MouseDown += MouseChanged;
        _display.MouseUp += MouseChanged;
        _display.MouseWheel += DisplayMouseWheel;
        _display.MouseDown += ScreenMouseDown;
        if (_display is HwndHost host) host.MessageHook += NativeVideoMessage;
    }

    private void ScreenMouseDown(object sender, MouseButtonEventArgs e)
    {
        FocusVideoSurface();
        if (!_mouseCaptured) CaptureRelativeMouse();
    }

    private IEmulationVideoSurface CreateVideoSurface(EmulationVideoRenderer renderer)
    {
        try { return EmulationVideoSurfaceFactory.Create(renderer); }
        catch { return EmulationVideoSurfaceFactory.Create(EmulationVideoRenderer.Wpf); }
    }

    private static TextBlock StatusIcon(string glyph) => new()
    {
        Text = glyph,
        FontFamily = new FontFamily("Segoe MDL2 Assets"),
        FontSize = 17,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(4, 0, 4, 0)
    };

    private static Border ToolbarGroup(params UIElement[] children) => ToolbarGroup(false, children);

    private static Border CenteredToolbarGroup(params UIElement[] children) => ToolbarGroup(true, children);

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
            Text = glyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 17,
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
        button.Click += async (_, _) =>
        {
            try { button.IsEnabled = false; await action(); }
            catch (Exception error) { ShowError(error); }
            finally { if (!_disposed) button.IsEnabled = true; }
        };
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
                    new TextBlock { Text = glyph, FontFamily = new FontFamily("Segoe MDL2 Assets"), FontSize = 15, Margin = new Thickness(0, 0, 4, 0) },
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
            Filter = compactDisc
                ? "CD|*.cue;*.ccd;*.chd;*.nrg;*.mds;*.iso|All files|*.*"
                : "Amiga floppy|*.scp;*.adf;*.adz;*.dms;*.fdi;*.ipf;*.raw|All files|*.*"
        };
        if (dialog.ShowDialog() != true) return;
        var mediaPath = await PrepareMediaAsync(dialog.FileName);
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

    private static async Task<string> PrepareMediaAsync(string path)
    {
        if (!Path.GetExtension(path).Equals(".scp", StringComparison.OrdinalIgnoreCase)) return path;
        var info = new FileInfo(path);
        var identity = $"{Path.GetFullPath(path)}|{info.Length}|{info.LastWriteTimeUtc.Ticks}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity)))[..16];
        var folder = Path.Combine(Path.GetTempPath(), "GW GUI", "Emulation", "Amiga", "Converted");
        Directory.CreateDirectory(folder);
        var output = Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(path)}-{hash}.adf");
        if (File.Exists(output)) return output;
        var converter = MediaEngineFactory.CreateAmigaAdfConversionService();
        try { await converter.ConvertAsync(path, output, DiskImageFormatIds.AmigaDos); }
        catch (InvalidDataException)
        {
            if (File.Exists(output)) File.Delete(output);
            await converter.ConvertAsync(path, output, DiskImageFormatIds.AmigaDosHighDensity);
        }
        return output;
    }

    internal static async Task<AmigaMachineConfiguration> PrepareRuntimeConfigurationAsync(
        AmigaMachineConfiguration configuration)
    {
        var initial = string.IsNullOrWhiteSpace(configuration.InitialDiskPath)
            ? configuration.InitialDiskPath : await PrepareMediaAsync(configuration.InitialDiskPath);
        if (configuration.Media is not { Count: > 0 }) return configuration with { InitialDiskPath = initial };
        var media = new List<AmigaMediaConfiguration>(configuration.Media.Count);
        foreach (var item in configuration.Media)
            media.Add(item with { Path = await PrepareMediaAsync(item.Path) });
        return configuration with { InitialDiskPath = initial, Media = media };
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
        var fitted = FitFourThree(availableWidth, availableHeight);
        if (fitted.IsEmpty) return;
        _screen.Width = fitted.Width;
        _screen.Height = fitted.Height;
    }

    internal static Size FitFourThree(double availableWidth, double availableHeight)
    {
        if (availableWidth <= 0 || availableHeight <= 0) return Size.Empty;
        var width = Math.Min(availableWidth, availableHeight * 4d / 3d);
        return new Size(width, width * 3d / 4d);
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
        _mouseStatus.Opacity = powered && _mouseCaptured ? 1 : 0.25;
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
        button.Click += async (_, _) =>
        {
            try
            {
                button.IsEnabled = false;
                await action();
            }
            catch (Exception error)
            {
                ShowError(error);
            }
            finally
            {
                if (!_disposed) button.IsEnabled = true;
            }
        };
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

    private static byte[] ConvertToBgra32(VideoFrame frame, int destinationPitch)
    {
        var source = frame.Pixels.Span;
        var destination = GC.AllocateUninitializedArray<byte>(checked(destinationPitch * frame.Height));
        for (var y = 0; y < frame.Height; y++)
        {
            var sourceRow = source.Slice(checked(y * frame.Pitch), frame.Pitch);
            var destinationRow = destination.AsSpan(checked(y * destinationPitch), destinationPitch);
            if (frame.PixelFormat == EmulationPixelFormat.Xrgb8888)
            {
                sourceRow[..Math.Min(sourceRow.Length, destinationPitch)].CopyTo(destinationRow);
                for (var x = 0; x < frame.Width; x++) destinationRow[x * 4 + 3] = 255;
                continue;
            }

            for (var x = 0; x < frame.Width; x++)
            {
                var sourceOffset = x * 2;
                var value = sourceRow[sourceOffset] | sourceRow[sourceOffset + 1] << 8;
                var destinationOffset = x * 4;
                destinationRow[destinationOffset] = (byte)((value & 0x1f) * 255 / 31);
                destinationRow[destinationOffset + 1] = (byte)(((value >> 5) & 0x3f) * 255 / 63);
                destinationRow[destinationOffset + 2] = (byte)(((value >> 11) & 0x1f) * 255 / 31);
                destinationRow[destinationOffset + 3] = 255;
            }
        }
        return destination;
    }

    private async Task InsertDisk()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Amiga media|*.adf;*.adz;*.dms;*.fdi;*.ipf;*.raw;*.hdf;*.hdz;*.lha;*.slave;*.info;*.cue;*.ccd;*.chd;*.nrg;*.mds;*.iso;*.uae;*.m3u;*.zip;*.7z|All files|*.*"
        };
        if (dialog.ShowDialog() == true) await _machine.InsertMediaAsync(dialog.FileName);
    }

    private async Task SaveState()
    {
        var dialog = new SaveFileDialog { Filter = "GW GUI Amiga state|*.gwas", DefaultExt = ".gwas" };
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
        var dialog = new OpenFileDialog { Filter = "GW GUI Amiga state|*.gwas" };
        if (dialog.ShowDialog() == true) await _machine.LoadStateAsync(dialog.FileName);
    }

    private void ShowError(Exception error)
    {
        var logPath = ErrorLog.Write(error, "Amiga emulator command");
        var detail = logPath is null ? LocExtension.Get("Common.Unknown") : LocExtension.Get("Error.LogSaved", logPath);
        MessageBox.Show(Window.GetWindow(this), LocExtension.Get("Error.Unexpected", detail), "Amiga",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void DisplayKeyDown(object sender, KeyEventArgs e)
    {
        var source = e.Key == Key.System ? e.SystemKey : e.Key;
        e.Handled = HandleKeyDown(source);
    }

    private bool HandleKeyDown(Key source)
    {
        if (IsReservedShortcut(source, Keyboard.Modifiers)) return false;
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
        else if (TryMapKey(source, out var key))
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
        else if (TryMapKey(source, out var key))
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
        if (!_mouseCaptured) return;
        ProcessRelativePointer();
    }

    private void ProcessRelativePointer()
    {
        if (!_mouseCaptured || !GetCursorPos(out var current)) return;
        var center = _screen.PointToScreen(new Point(_screen.ActualWidth / 2, _screen.ActualHeight / 2));
        var deltaX = current.X - (int)Math.Round(center.X);
        var deltaY = current.Y - (int)Math.Round(center.Y);
        if (deltaX == 0 && deltaY == 0) return;
        PublishInput(deltaX, deltaY);
        SetCursorPos((int)Math.Round(center.X), (int)Math.Round(center.Y));
    }

    private IntPtr NativeVideoMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int keyDown = 0x0100, keyUp = 0x0101, sysKeyDown = 0x0104, sysKeyUp = 0x0105;
        const int mouseMove = 0x0200, leftDown = 0x0201, rightDown = 0x0204, middleDown = 0x0207;
        const int mouseWheel = 0x020A, mouseHorizontalWheel = 0x020E, setCursor = 0x0020;
        switch (message)
        {
            case keyDown:
            case sysKeyDown:
                handled = HandleKeyDown(KeyInterop.KeyFromVirtualKey(unchecked((int)wParam.ToInt64())));
                break;
            case keyUp:
            case sysKeyUp:
                handled = HandleKeyUp(KeyInterop.KeyFromVirtualKey(unchecked((int)wParam.ToInt64())));
                break;
            case leftDown:
            case rightDown:
            case middleDown:
                FocusVideoSurface();
                if (!_mouseCaptured) CaptureRelativeMouse();
                if (_mouseCaptured) PublishInput();
                break;
            case mouseMove when _mouseCaptured:
                ProcessRelativePointer();
                break;
            case mouseWheel when _mouseCaptured:
                PublishInput(wheel: unchecked((short)((wParam.ToInt64() >> 16) & 0xffff)));
                break;
            case mouseHorizontalWheel when _mouseCaptured:
                PublishInput(horizontalWheel: unchecked((short)((wParam.ToInt64() >> 16) & 0xffff)));
                break;
            case setCursor when _mouseCaptured:
                SetCursor(IntPtr.Zero);
                handled = true;
                break;
        }
        return IntPtr.Zero;
    }

    private void MouseChanged(object sender, MouseButtonEventArgs e)
    {
        if (_mouseCaptured) PublishInput();
    }

    private void DisplayMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_mouseCaptured) PublishInput(wheel: e.Delta);
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
        const int activateApp = 0x001C, mouseHorizontalWheel = 0x020E;
        if (message == activateApp && wParam == IntPtr.Zero)
        {
            ReleaseRelativeMouse();
            return IntPtr.Zero;
        }
        if (message != mouseHorizontalWheel || !_mouseCaptured || !_display.IsMouseOver) return IntPtr.Zero;
        var delta = unchecked((short)((wParam.ToInt64() >> 16) & 0xffff));
        if (delta != 0) PublishInput(horizontalWheel: delta);
        return IntPtr.Zero;
    }

    private Task ToggleMouseCapture()
    {
        if (_mouseCaptured) ReleaseRelativeMouse();
        else CaptureRelativeMouse();
        return Task.CompletedTask;
    }

    private void CaptureRelativeMouse()
    {
        if (_mouseCaptured || _machine.State is not (EmulationMachineState.Running or EmulationMachineState.Paused)) return;
        _mouseCaptured = true;
        _display.Cursor = Cursors.None;
        Mouse.Capture(_display);
        if (_videoSurface.InputHandle != IntPtr.Zero) SetCapture(_videoSurface.InputHandle);
        FocusVideoSurface();
        _mouseStatus.Opacity = 1;
        var center = new Point(_screen.ActualWidth / 2, _screen.ActualHeight / 2);
        var screen = _screen.PointToScreen(center);
        SetCursorPos((int)Math.Round(screen.X), (int)Math.Round(screen.Y));
    }

    private void FocusVideoSurface()
    {
        _display.Focus();
        if (_videoSurface.InputHandle != IntPtr.Zero) SetFocus(_videoSurface.InputHandle);
        else Keyboard.Focus(_display);
    }

    private void ReleaseRelativeMouse()
    {
        if (!_mouseCaptured) return;
        _mouseCaptured = false;
        Mouse.Capture(null);
        if (_videoSurface.InputHandle != IntPtr.Zero) ReleaseCapture();
        _display.Cursor = null;
        _mouseStatus.Opacity = 0.35;
        _keys.Remove(EmulationKey.LeftControl);
        _keys.Remove(EmulationKey.RightControl);
        _keys.Remove(EmulationKey.LeftAlt);
        _keys.Remove(EmulationKey.RightAlt);
        if (!_disposed) PublishInput();
    }

    private void PublishInput(int deltaX = 0, int deltaY = 0, int wheel = 0, int horizontalWheel = 0)
    {
        var mouseActive = _mouseCaptured;
        var physical = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            ["Left"] = mouseActive && Mouse.LeftButton == MouseButtonState.Pressed,
            ["Right"] = mouseActive && Mouse.RightButton == MouseButtonState.Pressed,
            ["Middle"] = mouseActive && Mouse.MiddleButton == MouseButtonState.Pressed,
            ["XButton1"] = mouseActive && Mouse.XButton1 == MouseButtonState.Pressed,
            ["XButton2"] = mouseActive && Mouse.XButton2 == MouseButtonState.Pressed,
            ["WheelUp"] = mouseActive && wheel > 0,
            ["WheelDown"] = mouseActive && wheel < 0,
            ["WheelLeft"] = mouseActive && horizontalWheel < 0,
            ["WheelRight"] = mouseActive && horizontalWheel > 0
        };
        var actions = _input.MouseButtonMappings ?? new Dictionary<string, AmigaMouseAction>
        {
            ["Mouse:Left"] = AmigaMouseAction.LeftButton,
            ["Mouse:Right"] = AmigaMouseAction.RightButton
        };
        var controllers = XInputControllerReader.ReadAll();
        var primaryController = controllers.FirstOrDefault() ?? EmulationControllerState.Empty;
        bool IsPressed(AmigaMouseAction action) => actions.Any(mapping => mapping.Value == action
            && IsControllerSourcePressed(mapping.Key, ControllerForSource(mapping.Key, controllers, primaryController), physical));
        _machine.SetInput(new EmulationInputSnapshot(new HashSet<EmulationKey>(_keys),
            new EmulationPointerState(mouseActive ? deltaX : 0, mouseActive ? deltaY : 0, mouseActive ? wheel : 0,
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
            var sourcePort = ParseXInputPort(binding?.DeviceId, port);
            var source = sourcePort < physical.Count ? physical[sourcePort] : EmulationControllerState.Empty;
            if (binding?.ButtonMappings is not { Count: > 0 })
            {
                result[port] = source;
                continue;
            }
            uint buttons = 0;
            foreach (var mapping in binding.ButtonMappings)
            {
                var target = Array.IndexOf(ControllerButtonNames, mapping.Value);
                if (target >= 0 && IsControllerSourcePressed(mapping.Key, source, mouseButtons)) buttons |= 1u << target;
            }
            result[port] = source with { Buttons = buttons };
        }
        return result;
    }

    private bool IsControllerSourcePressed(string sourceName, EmulationControllerState controller,
        IReadOnlyDictionary<string, bool> mouseButtons)
    {
        if (sourceName.StartsWith("Controller:", StringComparison.OrdinalIgnoreCase))
            return IsModernControllerSourcePressed(sourceName["Controller:".Length..], controller);
        var controllerIndex = Array.IndexOf(ControllerButtonNames, sourceName);
        if (controllerIndex >= 0) return (controller.Buttons & (1u << controllerIndex)) != 0;
        if (sourceName.StartsWith("Keyboard:", StringComparison.OrdinalIgnoreCase)
            && Enum.TryParse<EmulationKey>(sourceName[9..], true, out var key)) return _hostKeys.Contains(key);
        if (sourceName.StartsWith("Mouse:", StringComparison.OrdinalIgnoreCase))
            return mouseButtons.GetValueOrDefault(sourceName[6..]);
        return false;
    }

    private static bool IsModernControllerSourcePressed(string source, EmulationControllerState controller)
    {
        const short threshold = 14000;
        var segments = source.Split(':', StringSplitOptions.RemoveEmptyEntries);
        source = segments[^1];
        var button = source switch
        {
            "ButtonB" => 0, "ButtonY" => 1, "View" => 2, "Menu" => 3,
            "DPadUp" => 4, "DPadDown" => 5, "DPadLeft" => 6, "DPadRight" => 7,
            "ButtonA" => 8, "ButtonX" => 9, "LeftShoulder" => 10, "RightShoulder" => 11,
            "LeftTrigger" => 12, "RightTrigger" => 13,
            "LeftStickClick" => 14, "RightStickClick" => 15,
            "XboxButton" => 16,
            _ => -1
        };
        if (button >= 0) return (controller.Buttons & (1u << button)) != 0;
        return source switch
        {
            "LeftStickLeft" => controller.LeftX < -threshold,
            "LeftStickRight" => controller.LeftX > threshold,
            "LeftStickUp" => controller.LeftY < -threshold,
            "LeftStickDown" => controller.LeftY > threshold,
            "RightStickLeft" => controller.RightX < -threshold,
            "RightStickRight" => controller.RightX > threshold,
            "RightStickUp" => controller.RightY < -threshold,
            "RightStickDown" => controller.RightY > threshold,
            _ => false
        };
    }

    private static EmulationControllerState ControllerForSource(string source,
        IReadOnlyList<EmulationControllerState> controllers, EmulationControllerState fallback)
    {
        if (!source.StartsWith("Controller:xinput:", StringComparison.OrdinalIgnoreCase)) return fallback;
        var segments = source.Split(':', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length >= 4 && int.TryParse(segments[2], out var port) && port >= 0 && port < controllers.Count
            ? controllers[port]
            : fallback;
    }

    private static int ParseXInputPort(string? deviceId, int fallback) =>
        deviceId?.StartsWith("xinput:", StringComparison.OrdinalIgnoreCase) == true
        && int.TryParse(deviceId[7..], out var port) && port is >= 0 and < 4 ? port : fallback;

    private static readonly string[] ControllerButtonNames =
        ["B", "Y", "Select", "Start", "Up", "Down", "Left", "Right", "A", "X", "L", "R", "L2", "R2", "L3", "R3"];

    private static IReadOnlyDictionary<EmulationKey, EmulationKey> BuildKeyboardMap(
        IReadOnlyDictionary<string, EmulationKey>? mappings)
    {
        if (mappings is null || mappings.Count == 0) return new Dictionary<EmulationKey, EmulationKey>();
        var result = new Dictionary<EmulationKey, EmulationKey>();
        foreach (var mapping in mappings)
            if (Enum.TryParse<EmulationKey>(mapping.Key, true, out var amigaKey) && mapping.Value != EmulationKey.Unknown)
                result[mapping.Value] = amigaKey;
        return result;
    }

    private static IReadOnlyList<KeyboardShortcutBinding> BuildKeyboardShortcuts(
        IReadOnlyDictionary<string, string>? mappings)
    {
        if (mappings is null || mappings.Count == 0) return [];
        var result = new List<KeyboardShortcutBinding>();
        foreach (var mapping in mappings)
        {
            if (!Enum.TryParse<EmulationKey>(mapping.Key, true, out var amigaKey) ||
                !KeyboardChord.TryParse(mapping.Value, out var chord)) continue;
            result.Add(new KeyboardShortcutBinding(chord, amigaKey));
        }
        return result;
    }

    private static IReadOnlyList<GlobalShortcutBinding> BuildGlobalShortcuts(
        IReadOnlyDictionary<string, string>? mappings)
    {
        if (mappings is null) return [];
        return mappings.Select(mapping => KeyboardChord.TryParse(mapping.Value, out var chord)
                ? new GlobalShortcutBinding(mapping.Key, chord) : null)
            .Where(binding => binding is not null).Cast<GlobalShortcutBinding>().ToArray();
    }

    internal static bool TryParseHostBinding(string? text, out Key key, out ModifierKeys modifiers)
    {
        key = Key.None;
        modifiers = ModifierKeys.None;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var parts = text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts[..^1])
            modifiers |= part.ToLowerInvariant() switch
            {
                "ctrl" or "control" => ModifierKeys.Control,
                "shift" => ModifierKeys.Shift,
                "alt" => ModifierKeys.Alt,
                "win" or "windows" => ModifierKeys.Windows,
                _ => ModifierKeys.None
            };
        return Enum.TryParse(parts[^1], true, out key) && key != Key.None;
    }

    private static bool IsReservedShortcut(Key key, ModifierKeys modifiers) =>
        (modifiers.HasFlag(ModifierKeys.Alt) && key is Key.F4 or Key.Tab) ||
        (modifiers.HasFlag(ModifierKeys.Control) && key == Key.Escape) ||
        (modifiers.HasFlag(ModifierKeys.Control) && modifiers.HasFlag(ModifierKeys.Shift) && key == Key.Escape) ||
        modifiers.HasFlag(ModifierKeys.Windows);

    private sealed record KeyboardShortcutBinding(KeyboardChord Chord, EmulationKey AmigaKey);
    private sealed record GlobalShortcutBinding(string Action, KeyboardChord Chord);

    internal static bool TryMapKey(Key key, out EmulationKey result)
    {
        if (key is >= Key.A and <= Key.Z) { result = (EmulationKey)((int)EmulationKey.A + key - Key.A); return true; }
        if (key is >= Key.D0 and <= Key.D9) { result = (EmulationKey)((int)EmulationKey.D0 + key - Key.D0); return true; }
        if (key is >= Key.F1 and <= Key.F10) { result = (EmulationKey)((int)EmulationKey.F1 + key - Key.F1); return true; }
        if (key is >= Key.NumPad0 and <= Key.NumPad9) { result = (EmulationKey)((int)EmulationKey.Numpad0 + key - Key.NumPad0); return true; }
        result = key switch
        {
            Key.Back => EmulationKey.Backspace, Key.Tab => EmulationKey.Tab, Key.Enter => EmulationKey.Return,
            Key.Escape => EmulationKey.Escape, Key.Space => EmulationKey.Space, Key.Left => EmulationKey.Left,
            Key.Right => EmulationKey.Right, Key.Up => EmulationKey.Up, Key.Down => EmulationKey.Down,
            Key.LeftShift => EmulationKey.LeftShift, Key.RightShift => EmulationKey.RightShift,
            Key.LeftCtrl => EmulationKey.LeftControl, Key.RightCtrl => EmulationKey.RightControl,
            Key.LeftAlt => EmulationKey.LeftAlt, Key.RightAlt => EmulationKey.RightAlt,
            Key.LWin => EmulationKey.LeftAmiga, Key.RWin => EmulationKey.RightAmiga,
            Key.Delete => EmulationKey.Delete, Key.Insert => EmulationKey.Insert,
            Key.Home => EmulationKey.Home, Key.End => EmulationKey.End,
            Key.PageUp => EmulationKey.PageUp, Key.PageDown => EmulationKey.PageDown,
            Key.CapsLock => EmulationKey.CapsLock, Key.Help => EmulationKey.Help,
            Key.OemComma => EmulationKey.Comma, Key.OemPeriod => EmulationKey.Period,
            Key.OemQuestion => EmulationKey.Slash, Key.OemMinus => EmulationKey.Minus,
            Key.OemPlus => EmulationKey.Equals, Key.OemSemicolon => EmulationKey.Semicolon,
            Key.OemQuotes => EmulationKey.Quote, Key.OemOpenBrackets => EmulationKey.LeftBracket,
            Key.OemCloseBrackets => EmulationKey.RightBracket, Key.OemBackslash => EmulationKey.Backslash,
            Key.Oem3 => EmulationKey.Backquote, Key.Decimal => EmulationKey.NumpadPeriod,
            Key.Divide => EmulationKey.NumpadDivide, Key.Multiply => EmulationKey.NumpadMultiply,
            Key.Subtract => EmulationKey.NumpadMinus, Key.Add => EmulationKey.NumpadPlus,
            _ => EmulationKey.Unknown
        };
        return result != EmulationKey.Unknown;
    }

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out NativePoint point);
    [DllImport("user32.dll")]
    private static extern IntPtr SetCapture(IntPtr hwnd);
    [DllImport("user32.dll")]
    private static extern IntPtr SetFocus(IntPtr hwnd);
    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();
    [DllImport("user32.dll")]
    private static extern IntPtr SetCursor(IntPtr cursor);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint { public int X; public int Y; }
}
