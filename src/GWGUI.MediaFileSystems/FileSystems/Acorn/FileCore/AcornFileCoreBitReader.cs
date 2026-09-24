using GWGUI.MediaFileSystems.Constants;
using System.Buffers.Binary;
using System.Numerics;
using GWGUI.MediaFileSystems.Primitives;

namespace GWGUI.MediaFileSystems.FileSystems.Acorn.FileCore;

/// <summary>Lit des champs et recherche des bits dans une carte FileCore.</summary>
public static class AcornFileCoreBitReader
{
    /// <summary>Lit un champ masqué à un offset de bit arbitraire.</summary>
    public static uint GetBits(ReadOnlySpan<byte> data, int bitOffset, uint mask)
    {
        var capacity = checked(data.Length * BitConstants.BitsPerByte);
        var bitLength = mask == 0 ? 1 : BitOperations.Log2(mask) + 1;
        var requiredLength = checked((bitOffset & AcornFileCoreLayout.IntraByteBitMask) + bitLength);
        if (bitOffset < 0 || bitOffset + bitLength > capacity) throw AcornFileCoreExceptions.InvalidBitRange(bitOffset, bitLength, capacity);
        var byteOffset = bitOffset / BitConstants.BitsPerByte;
        var requiredBytes = (requiredLength + AcornFileCoreLayout.IntraByteBitMask) / BitConstants.BitsPerByte;
        Span<byte> value = stackalloc byte[AcornFileCoreLayout.BitWindowByteLength];
        data.Slice(byteOffset, requiredBytes).CopyTo(value);
        return (BinaryPrimitives.ReadUInt32LittleEndian(value) >> (bitOffset & AcornFileCoreLayout.IntraByteBitMask)) & mask;
    }

    /// <summary>Recherche le prochain bit positionné dans une plage.</summary>
    public static int FindNextSetBit(ReadOnlySpan<byte> data, int start, int end)
    {
        var capacity = checked(data.Length * BitConstants.BitsPerByte);
        if (start < 0 || end < start || end > capacity) throw AcornFileCoreExceptions.InvalidBitRange(start, end - start, capacity);
        for (var bit = start; bit < end; bit++) if ((data[bit / BitConstants.BitsPerByte] & (1 << (bit & AcornFileCoreLayout.IntraByteBitMask))) != 0) return bit;
        return end;
    }
}
