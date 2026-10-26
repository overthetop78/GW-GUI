using GWGUI.MediaEngine.Contracts;
using C = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;

namespace GWGUI.MediaEngine.Functions;

internal static partial class MechanicalSoundFunctions
{
    private static short[] HardDisk(string operation, int sampleRate, int count, long offset, MechanicalSoundProfile profile)
    {
        var samples = new short[count];
        if (operation is not (C.Motor or C.Read or C.Write or C.Seek or C.Stop)) return samples;
        for (var index = 0; index < count; index++)
        {
            var position = offset + index; var seconds = (double)position / sampleRate;
            var envelope = operation is C.Seek or C.Stop ? Math.Max(0, 1 - seconds / (operation == C.Seek ? C.HardDiskSeekSeconds : C.HardDiskStopSeconds)) : Math.Min(1, seconds / profile.StartSeconds);
            var level = operation == C.Seek ? Noise(position) * C.HardDiskTransientLevel + Math.Sin(2 * Math.PI * C.HardDiskTransientFrequency * seconds) * C.HardDiskTransientTone : Rotation(seconds, operation, profile);
            samples[index] = Sample(level * envelope * profile.Gain);
        }
        return samples;
    }
}
