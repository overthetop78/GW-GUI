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
    private static readonly TimeSpan WorkerShutdownTimeout = TimeSpan.FromSeconds(3);
    private readonly MachineView _view;
    private readonly Func<EmulationVideoRenderer, IEmulationVideoSurface> _createSurface;
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
    private long _pendingGpuGeneration;
    private VideoFrame? _latestCompletedFrame;
    private volatile bool _disposed;
    private long _shaderLoadGeneration;
    private volatile bool _shaderLoading;
    private volatile bool _shaderPrepared;
    private volatile bool _presentationEnabled;
    private long _presentationGeneration;
    private long _historyGeneration = -1;
    private Window? _hostWindow;

    internal MachineVideoPresenter(MachineView view, IEmulatedMachine machine,
        EmulationVideoRenderer renderer,
        EmulationVideoProcessingConfiguration? videoProcessing = null,
        Func<EmulationVideoRenderer, IEmulationVideoSurface>? createSurface = null)
    {
        _view = view;
        _createSurface = createSurface ?? EmulationVideoSurfaceFactory.Create;
        _displayHost = view.DisplayHost;
        _machine = machine;
        _videoProcessing = EmulationVideoProcessingConfigurationFunctions.Normalize(videoProcessing);
        _surface = CreateSurface(renderer);
        _gpuWorker = Task.Factory.StartNew(ProcessGpuFrames, CancellationToken.None,
            TaskCreationOptions.LongRunning, TaskScheduler.Default);
        _view.SetVideoView(_surface.View);
        _shaderPrepared = _surface.Renderer == EmulationVideoRenderer.Wpf;
        _displayHost.SizeChanged += DisplayHostSizeChanged;
        _machine.Video.FrameReady += VideoFrameReady;
        _view.VideoHost.IsVisibleChanged += VideoHostVisibilityChanged;
        _view.VideoHost.Loaded += VideoHostLoaded;
        _view.VideoHost.Unloaded += VideoHostUnloaded;
        UpdatePresentationVisibility();
        FitScreen();
    }

    internal FrameworkElement InputView => _surface.View;
    internal IntPtr InputHandle => _surface.InputHandle;
    internal EmulationVideoRenderer Renderer => _surface.Renderer;
    internal EmulationVideoProcessingConfiguration VideoProcessing => _videoProcessing;
    internal System.Windows.Media.Imaging.BitmapSource? Snapshot => _surface.Snapshot;
    internal Task<System.Windows.Media.Imaging.BitmapSource?> CaptureSnapshotAsync() =>
        _presentationEnabled ? _surface.CaptureSnapshotAsync()
            : EmulationVideoSnapshotFunctions.CreateAsync(_machine.Video.LatestFrame,
                _videoProcessing, new EmulationVideoProcessingSize(
                    Math.Max(1, (int)_view.Screen.ActualWidth), Math.Max(1, (int)_view.Screen.ActualHeight)));
    internal double MeasuredFramesPerSecond => _measuredFramesPerSecond;
    internal bool IsShaderLoading => _shaderLoading;
    internal event EventHandler<VideoFrame>? FramePresented;
    internal event EventHandler? SurfaceChanged;
    internal event Action<bool>? ShaderLoadingChanged;

    internal void SetMachine(IEmulatedMachine machine)
    {
        if (ReferenceEquals(_machine, machine)) return;
        _machine.Video.FrameReady -= VideoFrameReady;
        _machine = machine;
        _machine.Video.FrameReady += VideoFrameReady;
        ResetFrameRate();
        Interlocked.Increment(ref _presentationGeneration);
        RequestCurrentFrame();
    }

    internal void SetRenderer(EmulationVideoRenderer renderer)
    {
        if (_surface.Renderer == renderer) return;
        var replacement = CreateSurface(renderer);
        Interlocked.Increment(ref _presentationGeneration);
        IEmulationVideoSurface previous;
        lock (_surfaceGate)
        {
            previous = _surface;
            _surface = replacement;
        }
        _view.SetVideoView(replacement.View);
        previous.Dispose();
        SurfaceChanged?.Invoke(this, EventArgs.Empty);
        _shaderPrepared = replacement.Renderer == EmulationVideoRenderer.Wpf;
        if (_shaderPrepared) SetShaderLoading(false);
        else if (_shaderLoading) Interlocked.Increment(ref _shaderLoadGeneration);
        RequestCurrentFrame();
    }

    internal void SetVideoProcessing(EmulationVideoProcessingConfiguration configuration)
    {
        var normalized = EmulationVideoProcessingConfigurationFunctions.Normalize(configuration);
        if (_videoProcessing == normalized) return;
        var rebuildsShader = _surface.Renderer != EmulationVideoRenderer.Wpf
            && (_videoProcessing.Sampling != normalized.Sampling
                || _videoProcessing.DisplayTechnology != normalized.DisplayTechnology);
        _videoProcessing = normalized;
        lock (_surfaceGate) _surface.SetVideoProcessing(normalized);
        if (rebuildsShader)
        {
            _shaderPrepared = false;
            if (_shaderLoading) Interlocked.Increment(ref _shaderLoadGeneration);
        }
        RequestCurrentFrame();
    }

    internal void SetVisible(bool visible) =>
        _view.VideoHost.Visibility = visible ? Visibility.Visible : Visibility.Hidden;

    internal void SetDisplayHost(FrameworkElement displayHost)
    {
        if (ReferenceEquals(_displayHost, displayHost)) return;
        _displayHost.SizeChanged -= DisplayHostSizeChanged;
        _displayHost = displayHost;
        _displayHost.SizeChanged += DisplayHostSizeChanged;
        UpdatePresentationVisibility();
        FitScreen();
    }

    internal void FitScreen(double? aspectRatio = null)
    {
        var frame = _machine.Video.LatestFrame;
        var ratio = aspectRatio ?? (frame is { Width: > 0, Height: > 0 }
            ? frame.Width / (double)frame.Height
            : MachinePresentationConstants.DefaultAspectRatio);
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
        if (_disposed) return;
        _machine.Video.FrameReady -= VideoFrameReady;
        _displayHost.SizeChanged -= DisplayHostSizeChanged;
        _view.VideoHost.IsVisibleChanged -= VideoHostVisibilityChanged;
        _view.VideoHost.Loaded -= VideoHostLoaded;
        _view.VideoHost.Unloaded -= VideoHostUnloaded;
        if (_hostWindow is not null) _hostWindow.StateChanged -= HostWindowStateChanged;
        _presentationEnabled = false;
        _disposed = true;
        lock (_gpuFrameGate) _pendingGpuFrame = null;
        _gpuWorkerCancellation.Cancel();
        _gpuFrameAvailable.Set();
        var workerStopped = false;
        try { workerStopped = _gpuWorker.Wait(WorkerShutdownTimeout); }
        catch (AggregateException error) when (error.InnerExceptions.All(exception =>
                   exception is OperationCanceledException)) { workerStopped = true; }
        if (workerStopped)
        {
            lock (_surfaceGate) _surface.Dispose();
            _gpuWorkerCancellation.Dispose();
            _gpuFrameAvailable.Dispose();
        }
    }

    private void DisplayHostSizeChanged(object sender, SizeChangedEventArgs args) => FitScreen();

    private void VideoHostVisibilityChanged(object sender, DependencyPropertyChangedEventArgs args) =>
        UpdatePresentationVisibility();
    private void VideoHostLoaded(object sender, RoutedEventArgs args) => UpdatePresentationVisibility();
    private void VideoHostUnloaded(object sender, RoutedEventArgs args) => SetPresentationEnabled(false);
    private void HostWindowStateChanged(object? sender, EventArgs args) => UpdatePresentationVisibility();

    private void UpdatePresentationVisibility()
    {
        if (_disposed) return;
        var window = Window.GetWindow(_view.VideoHost);
        if (!ReferenceEquals(window, _hostWindow))
        {
            if (_hostWindow is not null) _hostWindow.StateChanged -= HostWindowStateChanged;
            _hostWindow = window;
            if (window is not null) window.StateChanged += HostWindowStateChanged;
        }
        SetPresentationEnabled(_view.VideoHost.IsLoaded && _view.VideoHost.IsVisible
            && window?.WindowState != WindowState.Minimized);
    }

    private void SetPresentationEnabled(bool enabled)
    {
        lock (_gpuFrameGate)
        {
            if (_presentationEnabled == enabled) return;
            Interlocked.Increment(ref _presentationGeneration);
            _presentationEnabled = enabled;
            if (!enabled) _pendingGpuFrame = null;
        }
        if (!enabled)
        {
            _surface.SuspendPresentation();
        }
        else RequestCurrentFrame();
    }

    private void RequestCurrentFrame() => _view.Dispatcher.BeginInvoke(() =>
    {
        if (!_disposed && _presentationEnabled && _machine.Video.LatestFrame is { } frame)
            QueueFrame(frame);
    }, System.Windows.Threading.DispatcherPriority.Loaded);

    private void VideoFrameReady(object? sender, VideoFrame frame)
    {
        Interlocked.Increment(ref _framesInWindow);
        if (!_presentationEnabled)
        {
            if (!_disposed) NotifyFrameCompleted(frame);
            return;
        }
        QueueFrame(_machine.Video.LatestFrame ?? frame);
    }
    private void QueueFrame(VideoFrame frame)
    {
        if (_disposed || !_presentationEnabled) return;
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
        if (!_shaderPrepared) SetShaderLoading(true);
        lock (_gpuFrameGate)
        {
            if (_disposed || !_presentationEnabled) return;
            _pendingGpuFrame = frame;
            _pendingGpuGeneration = Volatile.Read(ref _presentationGeneration);
            _gpuFrameAvailable.Set();
        }
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
                long presentationGeneration;
                lock (_gpuFrameGate)
                {
                    frame = _pendingGpuFrame;
                    presentationGeneration = _pendingGpuGeneration;
                    _pendingGpuFrame = null;
                }
                if (frame is null) break;
                Exception? error = null;
                long generation = 0;
                try
                {
                    lock (_surfaceGate)
                    {
                        if (_disposed || !_presentationEnabled
                            || presentationGeneration != Volatile.Read(ref _presentationGeneration)) continue;
                        PrepareHistory(presentationGeneration);
                        _surface.Present(frame);
                        generation = Volatile.Read(ref _shaderLoadGeneration);
                    }
                }
                catch (Exception exception) { error = exception; }
                if (error is not null)
                {
                    _view.Dispatcher.BeginInvoke(() =>
                    {
                        if (presentationGeneration == Volatile.Read(ref _presentationGeneration))
                            FallbackAfterGpuFailure(frame);
                    });
                    lock (_gpuFrameGate) _pendingGpuFrame = null;
                    break;
                }
                if (presentationGeneration != Volatile.Read(ref _presentationGeneration)) continue;
                _shaderPrepared = true;
                CompleteShaderLoading(generation);
                NotifyFrameCompleted(frame);
            }
        }
    }

    private void PresentOnUi(VideoFrame frame)
    {
        if (_disposed || !_presentationEnabled) return;
        lock (_surfaceGate)
        {
            PrepareHistory(Volatile.Read(ref _presentationGeneration));
            _surface.Present(frame);
        }
        CompleteShaderLoading(Volatile.Read(ref _shaderLoadGeneration));
        NotifyFrameCompleted(frame);
    }

    private void PrepareHistory(long generation)
    {
        if (_historyGeneration == generation) return;
        _surface.ResetHistory();
        _historyGeneration = generation;
    }

    private void SetShaderLoading(bool loading)
    {
        if (loading) Interlocked.Increment(ref _shaderLoadGeneration);
        if (_shaderLoading == loading) return;
        _shaderLoading = loading;
        if (_view.Dispatcher.CheckAccess()) ShaderLoadingChanged?.Invoke(loading);
        else _view.Dispatcher.BeginInvoke(() => ShaderLoadingChanged?.Invoke(loading));
    }

    private void CompleteShaderLoading(long generation)
    {
        if (!_shaderLoading || generation != Volatile.Read(ref _shaderLoadGeneration)) return;
        _view.Dispatcher.BeginInvoke(() =>
        {
            if (generation == Volatile.Read(ref _shaderLoadGeneration)) SetShaderLoading(false);
        });
    }

    private void FallbackAfterGpuFailure(VideoFrame frame)
    {
        if (_disposed) return;
        SetRenderer(EmulationVideoRenderer.Wpf);
    }

    private void FrameCompleted(VideoFrame frame)
    {
        if (_disposed) return;
        UpdateFrameRate();
        if (_presentationEnabled) FitScreen(frame.Width / (double)frame.Height);
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
        try { surface = _createSurface(renderer); }
        catch when (renderer != EmulationVideoRenderer.Wpf)
        {
            surface = _createSurface(EmulationVideoRenderer.Wpf);
        }
        surface.SetVideoProcessing(_videoProcessing);
        return surface;
    }
}
