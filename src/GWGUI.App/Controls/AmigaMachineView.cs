using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GWGUI.App.Localization;
using GWGUI.App.Services;
using GWGUI.Emulation;
using GWGUI.Emulation.Amiga;
using Microsoft.Win32;

namespace GWGUI.App.Controls;

public sealed class AmigaMachineView : UserControl
{
    private readonly IAmigaMachine _machine;
    private readonly Image _display = new() { Stretch = Stretch.Uniform, Focusable = true };
    private readonly TextBlock _status = new() { VerticalAlignment = VerticalAlignment.Center };
    private readonly ComboBox _diskSelection = new() { MinWidth = 120, Margin = new Thickness(0, 0, 8, 0) };
    private readonly HashSet<EmulationKey> _keys = [];
    private WriteableBitmap? _bitmap;
    private Point? _lastMouse;
    private int _framePending;
    private bool _disposed;
    private readonly DispatcherTimer _inputTimer = new() { Interval = TimeSpan.FromMilliseconds(16) };

    public AmigaMachineView(IAmigaMachine machine)
    {
        _machine = machine;
        var root = new Grid();
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition());
        var bar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        AddButton(bar, "Common.Stop", StopAsync);
        AddButton(bar, "Common.Reset", () => _machine.HardResetAsync().AsTask());
        AddButton(bar, "Common.Browse", InsertDisk);
        bar.Children.Add(_diskSelection);
        AddButton(bar, "Common.Choose", SelectDisk);
        AddButton(bar, "Common.Save", SaveState);
        AddButton(bar, "Common.Choose", LoadState);
        AddButton(bar, "Common.Close", () =>
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        });
        bar.Children.Add(_status);
        root.Children.Add(bar);
        var border = new Border { Background = Brushes.Black, Child = _display, Padding = new Thickness(2) };
        Grid.SetRow(border, 1); root.Children.Add(border);
        Content = root;

        _machine.VideoFrameReady += VideoFrameReady;
        _display.KeyDown += DisplayKeyDown;
        _display.KeyUp += DisplayKeyUp;
        _display.LostKeyboardFocus += DisplayLostKeyboardFocus;
        _display.MouseMove += DisplayMouseMove;
        _display.MouseDown += MouseChanged;
        _display.MouseUp += MouseChanged;
        _display.MouseWheel += DisplayMouseWheel;
        _display.MouseDown += (_, _) => _display.Focus();
        _inputTimer.Tick += (_, _) => PublishInput();
    }

    public event EventHandler? CloseRequested;

    public async Task StartAsync()
    {
        _status.Text = "Starting";
        await _machine.StartAsync();
        _inputTimer.Start();
        _diskSelection.ItemsSource = Enumerable.Range(0, _machine.DiskCount).Select(index => LocExtension.Get("Emulation.DiskNumber", index + 1)).ToArray();
        _diskSelection.SelectedIndex = _machine.CurrentDiskIndex;
        _status.Text = _machine.State.ToString();
        _display.Focus();
    }

    public async Task StopAsync()
    {
        if (_disposed) return;
        try { await _machine.StopAsync(); }
        finally
        {
            _inputTimer.Stop();
            await _machine.DisposeAsync();
            _machine.VideoFrameReady -= VideoFrameReady;
            _status.Text = _machine.State.ToString();
            _disposed = true;
        }
    }

    private void AddButton(Panel panel, string key, Func<Task> action)
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
    }

    private void VideoFrameReady(object? sender, VideoFrame frame)
    {
        if (Interlocked.Exchange(ref _framePending, 1) != 0) return;
        Dispatcher.BeginInvoke(() =>
        {
            try { Render(_machine.LatestVideoFrame ?? frame); }
            finally { Interlocked.Exchange(ref _framePending, 0); }
        });
    }

    private void Render(VideoFrame frame)
    {
        var format = frame.PixelFormat == EmulationPixelFormat.Rgb565 ? PixelFormats.Bgr565 : PixelFormats.Bgr32;
        if (_bitmap is null || _bitmap.PixelWidth != frame.Width || _bitmap.PixelHeight != frame.Height || _bitmap.Format != format)
        {
            _bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, format, null);
            _display.Source = _bitmap;
        }
        _bitmap.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), frame.Pixels.ToArray(), frame.Pitch, 0);
        _status.Text = $"{frame.Width}×{frame.Height} · {frame.Sequence}";
    }

    private async Task InsertDisk()
    {
        var dialog = new OpenFileDialog { Filter = "Amiga disk|*.adf;*.adz;*.ipf;*.dms;*.hdf;*.lha;*.iso;*.cue|All files|*.*" };
        if (dialog.ShowDialog() == true) await _machine.InsertFloppyAsync(dialog.FileName);
    }

    private async Task SaveState()
    {
        var dialog = new SaveFileDialog { Filter = "GW GUI Amiga state|*.gwas", DefaultExt = ".gwas" };
        if (dialog.ShowDialog() == true) await _machine.SaveStateAsync(dialog.FileName);
    }

    private async Task SelectDisk()
    {
        if (_diskSelection.SelectedIndex < 0) return;
        await _machine.SelectDiskAsync(_diskSelection.SelectedIndex);
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
        if (TryMapKey(source, out var key)) { _keys.Add(key); PublishInput(); e.Handled = true; }
    }

    private void DisplayKeyUp(object sender, KeyEventArgs e)
    {
        var source = e.Key == Key.System ? e.SystemKey : e.Key;
        if (TryMapKey(source, out var key)) { _keys.Remove(key); PublishInput(); e.Handled = true; }
    }

    private void DisplayLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) { _keys.Clear(); PublishInput(); }

    private void DisplayMouseMove(object sender, MouseEventArgs e)
    {
        var current = e.GetPosition(_display);
        if (_lastMouse is { } previous) PublishInput((int)(current.X - previous.X), (int)(current.Y - previous.Y));
        _lastMouse = current;
    }

    private void MouseChanged(object sender, MouseButtonEventArgs e) => PublishInput();
    private void DisplayMouseWheel(object sender, MouseWheelEventArgs e) => PublishInput(wheel: e.Delta);

    private void PublishInput(int deltaX = 0, int deltaY = 0, int wheel = 0) => _machine.SetInput(new EmulationInputSnapshot(
        new HashSet<EmulationKey>(_keys), new EmulationPointerState(deltaX, deltaY, wheel,
            Mouse.LeftButton == MouseButtonState.Pressed, Mouse.RightButton == MouseButtonState.Pressed,
            Mouse.MiddleButton == MouseButtonState.Pressed),
        XInputControllerReader.ReadAll()));

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
}
