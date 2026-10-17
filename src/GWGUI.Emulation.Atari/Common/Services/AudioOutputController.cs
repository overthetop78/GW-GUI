using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Atari.Common.Services;

internal sealed class AudioOutputController(IAudioOutput? output = null, Func<IAudioOutput?>? factory = null)
    : EmulationAudioOutputController(output, factory)
{


}
