namespace GWGUI.Emulation.Atari.Cores;

internal sealed class AtariAudioBuffer
{
    private readonly object _gate = new();
    private readonly Queue<AudioChunk> _chunks = new();
    private int _bufferedFrames;

    internal int BufferedFrames { get { lock (_gate) return _bufferedFrames; } }
    internal long OverrunCount { get { lock (_gate) return _overrunCount; } }
    internal long UnderrunCount { get { lock (_gate) return _underrunCount; } }
    private long _overrunCount;
    private long _underrunCount;

    internal void Enqueue(AudioChunk chunk)
    {
        lock (_gate)
        {
            var maximumFrames = AtariAudioFunctions.MaximumBufferedFrames(chunk.SampleRate);
            var retained = AtariAudioFunctions.RetainNewestFrames(chunk, maximumFrames);
            if (!ReferenceEquals(retained, chunk)) _overrunCount++;
            _chunks.Enqueue(retained);
            _bufferedFrames += retained.FrameCount;
            while (_bufferedFrames > maximumFrames && _chunks.Count > AtariAudioConstants.MinimumBufferedFrameCount)
            {
                _bufferedFrames -= _chunks.Dequeue().FrameCount;
                _overrunCount++;
            }
        }
    }

    internal bool TryDequeue(out AudioChunk? chunk)
    {
        lock (_gate)
        {
            if (!_chunks.TryDequeue(out chunk))
            {
                _underrunCount++;
                return false;
            }
            _bufferedFrames -= chunk.FrameCount;
            return true;
        }
    }
}
