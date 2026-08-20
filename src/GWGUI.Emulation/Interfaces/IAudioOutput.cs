namespace GWGUI.Emulation;

public interface IAudioOutput : IDisposable
{
    void Start(int sampleRate);
    void Write(ReadOnlySpan<short> interleavedStereo);
    void Flush();
    void Stop();
}
