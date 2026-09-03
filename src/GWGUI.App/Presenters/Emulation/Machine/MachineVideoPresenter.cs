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
    private EmulationVideoProcessingConfiguration _videoProcessing;
    private int _framePending;
    private int _uiFramePending;
    private int _framesInWindow;
    private long _frameWindowStarted = Stopwatch.GetTimestamp();
    private long _lastUiFrameNotification;
    private double _measuredFramesPerSecond;
    private readonly object _surfaceGate = new();
    private readonly object _gpuFrameGate = new();
    private readonly AutoResetEvent _gpuFrameAvailable = new(false);
    private readonly CancellationTokenSource _gpuWorkerCancellation = new();
    private readonly Task _gpuWorker;
    private VideoFrame? _pendingGpuFrame;
    private VideoFrame? _latestCompletedFrame;
    private bool _disposed;

    internal MachineVideoPresenter(MachineView view, IEmulatedMachine machine,
        EmulationVideoRenderer renderer,
        EmulationVideoProcessingConfiguration? videoProcessing = null)
    {
        _view = view;
        _displayHost = view.DisplayHost;
        _machine = machine;
        _videoProcessing = EmulationVideoProcessingConfigurationFunctions.Normalize(videoProcessing);
        _surface = CreateSurface(renderer);
        _gpuWorker = Task.Factory.StartNew(ProcessGpuFrames, CancellationToken.None,
            TaskCreationOptions.LongRunning, TaskScheduler.Default);
        _view.SetVideoView(_surface.View);
        _displayHost.SizeChanged += DisplayHostSizeChanged;
        _machine.Video.FrameReady += VideoFrameReady;
        FitScreen();
    }

    internal FrameworkElement InputView => _surface.View;
    internal IntPtr InputHandle => _surface.InputHandle;
    internal EmulationVideoRenderer Renderer => _surface.Renderer;
    internal EmulationVideoProcessingConfiguration VideoProcessing => _videoProcessing;
    internal System.Windows.Media.Imaging.BitmapSource? Snapshot => _surface.Snapshot;
    internal Task<System.Windows.Media.Imaging.BitmapSource?> CaptureSnapshotAsync() =>
        _surface.CaptureSnapshotAsync();
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
        IEmulationVideoSurface previous;
        lock (_surfaceGate)
        {
            previous = _surface;
            _surface = replacement;
        }
        _view.SetVideoView(replacement.View);
        previous.Dispose();
        SurfaceChanged?.Invoke(this, EventArgs.Empty);
        if (_machine.Video.LatestFrame is { } frame) QueueFrame(frame);
    }

    internal void SetVideoProcessing(EmulationVideoProcessingConfiguration configuration)
    {
        var normalized = EmulationVideoProcessingConfigurationFunctions.Normalize(configuration);
        if (_videoProcessing == normalized) return;
        _videoProcessing = normalized;
        _surface.SetVideoProcessing(normalized);
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
        if (double.IsNaN(_view.Screen.Width) || Math.Abs(_view.Screen.Width - fitted.Width) >= 0.5d)
            _view.Screen.Width = fitted.Width;
        if (double.IsNaN(_view.Screen.Height) || Math.Abs(_view.Screen.Height - fitted.Height) >= 0.5d)
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
        _disposed = true;
        lock (_gpuFrameGate) _pendingGpuFrame = null;
        _gpuWorkerCancellation.Cancel();
        _gpuFrameAvailable.Set();
        try { _gpuWorker.GetAwaiter().GetResult(); }
        catch (OperationCanceledException) { }
        lock (_surfaceGate) _surface.Dispose();
        _gpuWorkerCancellation.Dispose();
        _gpuFrameAvailable.Dispose();
    }

    private void DisplayHostSizeChanged(object sender, SizeChangedEventArgs args) => FitScreen();

    private void VideoFrameReady(object? sender, VideoFrame frame)
    {
        Interlocked.Increment(ref _framesInWindow);
        QueueFrame(_machine.Video.LatestFrame ?? frame);
    }
    private void QueueFrame(VideoFrame frame)
    {
        if (_disposed) return;
        if (_surface.Renderer == EmulationVideoRenderer.Wpf)
        {
            if (Interlocked.Exchange(ref _framePending, MachinePresentationConstants.ActiveFramePending)
                != MachinePresentationConstants.InactiveFramePending) return;
            _view.Dispatcher.BeginInvoke(() =>
            {
                try { PresentOnUi(_machine.Video.LatestFrame ?? frame); }
                finally { Interlocked.Exchange(ref _framePending, MachinePresentationConstants.InactiveFramePending); }
            });
            return;
        }
        lock (_gpuFrameGate) _pendingGpuFrame = frame;
        _gpuFrameAvailable.Set();
    }

    private void ProcessGpuFrames()
    {
        while (true)
        {
            _gpuFrameAvailable.WaitOne();
            if (_gpuWorkerCancellation.IsCancellationRequested) return;
            while (true)
            {
                VideoFrame? frame;
                lock (_gpuFrameGate)
                {
                    frame = _pendingGpuFrame;
                    _pendingGpuFrame = null;
                }
                if (frame is null) break;
                Exception? error = null;
                try { lock (_surfaceGate) _surface.Present(frame); }
                catch (Exception exception) { error = exception; }
                if (error is not null)
                {
                    _view.Dispatcher.BeginInvoke(() => FallbackAfterGpuFailure(frame));
                    lock (_gpuFrameGate) _pendingGpuFrame = null;
                    break;
                }
                NotifyFrameCompleted(frame);
            }
        }
    }

    private void PresentOnUi(VideoFrame frame)
    {
        lock (_surfaceGate) _surface.Present(frame);
        NotifyFrameCompleted(frame);
    }

    private void FallbackAfterGpuFailure(VideoFrame frame)
    {
        if (_disposed) return;
        SetRenderer(EmulationVideoRenderer.Wpf);
        PresentOnUi(frame);
    }

    private void FrameCompleted(VideoFrame frame)
    {
        if (_disposed) return;
        UpdateFrameRate();
        FitScreen(frame.AspectRatio);
        FramePresented?.Invoke(this, frame);
    }
    private void NotifyFrameCompleted(VideoFrame frame)
    {
        Interlocked.Exchange(ref _latestCompletedFrame, frame);
        var now = Stopwatch.GetTimestamp();
        var previous = Interlocked.Read(ref _lastUiFrameNotification);
        if (previous != 0 && Stopwatch.GetElapsedTime(previous, now)
                < TimeSpan.FromMilliseconds(MachinePresentationConstants.UiFrameNotificationMilliseconds))
            return;
        if (_view.Dispatcher.CheckAccess())
        {
            Interlocked.Exchange(ref _lastUiFrameNotification, now);
            FrameCompleted(Interlocked.Exchange(ref _latestCompletedFrame, null) ?? frame);
            return;
        }
        if (Interlocked.CompareExchange(ref _uiFramePending, 1, 0) != 0) return;
        Interlocked.Exchange(ref _lastUiFrameNotification, now);
        _view.Dispatcher.BeginInvoke(() =>
        {
            try
            {
                var latest = Interlocked.Exchange(ref _latestCompletedFrame, null);
                if (latest is not null) FrameCompleted(latest);
            }
            finally { Interlocked.Exchange(ref _uiFramePending, 0); }
        });
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

    private IEmulationVideoSurface CreateSurface(EmulationVideoRenderer renderer)
    {
        IEmulationVideoSurface surface;
        try { surface = EmulationVideoSurfaceFactory.Create(renderer); }
        catch when (renderer != EmulationVideoRenderer.Wpf)
        {
            surface = EmulationVideoSurfaceFactory.Create(EmulationVideoRenderer.Wpf);
        }
        surface.SetVideoProcessing(_videoProcessing);
        return surface;
    }
}
