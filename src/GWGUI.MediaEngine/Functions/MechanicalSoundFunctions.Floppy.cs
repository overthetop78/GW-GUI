using GWGUI.MediaEngine.Contracts;
using C = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;

namespace GWGUI.MediaEngine.Functions;

internal static partial class MechanicalSoundFunctions
{
    private static short[] Floppy(string operation, int sampleRate, int count, long offset, MechanicalSoundProfile profile)
    {
        var samples = new short[count];
        if (operation is not (C.Motor or C.Read or C.Write or C.Seek or C.Insert or C.Eject or C.Stop)) return samples;
        var transient = operation is C.Seek or C.Insert or C.Eject or C.Stop;
        var duration = operation == C.Seek ? C.FloppySeekSeconds : C.FloppyActionSeconds;
        for (var index = 0; index < count; index++)
        {
            var position = offset + index; var seconds = (double)position / sampleRate;
            var envelope = transient ? Math.Max(0, 1 - seconds / duration) : Math.Min(1, seconds / profile.StartSeconds);
            var level = transient ? Noise(position) * C.FloppyTransientLevel + Math.Sin(2 * Math.PI * C.FloppyTransientFrequency * seconds) * C.FloppyTransientTone : Rotation(seconds, operation, profile);
            samples[index] = Sample(level * envelope * profile.Gain);
        }
        return samples;
    }
}
