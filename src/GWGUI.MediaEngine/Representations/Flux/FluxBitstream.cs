using System.Collections.Immutable;

namespace GWGUI.MediaEngine.Representations.Flux;

/// <summary>Conserve une représentation immuable des bits reconstruits et de leur durée de cellule.</summary>
/// <param name="bits">Bits reconstruits à copier.</param>
/// <param name="bitCellTicks">Durée d'une cellule, en ticks.</param>
/// <exception cref="ArgumentOutOfRangeException">La durée de cellule n'est pas finie ou est inférieure à la durée minimale.</exception>
internal sealed class FluxBitstream(bool[] bits, double bitCellTicks)
{
    /// <summary>Bits reconstruits exposés sans mutation possible.</summary>
    public ImmutableArray<bool> Bits { get; } = [.. bits];

    /// <summary>Durée d'une cellule de bit, en ticks.</summary>
    public double BitCellTicks { get; } = ValidateBitCellTicks(bitCellTicks);

    /// <summary>Ajoute une queue circulaire limitée à une longueur complète du flux source.</summary>
    /// <param name="bitCount">Nombre de bits demandé pour la queue.</param>
    /// <returns>L'instance courante lorsqu'aucune queue n'est nécessaire ; sinon une nouvelle représentation prolongée.</returns>
    public FluxBitstream WithCircularTail(int bitCount)
    {
        if (Bits.Length == 0 || bitCount <= 0) return this;
        var maximumTailLength = Bits.Length;
        var tailLength = Math.Min(bitCount, maximumTailLength);
        var extendedLength = checked(Bits.Length + tailLength);
        var extended = new bool[extendedLength];
        Bits.CopyTo(extended);
        Bits.AsSpan(0, tailLength).CopyTo(extended.AsSpan(Bits.Length));
        return new(extended, BitCellTicks);
    }

    /// <summary>Valide et retourne la durée de cellule fournie au constructeur primaire.</summary>
    /// <param name="value">Durée de cellule à valider, en ticks.</param>
    /// <returns>Durée de cellule validée.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La durée de cellule n'est pas finie ou est inférieure à la durée minimale.</exception>
    private static double ValidateBitCellTicks(double value)
    {
        if (!double.IsFinite(value) || value < FluxDecodingParameters.MinimumBitCellTicks) throw new ArgumentOutOfRangeException(nameof(bitCellTicks), value, $"La durée d'une cellule doit être finie et supérieure ou égale à {FluxDecodingParameters.MinimumBitCellTicks} tick.");
        return value;
    }
}
