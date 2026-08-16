namespace GWGUI.Emulation.Atari;

internal static class AtariAudioOutputFunctions
{
    internal static float NormalizeVolume(float volume) =>
        Math.Clamp(volume, AtariAudioOutputConstants.MinimumVolume, AtariAudioOutputConstants.MaximumVolume);

    internal static ReadOnlySpan<short> ApplyVolume(ReadOnlySpan<short> samples, float volume, ref short[] buffer)
    {
        if (volume >= AtariAudioOutputConstants.MaximumVolume) return samples;
        if (buffer.Length < samples.Length) buffer = GC.AllocateUninitializedArray<short>(samples.Length);
        for (var index = AtariAudioOutputConstants.FirstSampleIndex; index < samples.Length; index++)
            buffer[index] = (short)Math.Clamp((int)MathF.Round(samples[index] * volume),
                AtariAudioOutputConstants.MinimumSampleValue, AtariAudioOutputConstants.MaximumSampleValue);
        return buffer.AsSpan(AtariAudioOutputConstants.FirstSampleIndex, samples.Length);
    }
}
