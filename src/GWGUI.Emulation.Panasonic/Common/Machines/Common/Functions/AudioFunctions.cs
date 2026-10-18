
namespace GWGUI.Emulation.Panasonic.Common.Machines.Common.Functions;

internal static class AudioOutputFunctions
{
    internal static float NormalizeVolume(float volume) => EmulationAudioFunctions.NormalizeVolume(volume);

    internal static ReadOnlySpan<short> ApplyVolume(ReadOnlySpan<short> samples, float volume, ref short[] buffer) =>
        EmulationAudioFunctions.ApplyVolume(samples, volume, ref buffer);
}
