namespace GWGUI.Scp.Decoding;

internal sealed class FluxBitstream(bool[] bits, double bitCellTicks)
{
    public bool[] Bits { get; } = bits; public double BitCellTicks { get; } = bitCellTicks;
    public static FluxBitstream FromIntervals(IReadOnlyList<uint> intervals, bool fm = false)
    {
        return Reconstruct(intervals, EstimateBitCell(intervals, fm), 32);
    }
    public static FluxBitstream FromNrziIntervals(IReadOnlyList<uint> intervals)
    {
        return Reconstruct(intervals, EstimateNrziBitCell(intervals), 64);
    }
    public static FluxBitstream FromDoubledNrziIntervals(IReadOnlyList<uint> intervals)
    {
        return Reconstruct(intervals, EstimateBitCell(intervals), 64);
    }
    private static FluxBitstream Reconstruct(IReadOnlyList<uint> intervals, double initialCell, int maximumCells)
    {
        var currentCell = initialCell; var accumulatedCell = 0d; var samples = 0; var bits = new List<bool>(intervals.Count * 4);
        for (var index = 0; index < intervals.Count; index++)
        {
            var interval = intervals[index]; var cells = Math.Clamp((int)Math.Round(interval / currentCell), 1, maximumCells);
            for (var zero = 1; zero < cells; zero++) bits.Add(false); bits.Add(true);
            if (index == 0) continue;
            var observedCell = interval / (double)cells;
            if (observedCell >= currentCell * .7 && observedCell <= currentCell * 1.3) currentCell += (observedCell - currentCell) * .08;
            accumulatedCell += currentCell; samples++;
        }
        return new(bits.ToArray(), samples == 0 ? initialCell : accumulatedCell / samples);
    }
    public static double EstimateBitCell(IReadOnlyList<uint> intervals, bool fm = false)
    {
        if (intervals.Count == 0) return 1;
        // The first interval starts at the index pulse rather than at a previous flux transition,
        // so it is not a complete cell-spacing sample and must not drive the PLL estimate.
        var samples = fm ? intervals : intervals.Skip(1);
        var sorted = samples.Where(x => x > 0).Order().ToArray(); if (sorted.Length == 0) sorted = intervals.Where(x => x > 0).Order().ToArray(); if (sorted.Length == 0) return 1;
        var sampleLength = Math.Max(1, sorted.Length / 5); var lowerCluster = sorted.Take(sampleLength).ToArray(); var robustLower = lowerCluster[lowerCluster.Length / 2];
        return Math.Max(1, fm ? robustLower : robustLower / 2d);
    }
    private static double EstimateNrziBitCell(IReadOnlyList<uint> intervals)
    {
        if (intervals.Count == 0) return 1;
        var sorted = intervals.Skip(1).Where(value => value > 0).Order().ToArray();
        if (sorted.Length == 0) sorted = intervals.Where(value => value > 0).Order().ToArray();
        if (sorted.Length == 0) return 1;
        // In GCR, one-cell transitions may represent less than ten percent of a track.
        // Taking the median of the whole lower quintile can therefore lock onto two cells.
        // A low, but non-minimum, percentile stays inside the shortest genuine timing cluster
        // while ignoring isolated capture glitches.
        var percentile = Math.Clamp(sorted.Length / 50, 0, sorted.Length - 1);
        return Math.Max(1, sorted[percentile]);
    }
    public bool Match(int offset, ushort pattern) { if (offset + 16 > Bits.Length) return false; for (var bit = 0; bit < 16; bit++) if (Bits[offset + bit] != ((pattern & (1 << (15 - bit))) != 0)) return false; return true; }
    public bool Match(int offset, uint pattern, int length) { if (length is < 1 or > 32 || offset + length > Bits.Length) return false; for (var bit = 0; bit < length; bit++) if (Bits[offset + bit] != ((pattern & (1u << (length - 1 - bit))) != 0)) return false; return true; }
    public bool MatchBytes(int offset, IReadOnlyList<byte> pattern) { if (offset + pattern.Count * 8 > Bits.Length) return false; for (var index = 0; index < pattern.Count; index++) for (var bit = 0; bit < 8; bit++) if (Bits[offset + index * 8 + bit] != ((pattern[index] & (1 << (7 - bit))) != 0)) return false; return true; }
    public byte DecodeMfmByte(int offset) { byte value = 0; for (var bit = 0; bit < 8 && offset + bit * 2 + 1 < Bits.Length; bit++) if (Bits[offset + bit * 2 + 1]) value |= (byte)(1 << (7 - bit)); return value; }
    public byte DecodeFmByte32(int offset) { byte value = 0; for (var bit = 0; bit < 8 && offset + bit * 4 + 3 < Bits.Length; bit++) if (Bits[offset + bit * 4 + 3]) value |= (byte)(1 << (7 - bit)); return value; }
}
