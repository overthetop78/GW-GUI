using GWGUI.Emulation.Services;

namespace GWGUI.Emulation.Philips.Common.Services;

internal sealed class AudioOutputController(IAudioOutput? output = null, Func<IAudioOutput?>? factory = null)
    : EmulationAudioOutputController(output, factory)
{


}
