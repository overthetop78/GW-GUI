using GWGUI.App.Constants.Machine;
using GWGUI.App.Factories.Rendering.Emulation;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.App.Views.Controls.Emulation.Machine;
using System.Diagnostics;
using System.Windows;
using GWGUI.Emulation;

namespace GWGUI.App.Presenters.Emulation.Machine;

internal sealed class MachineVideoPresenter : IDisposable
{
    private readonly MachineView _view;
    private FrameworkElement _displayHost;
    private IEmulatedMachine _machine;
    private IEmulationVideoSurface _surface;
    private int _framePending;
    private int _framesInWindow;
    private long _frameWindowStarted = Stopwatch.GetTimestamp();
    private double _measuredFramesPerSecond;

    internal MachineVideoPresenter(MachineView view, IEmulatedMachine machine,
        EmulationVideoRenderer renderer)
    {
        _view = view;
        _displayHost = view.DisplayHost;
        _machine = machine;
        _surface = CreateSurface(renderer);
        _view.SetVideoView(_surface.View);
        _displayHost.SizeChanged += DisplayHostSizeChanged;
        _machine.Video.FrameReady += VideoFrameReady;
        FitScreen();
    }

    internal FrameworkElement InputView => _surface.View;
    internal IntPtr InputHandle => _surface.InputHandle;
    internal EmulationVideoRenderer Renderer => _surface.Renderer;
    internal System.Windows.Media.Imaging.BitmapSource? Snapshot => _surface.Snapshot;
    internal double MeasuredFramesPerSecond => _measuredFramesPerSecond;
    internal event EventHandler<VideoFrame>? FramePresented;
    internal event EventHandler? SurfaceChanged;

    internal void SetMachine(IEmulatedMachine machine)
    {
        if (ReferenceEquals(_machine, machine)) return;
        _machine.Video.FrameReady -= VideoFrameReady;
        _machine = machine;
        _machine.Video.FrameReady += VideoFrameReady;
        ResetFrameRate();
    }

    internal void SetRenderer(EmulationVideoRenderer renderer)
    {
        if (_surface.Renderer == renderer) return;
        var replacement = CreateSurface(renderer);
        var previous = _surface;
        _surface = replacement;
        _view.SetVideoView(replacement.View);
        previous.Dispose();
        SurfaceChanged?.Invoke(this, EventArgs.Empty);
        if (_machine.Video.LatestFrame is { } frame) Present(frame);
    }

    internal void SetVisible(bool visible) =>
        _view.VideoHost.Visibility = visible ? Visibility.Visible : Visibility.Hidden;

    internal void SetDisplayHost(FrameworkElement displayHost)
    {
        if (ReferenceEquals(_displayHost, displayHost)) return;
        _displayHost.SizeChanged -= DisplayHostSizeChanged;
        _displayHost = displayHost;
        _displayHost.SizeChanged += DisplayHostSizeChanged;
        FitScreen();
    }

    internal void FitScreen(double? aspectRatio = null)
    {
        var frame = _machine.Video.LatestFrame;
        var ratio = aspectRatio ?? frame?.AspectRatio ?? MachinePresentationConstants.DefaultAspectRatio;
        var fitted = EmulationVideoLayoutFunctions.Fit(_displayHost.ActualWidth,
            _displayHost.ActualHeight, (float)ratio);
        if (fitted.IsEmpty) return;
        _view.Screen.Width = fitted.Width;
        _view.Screen.Height = fitted.Height;
    }

    internal void ResetFrameRate()
    {
        Interlocked.Exchange(ref _framesInWindow, MachinePresentationConstants.InactiveFramePending);
        _frameWindowStarted = Stopwatch.GetTimestamp();
        _measuredFramesPerSecond = MachinePresentationConstants.EmptyMeasurement;
    }

    public void Dispose()
    {
        _machine.Video.FrameReady -= VideoFrameReady;
        _displayHost.SizeChanged -= DisplayHostSizeChanged;
        _surface.Dispose();
    }

    private void DisplayHostSizeChanged(object sender, SizeChangedEventArgs args) => FitScreen();

    private void VideoFrameReady(object? sender, VideoFrame frame)
    {
        Interlocked.Increment(ref _framesInWindow);
        if (Interlocked.Exchange(ref _framePending, MachinePresentationConstants.ActiveFramePending)
            != MachinePresentationConstants.InactiveFramePending) return;
        _view.Dispatcher.BeginInvoke(() =>
        {
            try { Present(_machine.Video.LatestFrame ?? frame); }
            finally
            {
                Interlocked.Exchange(ref _framePending, MachinePresentationConstants.InactiveFramePending);
            }
        });
    }

    private void Present(VideoFrame frame)
    {
        try { _surface.Present(frame); }
        catch when (_surface.Renderer != EmulationVideoRenderer.Wpf)
        {
            SetRenderer(EmulationVideoRenderer.Wpf);
            _surface.Present(frame);
        }
        UpdateFrameRate();
        FitScreen(frame.AspectRatio);
        FramePresented?.Invoke(this, frame);
    }

    private void UpdateFrameRate()
    {
        var now = Stopwatch.GetTimestamp();
        var elapsed = Stopwatch.GetElapsedTime(_frameWindowStarted, now);
        if (elapsed < TimeSpan.FromSeconds(MachinePresentationConstants.FrameRateWindowSeconds)) return;
        var frames = Interlocked.Exchange(ref _framesInWindow,
            MachinePresentationConstants.InactiveFramePending);
        _measuredFramesPerSecond = frames / elapsed.TotalSeconds;
        _frameWindowStarted = now;
    }

    private static IEmulationVideoSurface CreateSurface(EmulationVideoRenderer renderer)
    {
        try { return EmulationVideoSurfaceFactory.Create(renderer); }
        catch when (renderer != EmulationVideoRenderer.Wpf)
        {
            return EmulationVideoSurfaceFactory.Create(EmulationVideoRenderer.Wpf);
        }
    }
}
