using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Dictionaries;
using C = GWGUI.MediaEngine.Constants.MechanicalSoundConstants;

namespace GWGUI.MediaEngine.Functions;

/// <summary>Rendu mécanique mono pour tout consommateur de MediaEngine ; aucune sortie, horloge, tâche ou ressource externe possédée.</summary>
internal static partial class MechanicalSoundFunctions
{
    internal static IReadOnlyList<string> Profiles(string device) => MechanicalSoundProfiles.ForDevice(device);
    internal static short[] Render(string device, string operation, int sampleRate, int sampleCount, long offset, string? profile = null, double volumePercent = C.DefaultVolumePercent)
    {
        var recordings = RecordedMechanicalSoundFunctions.Shared;
        var fallback = RecordedMechanicalSoundFunctions.BaseProfile(device);
        if (fallback is not null)
            return recordings.Render(device, operation, sampleRate, sampleCount, offset,
                profile is not null && recordings.Contains(profile) ? profile : fallback, volumePercent);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        ArgumentOutOfRangeException.ThrowIfNegative(sampleCount);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        profile ??= Profiles(device)[0];
        if (RecordedMechanicalSoundFunctions.Shared.Contains(profile))
            return RecordedMechanicalSoundFunctions.Shared.Render(device, operation, sampleRate, sampleCount, offset, profile, volumePercent);
        var definition = MechanicalSoundProfiles.Get(profile);
        if (definition.Device != device) throw new ArgumentOutOfRangeException(nameof(profile));
        if (!double.IsFinite(volumePercent) || volumePercent is < 0 or > C.MaximumVolumePercent) throw new ArgumentOutOfRangeException(nameof(volumePercent));
        var result = device switch
        {
            C.Cassette => Cassette(operation, sampleRate, sampleCount, offset, definition),
            C.Floppy => Floppy(operation, sampleRate, sampleCount, offset, definition),
            C.HardDisk => HardDisk(operation, sampleRate, sampleCount, offset, definition),
            C.Optical => Optical(operation, sampleRate, sampleCount, offset, definition, profile),
            _ => throw new ArgumentOutOfRangeException(nameof(device))
        };
        for (var index = 0; index < result.Length; index++) result[index] = Sample(result[index] * volumePercent / C.MaximumVolumePercent);
        return result;
    }
    internal static double Duration(string device, string profile, string operation)
    {
        var recordings = RecordedMechanicalSoundFunctions.Shared;
        var fallback = RecordedMechanicalSoundFunctions.BaseProfile(device);
        if (fallback is not null)
            return recordings.Duration(recordings.Contains(profile) ? profile : fallback, operation);
        if (recordings.Contains(profile)) return recordings.Duration(profile, operation);
        return device switch
        {
            C.Cassette => operation == C.Eject ? C.CassetteEjectSeconds : C.CassetteStopSeconds,
            C.Floppy => operation == C.Seek ? C.FloppySeekSeconds : C.FloppyActionSeconds,
            C.HardDisk => operation == C.Seek ? C.HardDiskSeekSeconds : C.HardDiskStopSeconds,
            C.Optical => operation == C.Seek ? C.OpticalSeekSeconds : C.OpticalActionSeconds,
            _ => throw new ArgumentOutOfRangeException(nameof(device))
        };
    }

    /// <summary>Ne relève jamais le niveau : atténue seulement un mélange qui dépasserait le PCM, avant conversion.</summary>
    internal static short[] ProtectMix(IReadOnlyList<double> samples)
    {
        var peak = samples.Count == 0 ? 0 : samples.Max(value => Math.Abs(value));
        var gain = peak > short.MaxValue ? short.MaxValue / peak : 1;
        return samples.Select(value => Sample(value * gain)).ToArray();
    }

    private static double Noise(long position)
    {
        var random = unchecked((uint)(position * C.NoiseMultiplier + C.NoiseIncrement));
        return (random >> C.NoiseShift) / C.NoiseDivisor - 1;
    }
    private static double Rotation(double seconds, string operation, MechanicalSoundProfile profile)
    {
        var noise = operation == C.Write ? profile.WriteNoise : operation == C.Read ? profile.ReadNoise : profile.MotorNoise;
        return Math.Sin(2 * Math.PI * profile.Frequency * seconds) * profile.MotorLevel + Math.Sin(2 * Math.PI * profile.Frequency * profile.Harmonic * seconds) * profile.HarmonicLevel + Noise((long)(seconds * C.NoiseDivisor)) * noise;
    }
    private static short Sample(double level) => (short)Math.Clamp(Math.Round(level), short.MinValue, short.MaxValue);
    private static short[] Cassette(string operation, int sampleRate, int count, long offset, MechanicalSoundProfile profile)
    {
        var samples = new short[count];
        if (operation is not (C.Play or C.Record or C.FastForward or C.Rewind or C.Stop or C.Eject)) return samples;
        var transient = operation is C.Stop or C.Eject;
        var frequency = operation switch { C.FastForward => C.CassetteForwardFrequency, C.Rewind => C.CassetteRewindFrequency, _ => profile.Frequency };
        for (var index = 0; index < count; index++)
        {
            var position = offset + index; var seconds = (double)position / sampleRate;
            var envelope = transient ? Math.Max(0, 1 - seconds / (operation == C.Eject ? C.CassetteEjectSeconds : C.CassetteStopSeconds)) : Math.Min(1, seconds / profile.StartSeconds);
            var level = transient ? Noise(position) * C.CassetteTransientNoise * C.CassetteTransientLevel : Math.Sin(2 * Math.PI * frequency * seconds) * profile.MotorLevel + Math.Sin(2 * Math.PI * frequency * profile.Harmonic * seconds) * profile.HarmonicLevel + Noise(position) * profile.MotorNoise;
            samples[index] = Sample(level * envelope * profile.Gain);
        }
        return samples;
    }
}
