using System.Collections.Immutable;

namespace GWGUI.MediaEngine.Representations.Flux;

/// <summary>Conserve une représentation immuable des bits reconstruits et de leur durée de cellule.</summary>
internal sealed class FluxBitstream
{
    /// <summary>Durée minimale acceptée pour une cellule de bit, en ticks.</summary>
    public const double MinimumBitCellTicks = 1d;

    /// <summary>Crée une représentation en copiant les bits et en validant leur durée de cellule.</summary>
    /// <param name="bits">Bits reconstruits à copier.</param>
    /// <param name="bitCellTicks">Durée d'une cellule, en ticks.</param>
    /// <exception cref="ArgumentOutOfRangeException">La durée de cellule n'est pas finie ou est inférieure à la durée minimale.</exception>
    public FluxBitstream(bool[] bits, double bitCellTicks)
    {
        if (!double.IsFinite(bitCellTicks) || bitCellTicks < MinimumBitCellTicks) throw new ArgumentOutOfRangeException(nameof(bitCellTicks), bitCellTicks, $"La durée d'une cellule doit être finie et supérieure ou égale à {MinimumBitCellTicks} tick.");
        Bits = [.. bits];
        BitCellTicks = bitCellTicks;
    }

    /// <summary>Bits reconstruits exposés sans mutation possible.</summary>
    public ImmutableArray<bool> Bits { get; }

    /// <summary>Durée d'une cellule de bit, en ticks.</summary>
    public double BitCellTicks { get; }

    public FluxBitstream WithCircularTail(int bitCount)
    {
        if (Bits.Length == 0 || bitCount <= 0) return this;
        var tailLength = Math.Min(bitCount, Bits.Length);
        var extended = new bool[Bits.Length + tailLength];
        Bits.CopyTo(extended);
        Bits.AsSpan(0, tailLength).CopyTo(extended.AsSpan(Bits.Length));
        return new(extended, BitCellTicks);
    }
}
