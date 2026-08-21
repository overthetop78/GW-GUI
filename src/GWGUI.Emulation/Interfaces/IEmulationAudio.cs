namespace GWGUI.Emulation;

public interface IEmulationAudio
{
    AudioChunk? LatestChunk { get; }
    int SampleRate { get; }
    bool IsMuted { get; }
    float Volume { get; }
    event EventHandler<AudioChunk>? ChunkReady;
    void SetMuted(bool muted);
    void SetVolume(float volume);
    void SetOutputFactory(Func<IAudioOutput?>? factory);
}
