using System.Runtime.InteropServices;
using System.Globalization;

namespace GWGUI.Emulation.Functions;

/// <summary>Copies et réglages audio communs aux modules indépendants.</summary>
/// <remarks>Les sorties et pointeurs fournis sont empruntés. Les règles existantes de taille, canaux, allocation et arrondi sont conservées ; les buffers natifs doivent rester valides pendant la copie.</remarks>
internal static class EmulationAudioFunctions
{
    /// <summary>Vérifie une latence configurée avec les choix communs du SDK et la culture invariante.</summary>
    public static bool HasInvalidConfiguredLatency(string? value) =>
        !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var latency)
            || !EmulationSettingConstants.AudioLatencyMillisecondsChoices.Contains(latency);

    /// <summary>Vérifie un volume entier configuré dans les bornes fournies.</summary>
    public static bool HasInvalidConfiguredVolume(string? value, int minimum, int maximum) =>
        !int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var volume)
            || volume < minimum || volume > maximum;

    /// <summary>Résout la sortie sélectionnée ; la valeur automatique du module devient null.</summary>
    public static string? ResolveRuntimeOutput(IReadOnlyDictionary<string, string> options, string key, string defaultOutput)
    {
        var output = options.GetValueOrDefault(key);
        if (output == defaultOutput) output = null;
        return output;
    }

    /// <summary>Résout la latence sélectionnée ou utilise la valeur par défaut commune.</summary>
    public static int ResolveRuntimeLatency(IReadOnlyDictionary<string, string> options, string key) =>
        options.TryGetValue(key, out var selectedLatency)
            && int.TryParse(selectedLatency, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                ? parsed : EmulationSettingConstants.DefaultAudioLatencyMilliseconds;

    /// <summary>Écrit les échantillons dans la sortie empruntée avec la règle de volume de cette variante.</summary>
    public static void WriteVolume(ref IAudioOutput? output, ref float volume, ReadOnlySpan<short> samples)
    {
        if (output is null) return;
        if (volume >= 1f)
        {
            output.Write(samples);
            return;
        }

        var scaled = new short[samples.Length];
        for (var index = 0; index < samples.Length; index++)
            scaled[index] = (short)Math.Clamp(samples[index] * volume, short.MinValue, short.MaxValue);
        output.Write(scaled);
    }

    /// <summary>Calcule la capacité audio commune à partir de la fréquence des échantillons.</summary>
    public static int MaximumBufferedFrames(int sampleRate) => Math.Max(1, sampleRate / 5);

    /// <summary>Conserve les frames natives les plus récentes en respectant la disposition stéréo ou multicanale demandée.</summary>
    public static AudioChunk RetainNativeNewestFrames(AudioChunk chunk, int maximumFrames, bool multichannel)
    {
        if (chunk.FrameCount <= maximumFrames) return chunk;
        var retainedSamples = chunk.InterleavedSamples.Slice(
            (chunk.FrameCount - maximumFrames) * (multichannel ? chunk.ChannelCount : 2)).ToArray();
        return multichannel
            ? chunk with { InterleavedSamples = retainedSamples, FrameCount = maximumFrames }
            : new AudioChunk(retainedSamples, chunk.SampleRate, maximumFrames, chunk.Sequence, chunk.Timestamp);
    }

    /// <summary>Copie les frames du pointeur natif emprunté dans un tableau possédé par le résultat, avec contrôle arithmétique des tailles.</summary>
    public static short[] CopyBatch(nint data, int frameCount)
        => CopyBatch(data, GC.AllocateUninitializedArray<short>(checked(frameCount * 2)));

    /// <summary>Copie les frames du pointeur natif emprunté dans un tableau possédé par le résultat, avec contrôle arithmétique des tailles.</summary>
    public static short[] CopyBatch(nint data, nuint frameCount)
        => CopyBatch(data, frameCount, 2);

    /// <summary>Copie les frames du pointeur natif emprunté dans un tableau possédé par le résultat, avec contrôle arithmétique des tailles.</summary>
    public static short[] CopyBatch(nint data, nuint frameCount, uint channelCount)
        => CopyBatch(data, new short[checked((int)frameCount * (int)channelCount)]);

    private static short[] CopyBatch(nint data, short[] samples)
    {
        Marshal.Copy(data, samples, 0, samples.Length);
        return samples;
    }

    /// <summary>Crée les deux échantillons gauche et droit d’une frame stéréo.</summary>
    public static short[] SingleFrame(short left, short right) => [left, right];

    /// <summary>Conserve les frames stéréo les plus récentes dans la limite demandée.</summary>
    public static AudioChunk RetainNewestFrames(AudioChunk chunk, int maximumFrames)
    {
        if (chunk.FrameCount <= maximumFrames) return chunk;
        var firstSample = checked((chunk.FrameCount - maximumFrames) * 2);
        return chunk with
        {
            InterleavedSamples = chunk.InterleavedSamples[firstSample..].ToArray(),
            FrameCount = maximumFrames
        };
    }

    /// <summary>Applique le bornage commun du volume, avec les règles numériques de Math.Clamp.</summary>
    public static float NormalizeVolume(float volume) =>
        Math.Clamp(volume, 0f, 1f);

    /// <summary>Applique le volume dans le buffer réutilisable fourni ou emprunte les échantillons lorsque le volume est maximal.</summary>
    public static ReadOnlySpan<short> ApplyVolume(ReadOnlySpan<short> samples, float volume, ref short[] buffer)
    {
        if (volume >= 1f) return samples;
        if (buffer.Length < samples.Length) buffer = GC.AllocateUninitializedArray<short>(samples.Length);
        for (var index = 0; index < samples.Length; index++)
            buffer[index] = (short)Math.Clamp((int)MathF.Round(samples[index] * volume),
                short.MinValue, short.MaxValue);
        return buffer.AsSpan(0, samples.Length);
    }
}
