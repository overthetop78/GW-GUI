using GWGUI.App.Constants.Rendering.Emulation;
using GWGUI.App.Factories.Rendering.Emulation;
using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.App.Rendering.Emulation.Processing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GWGUI.Emulation;

namespace GWGUI.App.Rendering.Emulation.Surfaces;

internal sealed class WpfVideoSurface : IEmulationVideoSurface
{
    private readonly Image _image = new() { Stretch = Stretch.Fill, Focusable = true };
    private readonly IEmulationVideoProcessingPipeline? _synchronousPipeline;
    private readonly SoftwareVideoFrameProcessingWorker? _worker;
    private long _configurationVersion;
    private bool _disposed;
    private WriteableBitmap? _bitmap;
    private EmulationVideoProcessingConfiguration _videoProcessing =
        EmulationVideoProcessingConfigurationFunctions.Normalize(null);

    internal WpfVideoSurface(IEmulationVideoProcessingPipeline? videoProcessingPipeline = null)
    {
        var pipeline = videoProcessingPipeline
            ?? EmulationVideoProcessingPipelineFactory.Create(EmulationVideoRenderer.Wpf);
        if (pipeline.Renderer != EmulationVideoRenderer.Wpf)
            throw new ArgumentException(nameof(videoProcessingPipeline));
        if (videoProcessingPipeline is not null || Application.Current is null)
            _synchronousPipeline = pipeline;
        else _worker = new SoftwareVideoFrameProcessingWorker(pipeline);
    }

    public FrameworkElement View => _image;
    public BitmapSource? Snapshot => _bitmap;
    public EmulationVideoRenderer Renderer => EmulationVideoRenderer.Wpf;
    public IntPtr InputHandle => IntPtr.Zero;
    public EmulationVideoProcessingConfiguration VideoProcessing => _videoProcessing;

    public void SetVideoProcessing(EmulationVideoProcessingConfiguration configuration)
    {
        _videoProcessing = EmulationVideoProcessingConfigurationFunctions.Normalize(configuration);
        Interlocked.Increment(ref _configurationVersion);
    }

    public void Present(VideoFrame frame)
    {
        var outputSize = new EmulationVideoProcessingSize(
            _image.ActualWidth > 0 ? (int)Math.Round(_image.ActualWidth) : frame.Width,
            _image.ActualHeight > 0 ? (int)Math.Round(_image.ActualHeight) : frame.Height);
        var configuration = _videoProcessing;
        var version = Interlocked.Read(ref _configurationVersion);
        if (_worker is null)
        {
            var surfaceFrame = EmulationVideoSurfaceFrameFunctions.Process(
                _synchronousPipeline!, configuration, frame, outputSize);
            UpdateBitmap(surfaceFrame.Frame, surfaceFrame.Bgra32Pixels);
            return;
        }
        _worker.Submit(configuration, frame, outputSize, version, result =>
        {
            if (_disposed || result.Error is not null || result.ProcessedFrame is null
                || result.Bgra32Pixels is null) return;
            _image.Dispatcher.BeginInvoke(() =>
            {
                if (_disposed || result.ConfigurationVersion != Interlocked.Read(ref _configurationVersion)) return;
                UpdateBitmap(result.ProcessedFrame, result.Bgra32Pixels);
            }, System.Windows.Threading.DispatcherPriority.Render);
        });
    }

    public void Dispose()
    {
        _disposed = true;
        _worker?.Dispose();
        _synchronousPipeline?.Dispose();
    }

    private void UpdateBitmap(VideoFrame processed, byte[] pixels)
    {
        var pitch = checked(processed.Width * EmulationVideoPixelConstants.BytesPerBgraPixel);
        if (_bitmap is null || _bitmap.PixelWidth != processed.Width
            || _bitmap.PixelHeight != processed.Height)
        {
            _bitmap = new WriteableBitmap(
                processed.Width, processed.Height, 96, 96, PixelFormats.Bgra32, null);
            _image.Source = _bitmap;
        }
        _bitmap.WritePixels(
            new Int32Rect(0, 0, processed.Width, processed.Height), pixels, pitch, 0);
    }}
