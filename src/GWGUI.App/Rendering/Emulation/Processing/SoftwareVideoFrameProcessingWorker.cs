using GWGUI.App.Functions.Rendering.Emulation;
using GWGUI.App.Interfaces.Rendering.Emulation;
using GWGUI.Emulation;

namespace GWGUI.App.Rendering.Emulation.Processing;

internal sealed record CpuVideoFrameProcessingResult(
    VideoFrame SourceFrame,
    VideoFrame? ProcessedFrame,
    byte[]? Bgra32Pixels,
    long ConfigurationVersion,
    Exception? Error);

internal sealed class SoftwareVideoFrameProcessingWorker : IDisposable
{
    private readonly object _gate = new();
    private readonly IEmulationVideoProcessingPipeline _pipeline;
    private WorkItem? _pending;
    private bool _running;
    private bool _disposed;

    internal SoftwareVideoFrameProcessingWorker(IEmulationVideoProcessingPipeline pipeline) =>
        _pipeline = pipeline;

    internal void Submit(EmulationVideoProcessingConfiguration configuration,
        VideoFrame frame, EmulationVideoProcessingSize outputSize,
        long configurationVersion, Action<CpuVideoFrameProcessingResult> completed)
    {
        lock (_gate)
        {
            if (_disposed) return;
            _pending = new WorkItem(configuration, frame, outputSize,
                configurationVersion, completed);
            if (_running) return;
            _running = true;
            _ = Task.Run(ProcessPending);
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            _pending = null;
            if (!_running) _pipeline.Dispose();
        }
    }

    private void ProcessPending()
    {
        while (true)
        {
            WorkItem? work;
            lock (_gate)
            {
                if (_disposed || _pending is null)
                {
                    _running = false;
                    if (_disposed) _pipeline.Dispose();
                    return;
                }
                work = _pending;
                _pending = null;
            }

            CpuVideoFrameProcessingResult result;
            try
            {
                var processed = _pipeline.Process(work.Configuration, work.Frame,
                    new EmulationVideoProcessingSize(work.Frame.Width, work.Frame.Height),
                    work.OutputSize);
                result = new(work.Frame, processed,
                    EmulationVideoPixelFunctions.ToBgra32(processed),
                    work.ConfigurationVersion, null);
            }
            catch (Exception error)
            {
                result = new(work.Frame, null, null, work.ConfigurationVersion, error);
            }

            lock (_gate)
            {
                if (_disposed) continue;
                if (_pending is not null) continue;
                _running = false;
            }
            work.Completed(result);
            return;
        }
    }

    private sealed record WorkItem(
        EmulationVideoProcessingConfiguration Configuration,
        VideoFrame Frame,
        EmulationVideoProcessingSize OutputSize,
        long ConfigurationVersion,
        Action<CpuVideoFrameProcessingResult> Completed);
}
