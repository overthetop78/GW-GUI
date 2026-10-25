namespace GWGUI.Emulation.Interfaces;

public interface IAudioOutput : IDisposable
{
    AudioOutputBufferStatus? BufferStatus => null;
    bool TrySetMinimumLatency(int latencyMilliseconds) => false;
    void Start(int sampleRate);
    void Write(ReadOnlySpan<short> interleavedSamples);
    void Flush();
    void Stop();
}
