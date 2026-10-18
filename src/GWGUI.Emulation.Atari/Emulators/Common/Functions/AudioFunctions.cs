
namespace GWGUI.Emulation.Atari.Emulators.Common.Functions;

internal static class AudioFunctions
{

    internal static short[] SingleFrame(short left, short right) => EmulationAudioFunctions.SingleFrame(left, right);

    internal static AudioChunk RetainNewestFrames(AudioChunk chunk, int maximumFrames)
    => EmulationAudioFunctions.RetainNewestFrames(chunk, maximumFrames);
}

internal static class AudioOutputFunctions
{
    internal static float NormalizeVolume(float volume) => EmulationAudioFunctions.NormalizeVolume(volume);

    internal static ReadOnlySpan<short> ApplyVolume(ReadOnlySpan<short> samples, float volume, ref short[] buffer) =>
        EmulationAudioFunctions.ApplyVolume(samples, volume, ref buffer);
}
