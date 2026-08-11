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
}
