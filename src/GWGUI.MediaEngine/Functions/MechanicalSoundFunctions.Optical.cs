using GWGUI.MediaEngine.Contracts;
using C = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;

namespace GWGUI.MediaEngine.Functions;

internal static partial class MechanicalSoundFunctions
{
    /// <summary>Mécanisme CD/DVD : rotation, accès, déplacement, insertion et éjection ; aucune piste audio du disque n'est mélangée ici.</summary>
    private static short[] Optical(string operation, int sampleRate, int count, long offset, MechanicalSoundProfile profile, string profileId)
    {
        var samples = new short[count];
        if (operation is not (C.Motor or C.Read or C.Write or C.Seek or C.Insert or C.Eject or C.Stop)) return samples;
        var transient = operation is C.Seek or C.Insert or C.Eject or C.Stop;
        var duration = operation == C.Seek ? C.OpticalSeekSeconds : profileId == C.OpticalSlot ? C.OpticalSlotActionSeconds : C.OpticalActionSeconds;
        for (var index = 0; index < count; index++)
        {
            var position = offset + index; var seconds = (double)position / sampleRate;
            var envelope = transient ? Math.Max(0, 1 - seconds / duration) : Math.Min(1, seconds / profile.StartSeconds);
            var level = transient ? Noise(position) * C.OpticalTransientLevel + Math.Sin(2 * Math.PI * C.OpticalTransientFrequency * seconds) * C.OpticalTransientTone : Rotation(seconds, operation, profile);
            samples[index] = Sample(level * envelope * profile.Gain);
        }
        return samples;
    }
}
