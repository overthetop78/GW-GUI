using GWGUI.Emulation.Functions;

namespace GWGUI.Emulation.Services;

internal sealed class EmulationAudioBuffer
{
    private readonly object _gate = new();
    private readonly Queue<AudioChunk> _chunks = new();
    private int _bufferedFrames;
    private long _overrunCount;
    private long _underrunCount;

    internal int BufferedFrames { get { lock (_gate) return _bufferedFrames; } }
    internal long OverrunCount { get { lock (_gate) return _overrunCount; } }
    internal long UnderrunCount { get { lock (_gate) return _underrunCount; } }
    internal long NativeOverrunCount => _overrunCount;

    internal void Enqueue(AudioChunk chunk)
    {
        lock (_gate)
        {
            var maximumFrames = EmulationAudioFunctions.MaximumBufferedFrames(chunk.SampleRate);
            var retained = EmulationAudioFunctions.RetainNewestFrames(chunk, maximumFrames);
            EnqueueRetained(retained, maximumFrames, !ReferenceEquals(retained, chunk));
        }
    }

    internal void EnqueueNative<TSource>(AudioChunk chunk, TSource source, Func<TSource, int> readSampleRate, bool multichannel = false)
    {
        lock (_gate)
        {
            var maximumFrames = EmulationAudioFunctions.MaximumBufferedFrames(readSampleRate(source));
            var retained = EmulationAudioFunctions.RetainNativeNewestFrames(chunk, maximumFrames, multichannel);
            EnqueueRetained(retained, maximumFrames, !ReferenceEquals(retained, chunk));
        }
    }

    private void EnqueueRetained(AudioChunk chunk, int maximumFrames, bool trimmed)
    {
        if (trimmed) _overrunCount++;
        _chunks.Enqueue(chunk);
        _bufferedFrames += chunk.FrameCount;
        while (_bufferedFrames > maximumFrames && _chunks.Count > 1)
        {
            _bufferedFrames -= _chunks.Dequeue().FrameCount;
            _overrunCount++;
        }
    }

    internal bool TryDequeue(out AudioChunk? chunk) => TryDequeue(out chunk, countUnderrun: true);
    internal bool TryDequeueNative(out AudioChunk? chunk) => TryDequeue(out chunk, countUnderrun: false);

    private bool TryDequeue(out AudioChunk? chunk, bool countUnderrun)
    {
        lock (_gate)
        {
            if (!_chunks.TryDequeue(out chunk))
            {
                if (countUnderrun) _underrunCount++;
                return false;
            }
            _bufferedFrames -= chunk.FrameCount;
            return true;
        }
    }

    internal void ClearNative(Action clearLatest)
    {
        lock (_gate)
        {
            _chunks.Clear();
            _bufferedFrames = 0;
            clearLatest();
        }
    }
}
