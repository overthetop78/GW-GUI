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
    public bool Match(int offset, ushort pattern) { if (offset + 16 > Bits.Length) return false; for (var bit = 0; bit < 16; bit++) if (Bits[offset + bit] != ((pattern & (1 << (15 - bit))) != 0)) return false; return true; }
    public bool Match(int offset, uint pattern, int length) { if (length is < 1 or > 32 || offset + length > Bits.Length) return false; for (var bit = 0; bit < length; bit++) if (Bits[offset + bit] != ((pattern & (1u << (length - 1 - bit))) != 0)) return false; return true; }
    public bool MatchBytes(int offset, IReadOnlyList<byte> pattern) { if (offset + pattern.Count * 8 > Bits.Length) return false; for (var index = 0; index < pattern.Count; index++) for (var bit = 0; bit < 8; bit++) if (Bits[offset + index * 8 + bit] != ((pattern[index] & (1 << (7 - bit))) != 0)) return false; return true; }
    public byte DecodeMfmByte(int offset) { byte value = 0; for (var bit = 0; bit < 8 && offset + bit * 2 + 1 < Bits.Length; bit++) if (Bits[offset + bit * 2 + 1]) value |= (byte)(1 << (7 - bit)); return value; }
    public byte DecodeByte(int offset) { byte value = 0; for (var bit = 0; bit < 8 && offset + bit < Bits.Length; bit++) if (Bits[offset + bit]) value |= (byte)(1 << (7 - bit)); return value; }
    public byte DecodeFmByte32(int offset) { byte value = 0; for (var bit = 0; bit < 8 && offset + bit * 4 + 3 < Bits.Length; bit++) if (Bits[offset + bit * 4 + 3]) value |= (byte)(1 << (7 - bit)); return value; }
}
