namespace GWGUI.MediaEngine.Representations.Flux;

internal sealed class FluxBitstream(bool[] bits, double bitCellTicks)
{
    public bool[] Bits { get; } = bits; public double BitCellTicks { get; } = bitCellTicks;
    public FluxBitstream WithCircularTail(int bitCount)
    {
        if (Bits.Length == 0 || bitCount <= 0) return this;
        var tailLength = Math.Min(bitCount, Bits.Length);
        var extended = new bool[Bits.Length + tailLength];
        Array.Copy(Bits, extended, Bits.Length);
        Array.Copy(Bits, 0, extended, Bits.Length, tailLength);
        return new(extended, BitCellTicks);
    }
    public static FluxBitstream FromIntervals(IReadOnlyList<uint> intervals, bool fm = false)
    {
        return Reconstruct(intervals, FluxTimingEstimator.EstimateBitCell(intervals, fm), 32);
    }
    public static FluxBitstream FromIntervalsPll(IReadOnlyList<uint> intervals, bool fm = false)
    {
        var centre = FluxTimingEstimator.EstimateBitCell(intervals, fm);
        return ReconstructPll(intervals, centre, 32);
    }
    public static FluxBitstream FromIntervalsPll(IReadOnlyList<uint> intervals, double bitCellTicks)
    {
        return ReconstructPll(intervals, Math.Max(1, bitCellTicks), 32);
    }
    public static FluxBitstream FromNrziIntervals(IReadOnlyList<uint> intervals)
    {
        return Reconstruct(intervals, FluxTimingEstimator.EstimateNrziBitCell(intervals), 64);
    }
    public static FluxBitstream FromNrziIntervals(IReadOnlyList<uint> intervals, double bitCellTicks)
    {
        return Reconstruct(intervals, Math.Max(1, bitCellTicks), 64, adaptClock: false);
    }
    public static FluxBitstream FromDoubledNrziIntervals(IReadOnlyList<uint> intervals, bool adaptClock = true)
    {
        return Reconstruct(intervals, FluxTimingEstimator.EstimateBitCell(intervals), 64, adaptClock);
    }
    public static FluxBitstream FromDoubledNrziIntervals(IReadOnlyList<uint> intervals, double bitCellTicks)
    {
        return Reconstruct(intervals, Math.Max(1, bitCellTicks), 64, adaptClock: false);
    }
    private static FluxBitstream Reconstruct(IReadOnlyList<uint> intervals, double initialCell, int maximumCells, bool adaptClock = true)
    {
        var currentCell = initialCell; var accumulatedCell = 0d; var samples = 0; var bits = new List<bool>(intervals.Count * 4);
        for (var index = 0; index < intervals.Count; index++)
        {
            var interval = intervals[index]; var cells = Math.Clamp((int)Math.Round(interval / currentCell), 1, maximumCells);
            for (var zero = 1; zero < cells; zero++) bits.Add(false); bits.Add(true);
            if (index == 0 || !adaptClock) continue;
            var observedCell = interval / (double)cells;
            if (observedCell >= currentCell * .7 && observedCell <= currentCell * 1.3) currentCell += (observedCell - currentCell) * .08;
            accumulatedCell += currentCell; samples++;
        }
        return new(bits.ToArray(), samples == 0 ? initialCell : accumulatedCell / samples);
    }
    private static FluxBitstream ReconstructPll(IReadOnlyList<uint> intervals, double centre, int maximumCells)
    {
        var clock = centre;
        var minimum = centre * .9;
        var maximum = centre * 1.1;
        var ticks = 0d;
        var accumulatedClock = 0d;
        var samples = 0;
        var bits = new List<bool>(intervals.Count * 4);

        foreach (var interval in intervals)
        {
            ticks += interval;
            if (ticks < clock / 2) continue;

            var zeros = 0;
            while (zeros < maximumCells - 1)
            {
                ticks -= clock;
                if (ticks < clock / 2) break;
                zeros++;
                bits.Add(false);
            }
            bits.Add(true);

            var correctedTicks = ticks * .4;
            if (zeros <= 3) clock += ticks * .05;
            else clock += (centre - clock) * .05;
            clock = Math.Clamp(clock, minimum, maximum);
            ticks = correctedTicks;
            accumulatedClock += clock;
            samples++;
        }

        return new(bits.ToArray(), samples == 0 ? centre : accumulatedClock / samples);
    }
    public bool Match(int offset, ushort pattern) { if (offset + 16 > Bits.Length) return false; for (var bit = 0; bit < 16; bit++) if (Bits[offset + bit] != ((pattern & (1 << (15 - bit))) != 0)) return false; return true; }
    public bool Match(int offset, uint pattern, int length) { if (length is < 1 or > 32 || offset + length > Bits.Length) return false; for (var bit = 0; bit < length; bit++) if (Bits[offset + bit] != ((pattern & (1u << (length - 1 - bit))) != 0)) return false; return true; }
    public bool MatchBytes(int offset, IReadOnlyList<byte> pattern) { if (offset + pattern.Count * 8 > Bits.Length) return false; for (var index = 0; index < pattern.Count; index++) for (var bit = 0; bit < 8; bit++) if (Bits[offset + index * 8 + bit] != ((pattern[index] & (1 << (7 - bit))) != 0)) return false; return true; }
    public byte DecodeMfmByte(int offset) { byte value = 0; for (var bit = 0; bit < 8 && offset + bit * 2 + 1 < Bits.Length; bit++) if (Bits[offset + bit * 2 + 1]) value |= (byte)(1 << (7 - bit)); return value; }
    public byte DecodeByte(int offset) { byte value = 0; for (var bit = 0; bit < 8 && offset + bit < Bits.Length; bit++) if (Bits[offset + bit]) value |= (byte)(1 << (7 - bit)); return value; }
    public byte DecodeFmByte32(int offset) { byte value = 0; for (var bit = 0; bit < 8 && offset + bit * 4 + 3 < Bits.Length; bit++) if (Bits[offset + bit * 4 + 3]) value |= (byte)(1 << (7 - bit)); return value; }
}
