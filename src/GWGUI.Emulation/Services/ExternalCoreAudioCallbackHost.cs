using GWGUI.Emulation;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Services;

internal sealed class ExternalCoreAudioCallbackHost : IEmulationAudioCallbackHost
{
    private const int MaximumFramesPerBatch = 64 * 1024;
    private readonly Queue<AudioChunk> _chunks = new();
    private int _bufferedFrames;
    private long _sequence;
    private bool _disposed;
    internal ExternalCoreAudioCallbackHost() { SampleCallback = OnSample; BatchCallback = OnBatch; }
    public ExternalCoreApi.AudioSampleCallback SampleCallback { get; }
    public ExternalCoreApi.AudioBatchCallback BatchCallback { get; }
    public int SampleRate { get; set; }
    public AudioChunk? LatestChunk { get; private set; }
    public Exception? Failure { get; private set; }
    public int BufferedFrames => _bufferedFrames;
    public long OverrunCount { get; private set; }
    public long UnderrunCount => 0;

    private void OnSample(short left, short right)
    {
        if (_disposed) return;
        try { Enqueue(EmulationSdk.AudioProcessing.SingleFrame(left, right), 1); }
        catch (Exception error) { Failure ??= error; }
    }

    private nuint OnBatch(nint data, nuint frames)
    {
        if (_disposed || data == nint.Zero || frames == 0 || frames > MaximumFramesPerBatch) return 0;
        try
        {
            var count = checked((int)frames);
            var samples = EmulationSdk.AudioProcessing.CopyBatch(data, count);
            Enqueue(samples, count);
            return frames;
        }
        catch (Exception error) { Failure ??= error; return 0; }
    }

    private void Enqueue(short[] samples, int frames)
    {
        var chunk = new AudioChunk(samples, SampleRate, frames, ++_sequence, TimeSpan.Zero);
        var maximum = EmulationSdk.AudioProcessing.MaximumBufferedFrames(SampleRate);
        chunk = EmulationSdk.AudioProcessing.RetainNewestFrames(chunk, maximum);
        LatestChunk = chunk;
        while (_chunks.Count > 0 && _bufferedFrames + chunk.FrameCount > maximum) { _bufferedFrames -= _chunks.Dequeue().FrameCount; OverrunCount++; }
        _chunks.Enqueue(chunk);
        _bufferedFrames += chunk.FrameCount;
    }

    public bool TryDequeue(out AudioChunk? chunk)
    {
        if (!_chunks.TryDequeue(out chunk)) return false;
        _bufferedFrames -= chunk.FrameCount;
        return true;
    }

    public void Dispose()
    {
        _disposed = true;
        _chunks.Clear();
        _bufferedFrames = 0;
        LatestChunk = null;
        Failure = null;
    }
}