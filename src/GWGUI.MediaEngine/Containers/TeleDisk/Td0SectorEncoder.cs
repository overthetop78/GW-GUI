using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Choisit sans perte l'encodage sectoriel TeleDisk le plus compact pris en charge.</summary>
internal static class Td0SectorEncoder
{
    /// <summary>Encode les données brutes ou sous forme d'un mot répété.</summary>
    public static Td0EncodedSector Encode(IReadOnlyList<byte> data)
    {
        var raw = data.ToArray();
        if (raw.Length < Td0Layout.WordSize || (raw.Length & 1) != 0) return new(Td0SectorEncoding.Raw, raw);
        for (var offset = Td0Layout.WordSize; offset < raw.Length; offset += Td0Layout.WordSize)
        {
            if (raw[offset] != raw[0] || raw[offset + 1] != raw[1]) return new(Td0SectorEncoding.Raw, raw);
        }
        var repeated = new byte[Td0Layout.RepeatedSectorPayloadSize];
        BinaryPrimitives.WriteUInt16LittleEndian(repeated, checked((ushort)(raw.Length / Td0Layout.WordSize)));
        repeated[Td0Layout.RepeatedSectorPatternOffset] = raw[0];
        repeated[Td0Layout.RepeatedSectorSecondPatternByteOffset] = raw[1];
        return new(Td0SectorEncoding.RepeatedWord, repeated);
    }
}
