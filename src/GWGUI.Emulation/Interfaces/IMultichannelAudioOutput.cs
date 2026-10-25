using GWGUI.Emulation.Enums;

namespace GWGUI.Emulation.Interfaces;

public interface IMultichannelAudioOutput : IAudioOutput
{
    void Start(int sampleRate, EmulationAudioChannelLayout channelLayout);
}
