using GWGUI.Domain.Settings.Emulation;
using GWGUI.App.Constants.Emulation;
using GWGUI.App.Constants.Machine;
using GWGUI.App.Contracts.Machine;
using GWGUI.App.Controllers.Emulation.Machine;
using GWGUI.App.Functions.Machine;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Localization.Extensions;
using GWGUI.App.Presenters.Emulation.Machine;
using GWGUI.App.Services.Emulation;
using GWGUI.App.Services.Input.GameInput;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GWGUI.Emulation;
using Microsoft.Win32;

namespace GWGUI.App.Views.Controls.Emulation.Machine;

internal sealed class MachineController : UserControl, IAsyncDisposable
{
    private readonly MachineControllerOptions _options;
    private readonly MachineView _view = new();
    private readonly MachineSession _session;
    private readonly MachineVideoPresenter _video;
    private readonly MachineCommandBar _commands;
    private readonly MachineInputController _input;
    private readonly Dictionary<EmulationMediaSlot, DateTime> _mediaActivityUntil = [];
    private Window? _fullscreenWindow;
    private Grid? _fullscreenHost;
    private bool _audioMuted;
    private bool _disposed;

    internal MachineController(MachineControllerOptions options)
    {
        _options = options;
        _audioMuted = options.Machine.Audio.IsMuted;
        _session = new MachineSession(options.Machine, options.MachineFactory, options.MountedMedia);
        _video = new MachineVideoPresenter(_view, options.Machine, options.VideoRenderer);
        _video.FramePresented += FramePresented;
        _session.MachineChanged += MachineChanged;
        _commands = new MachineCommandBar(_view.Toolbar, CreateActions(), options.GlobalShortcuts,
            options.ShowError);
        _commands.SetSavedStateAvailability(options.Machine.SavedStates.IsSupported,
            File.Exists(options.QuickStatePath));
        _input = new MachineInputController(_view, _video.InputView, _video.InputHandle,
            () => _session.Machine, options.GlobalShortcuts, ExecuteShortcutAsync);
        _video.SurfaceChanged += VideoSurfaceChanged;
        _commands.RendererStatus.Text = MachinePresentationFunctions.RendererName(_video.Renderer);
        _commands.SetPowered(false);
        _video.SetVisible(false);
        RebuildMediaDevices();
        Content = _view;
    }

    internal IEmulatedMachine Machine => _session.Machine;
    internal bool IsFullscreen => _fullscreenWindow is not null;

    internal async Task StopAsync()
    {
        if (_disposed) return;
        ExitFullscreen();
        _video.FramePresented -= FramePresented;
        _video.SurfaceChanged -= VideoSurfaceChanged;
        _session.MachineChanged -= MachineChanged;
        _input.Dispose();
        _video.Dispose();
        await _session.DisposeAsync();
        _disposed = true;
    }

    internal void ApplyVideoRenderer(EmulationVideoRenderer renderer)
    {
        _video.SetRenderer(renderer);
        _commands.RendererStatus.Text = MachinePresentationFunctions.RendererName(_video.Renderer);
    }

    public async ValueTask DisposeAsync() => await StopAsync();

    private MachineCommandActions CreateActions() => new(
        TogglePowerAsync,
        TogglePauseAsync,
        () => _session.Machine.Lifecycle.SoftResetAsync().AsTask(),
        HardResetAsync,
        QuickSaveAsync,
        QuickLoadAsync,
        CaptureScreenAsync,
        ToggleFullscreenAsync,
        ToggleAudioAsync,
        _options.SwitchControllerPointer ?? (_session.Machine.Input.SupportsControllerPointerSwitch
            ? SwitchControllerPointerAsync : null));

    private async Task SwitchControllerPointerAsync()
    {
        var pointerMode = await _session.Machine.Input.SwitchControllerPointerAsync();
        _commands.SetControllerPointerMode(pointerMode);
    }

    private async Task TogglePowerAsync()
    {
        await _session.TogglePowerAsync();
        _input.SetPowered(_session.IsPowered);
        _commands.SetPowered(_session.IsPowered);
        _video.SetVisible(_session.IsPowered);
        if (_session.IsPowered) _video.InputView.Focus();
        else _commands.Status.Text = string.Empty;
    }

    private Task HardResetAsync()
    {
        var requiresRecreation = _options.MediaDevices.Any(device => device.RequiresMachineRecreation
            && _session.MountedMedia.Any(media => media.Slot == device.Slot));
        return requiresRecreation
            ? _session.RecreateRunningMachineAsync()
            : _session.Machine.Lifecycle.HardResetAsync().AsTask();
    }

    private async Task TogglePauseAsync()
    {
        await _session.TogglePauseAsync();
        _commands.SetPaused(_session.Machine.State == EmulationMachineState.Paused);
    }

    private Task ToggleAudioAsync()
    {
        _audioMuted = !_audioMuted;
        _session.Machine.Audio.SetMuted(_audioMuted);
        _commands.SetMuted(_audioMuted);
        return Task.CompletedTask;
    }

    private async Task QuickSaveAsync()
    {
        if (!_session.Machine.SavedStates.IsSupported) return;
        var folder = Path.GetDirectoryName(_options.QuickStatePath);
        if (!string.IsNullOrWhiteSpace(folder)) Directory.CreateDirectory(folder);
        await _session.Machine.SavedStates.SaveAsync(_options.QuickStatePath);
        _commands.SetSavedStateAvailability(true, true);
    }

    private async Task QuickLoadAsync()
    {
        if (!_session.Machine.SavedStates.IsSupported || !File.Exists(_options.QuickStatePath)) return;
        await _session.Machine.SavedStates.LoadAsync(_options.QuickStatePath);
    }

    private Task CaptureScreenAsync()
    {
        var snapshot = _video.Snapshot
            ?? throw new InvalidOperationException(LocExtension.Get("Emulation.Shortcut.Unavailable"));
        var path = MachineCaptureFunctions.Save(snapshot, _options.CaptureFolder, DateTime.Now);
        _commands.Status.Text = Path.GetFileName(path);
        return Task.CompletedTask;
    }

    private Task ToggleFullscreenAsync()
    {
        if (_fullscreenWindow is null) EnterFullscreen();
        else ExitFullscreen();
        return Task.CompletedTask;
    }

    private void EnterFullscreen()
    {
        _input.BeginHostTransition();
        _view.DisplayHost.Children.Remove(_view.Screen);
        _fullscreenHost = new Grid
        {
            Background = Brushes.Black,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };
        _fullscreenHost.Children.Add(_view.Screen);
        _video.SetDisplayHost(_fullscreenHost);
        _fullscreenWindow = new Window
        {
            Title = _options.WindowTitle,
            Content = _fullscreenHost,
            WindowStyle = WindowStyle.None,
            WindowState = WindowState.Maximized,
            Background = Brushes.Black
        };
        _fullscreenWindow.Closed += FullscreenWindowClosed;
        _fullscreenWindow.ContentRendered += FullscreenContentRendered;
        _fullscreenWindow.Show();
        _fullscreenWindow.Activate();
        _video.InputView.Focus();
        _input.CompleteHostTransition();
    }

    private void ExitFullscreen()
    {
        if (_fullscreenWindow is null) return;
        _input.BeginHostTransition();
        var window = _fullscreenWindow;
        window.Closed -= FullscreenWindowClosed;
        window.ContentRendered -= FullscreenContentRendered;
        _fullscreenHost?.Children.Remove(_view.Screen);
        _fullscreenHost = null;
        _fullscreenWindow = null;
        _view.DisplayHost.Children.Add(_view.Screen);
        _video.SetDisplayHost(_view.DisplayHost);
        if (window.IsVisible) window.Close();
        _video.InputView.Focus();
        _input.CompleteHostTransition();
    }

    private void FullscreenWindowClosed(object? sender, EventArgs args) => ExitFullscreen();

    private void FullscreenContentRendered(object? sender, EventArgs args)
    {
        _video.FitScreen();
        _video.InputView.Focus();
    }

    private void MachineChanged(object? sender, IEmulatedMachine machine)
    {
        _video.SetMachine(machine);
        machine.Audio.SetMuted(_audioMuted);
        _commands.SetMuted(_audioMuted);
        _commands.SetSavedStateAvailability(machine.SavedStates.IsSupported,
            File.Exists(_options.QuickStatePath));
        RebuildMediaDevices();
    }

    private void RebuildMediaDevices()
    {
        _view.SetDevices(_options.MediaDevices.Select(device =>
        {
            var mounted = _session.MountedMedia.FirstOrDefault(media => media.Slot == device.Slot);
            return new MachineViewDevice(device.Slot.ToString(), device.DisplayLabel ?? device.Slot.ToString(),
                DeviceGlyph(device.MediaType), device.IsRemovable, mounted is not null,
                device.IsRemovable ? () => InsertMediaAsync(device) : null,
                device.IsRemovable && mounted is not null ? () => EjectMediaAsync(device) : null);
        }), _options.ShowError);
    }

    private async Task InsertMediaAsync(EmulationMediaDevice device)
    {
        var extensions = device.AcceptedExtensions.Select(extension => $"*{extension}").ToArray();
        var dialog = new OpenFileDialog
        {
            Filter = $"{LocExtension.Get("Emulation.Storage.Media.Associated")} ({string.Join(";", extensions)})|{string.Join(";", extensions)}",
            InitialDirectory = _options.InitialMediaDirectory(device)
        };
        if (dialog.ShowDialog() != true) return;
        var directory = Path.GetDirectoryName(dialog.FileName);
        if (!string.IsNullOrWhiteSpace(directory)) _options.RememberMediaDirectory(device, directory);
        var media = new EmulationMedia(dialog.FileName, device.Slot, device.MediaType, false, true);
        if (_options.PrepareMediaAsync is not null)
            media = await _options.PrepareMediaAsync(media, CancellationToken.None);
        await _session.InsertAsync(media, device.RequiresMachineRecreation);
        RebuildMediaDevices();
    }

    private async Task EjectMediaAsync(EmulationMediaDevice device)
    {
        await _session.EjectAsync(device.Slot, device.RequiresMachineRecreation);
        RebuildMediaDevices();
    }

    private static string DeviceGlyph(EmulationMediaType type) => type switch
    {
        EmulationMediaType.Floppy => MachinePresentationConstants.FloppyGlyph,
        EmulationMediaType.HardDisk => MachinePresentationConstants.HardDiskGlyph,
        EmulationMediaType.CompactDisc => MachinePresentationConstants.CompactDiscGlyph,
        EmulationMediaType.Cartridge => MachinePresentationConstants.CartridgeGlyph,
        EmulationMediaType.Cassette => MachinePresentationConstants.CassetteGlyph,
        _ => MachinePresentationConstants.FloppyGlyph
    };

    private void VideoSurfaceChanged(object? sender, EventArgs args) =>
        _input.SetInputView(_video.InputView, _video.InputHandle);

    private async Task ExecuteShortcutAsync(string action)
    {
        try
        {
            switch (action)
            {
                case EmulationShortcutDefaults.Power: await TogglePowerAsync(); break;
                case EmulationShortcutDefaults.PauseResume: await TogglePauseAsync(); break;
                case EmulationShortcutDefaults.SoftReset:
                    await _session.Machine.Lifecycle.SoftResetAsync();
                    break;
                case EmulationShortcutDefaults.HardReset:
                    await HardResetAsync();
                    break;
                case EmulationShortcutDefaults.QuickSave: await QuickSaveAsync(); break;
                case EmulationShortcutDefaults.QuickLoad: await QuickLoadAsync(); break;
                case EmulationShortcutDefaults.Screenshot: await CaptureScreenAsync(); break;
                case EmulationShortcutDefaults.ToggleFullscreen: await ToggleFullscreenAsync(); break;
                case EmulationShortcutDefaults.ReleaseMouse: _input.ReleasePointer(); break;
                case EmulationShortcutDefaults.ToggleMute: await ToggleAudioAsync(); break;
            }
        }
        catch (Exception error) { _options.ShowError(error); }
    }

    private void FramePresented(object? sender, VideoFrame frame)
    {
        _commands.Status.Text = MachinePresentationFunctions.Status(frame,
            _session.Machine.Video.FramesPerSecond, _video.MeasuredFramesPerSecond);
        var now = DateTime.UtcNow;
        var activityStates = _session.Machine.Runtime.MediaActivity;
        foreach (var device in _options.MediaDevices)
        {
            if (!_view.DeviceLeds.TryGetValue(device.Slot.ToString(), out var led)) continue;
            if (activityStates.GetValueOrDefault(device.Slot))
                _mediaActivityUntil[device.Slot] = now + EmulationRuntimeConstants.MediaActivityPersistence;
            var active = _mediaActivityUntil.GetValueOrDefault(device.Slot) > now;
            var present = _session.MountedMedia.Any(media => media.Slot == device.Slot)
                || !device.IsRemovable;
            led.Fill = active ? Brushes.LimeGreen : present ? Brushes.ForestGreen : Brushes.Gray;
        }
        _commands.SetInputStatus(_input.IsPointerCaptured,
            GameInputControllerReader.ReadAll().Any(state => state != EmulationControllerState.Empty));
    }
}
