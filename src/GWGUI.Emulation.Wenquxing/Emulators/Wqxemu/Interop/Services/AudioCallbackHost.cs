using System.Runtime.InteropServices;
using GWGUI.Emulation.Interop;

namespace GWGUI.Emulation.Wenquxing.Emulators.Wqxemu.Interop.Services;

internal sealed class AudioCallbackHost : IDisposable
{
    private readonly Queue<AudioChunk> _chunks = new();
    private int _bufferedFrames;
    private long _sequence;
    private bool _disposed;
    internal AudioCallbackHost() { SampleCallback = OnSample; BatchCallback = OnBatch; }
    internal ExternalCoreApi.AudioSampleCallback SampleCallback { get; }
    internal ExternalCoreApi.AudioBatchCallback BatchCallback { get; }
    internal int SampleRate { get; set; }
    internal AudioChunk? LatestChunk { get; private set; }
    internal Exception? Failure { get; private set; }
    internal int BufferedFrames => _bufferedFrames;
    internal long OverrunCount { get; private set; }
    internal long UnderrunCount => 0;

    private void OnSample(short left, short right)
    {
        if (_disposed) return;
        try { Enqueue([left, right], AudioConstants.SingleFrameCount); }
        catch (Exception error) { Failure ??= error; }
    }

    private nuint OnBatch(nint data, nuint frames)
    {
        if (_disposed || data == nint.Zero || frames == 0 || frames > AudioConstants.MaximumFramesPerBatch) return 0;
        try
        {
            var count = checked((int)frames);
            var samples = GC.AllocateUninitializedArray<short>(checked(count * AudioConstants.StereoChannelCount));
            Marshal.Copy(data, samples, 0, samples.Length);
            Enqueue(samples, count);
            return frames;
        }
        catch (Exception error) { Failure ??= error; return 0; }
    }

    private void Enqueue(short[] samples, int frames)
    {
        var chunk = new AudioChunk(samples, SampleRate, frames, ++_sequence, TimeSpan.Zero);
        var maximum = AudioFunctions.MaximumBufferedFrames(SampleRate);
        chunk = AudioFunctions.RetainNewestFrames(chunk, maximum);
        LatestChunk = chunk;
        while (_chunks.Count > 0 && _bufferedFrames + chunk.FrameCount > maximum) { _bufferedFrames -= _chunks.Dequeue().FrameCount; OverrunCount++; }
        _chunks.Enqueue(chunk);
        _bufferedFrames += chunk.FrameCount;
    }

    internal bool TryDequeue(out AudioChunk? chunk)
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
