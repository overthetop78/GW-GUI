namespace GWGUI.MediaEngine.Functions;

/// <summary>Synthèse mécanique commune aux magnétophones virtuels, indépendante des données enregistrées.</summary>
/// <remarks>Ne possède aucune sortie audio ni horloge hôte. Le consommateur fournit sa cadence et une position
/// persistante depuis le début de l'opération. Le résultat est mélangé uniquement à la sortie sonore, jamais à l'image cassette.</remarks>
internal static class CassetteSoundFunctions
{
    internal static short[] Render(string operation, int sampleRate, int sampleCount, long offset)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleRate);
        ArgumentOutOfRangeException.ThrowIfNegative(sampleCount);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        var samples = new short[sampleCount];
        if (operation is not ("Play" or "Record" or "FastForward" or "Rewind" or "Stop" or "Eject")) return samples;
        var transient = operation is "Stop" or "Eject";
        var frequency = operation switch { "FastForward" => 190, "Rewind" => 165, _ => 55 };
        for (var index = 0; index < sampleCount; index++)
        {
            var position = offset + index;
            var seconds = (double)position / sampleRate;
            var random = unchecked((uint)(position * 1664525 + 1013904223));
            var noise = ((random >> 16) / 32768.0 - 1) * 0.3;
            var envelope = transient ? Math.Max(0, 1 - seconds / (operation == "Eject" ? 0.12 : 0.05)) : Math.Min(1, seconds / 0.02);
            var level = transient ? noise * 2200 : (Math.Sin(2 * Math.PI * frequency * seconds) + 0.2 * Math.Sin(2 * Math.PI * 3 * frequency * seconds) + noise) * 300;
            samples[index] = (short)Math.Clamp(Math.Round(level * envelope), short.MinValue, short.MaxValue);
        }
        return samples;
    }
}
