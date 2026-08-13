namespace GWGUI.MediaEngine.Conversion.Flux;

/// <summary>Convertit sans secteurs des cellules binaires et des intervalles de flux uniformes.</summary>
internal static class FluxBitCellConverter
{
    public static IReadOnlyList<uint> ToIntervals(
        IReadOnlyList<bool> bits,
        uint bitCellTicks)
    {
        ArgumentNullException.ThrowIfNull(bits);
        if (bitCellTicks == 0)
            throw new ArgumentOutOfRangeException(nameof(bitCellTicks));
        var intervals = new List<uint>();
        uint cells = 0;
        foreach (var bit in bits)
        {
            cells++;
            if (!bit)
                continue;
            intervals.Add(checked(cells * bitCellTicks));
            cells = 0;
        }
        return intervals;
    }

    public static IReadOnlyList<bool> ToBits(
        IReadOnlyList<uint> intervals,
        uint indexTimeTicks,
        uint bitCellTicks)
    {
        ArgumentNullException.ThrowIfNull(intervals);
        if (bitCellTicks == 0)
            throw new ArgumentOutOfRangeException(nameof(bitCellTicks));
        if (indexTimeTicks == 0 || indexTimeTicks % bitCellTicks != 0)
            throw new NotSupportedException("Le temps d'index n'est pas représentable en cellules uniformes.");
        var bits = new List<bool>(checked((int)(indexTimeTicks / bitCellTicks)));
        foreach (var interval in intervals)
        {
            if (interval == 0 || interval % bitCellTicks != 0)
                throw new NotSupportedException("Un intervalle de flux n'est pas représentable en cellules uniformes.");
            var cells = interval / bitCellTicks;
            for (uint index = 1; index < cells; index++)
                bits.Add(false);
            bits.Add(true);
        }
        var expected = checked((int)(indexTimeTicks / bitCellTicks));
        if (bits.Count > expected)
            throw new InvalidDataException("Les intervalles de flux dépassent le temps d'index.");
        while (bits.Count < expected)
            bits.Add(false);
        return bits.AsReadOnly();
    }

    public static uint GreatestCommonDivisor(IEnumerable<uint> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        uint result = 0;
        foreach (var value in values)
            result = GreatestCommonDivisor(result, value);
        return result;
    }

    private static uint GreatestCommonDivisor(uint left, uint right)
    {
        while (right != 0)
        {
            var remainder = left % right;
            left = right;
            right = remainder;
        }
        return left;
    }
}
