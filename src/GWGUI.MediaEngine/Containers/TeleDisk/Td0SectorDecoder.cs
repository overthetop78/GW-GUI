using System.Buffers.Binary;

namespace GWGUI.MediaEngine.Containers.TeleDisk;

/// <summary>Décode les trois représentations de charge utile sectorielle TeleDisk.</summary>
internal static class Td0SectorDecoder
{
    /// <summary>Décode une charge utile sectorielle en contrôlant sa longueur finale.</summary>
    /// <param name="encoded">Charge utile encodée.</param>
    /// <param name="encoding">Encodage TeleDisk appliqué.</param>
    /// <param name="expectedLength">Longueur décodée attendue, en octets.</param>
    /// <param name="cylinder">Cylindre utilisé dans les diagnostics.</param>
    /// <param name="head">Face utilisée dans les diagnostics.</param>
    /// <param name="sector">Numéro de secteur utilisé dans les diagnostics.</param>
    /// <returns>Données sectorielles décodées.</returns>
    /// <exception cref="InvalidDataException">L'encodage est inconnu, tronqué ou produit une longueur incorrecte.</exception>
    public static byte[] Decode(ReadOnlySpan<byte> encoded, Td0SectorEncoding encoding, int expectedLength, int cylinder, int head, int sector)
    {
        var output = new List<byte>(expectedLength);
        switch (encoding)
        {
            case Td0SectorEncoding.Raw:
                output.AddRange(encoded.ToArray());
                break;
            case Td0SectorEncoding.RepeatedWord:
                if (encoded.Length != Td0Layout.RepeatedSectorPayloadSize) throw Td0Exceptions.InvalidRepeatedPayload(cylinder, head, sector, encoded.Length, Td0Layout.RepeatedSectorPayloadSize);
                var repetitions = ReadUInt16(encoded, Td0Layout.RepeatedSectorCountOffset);
                for (var index = 0; index < repetitions; index++)
                {
                    output.Add(encoded[Td0Layout.RepeatedSectorPatternOffset]);
                    output.Add(encoded[Td0Layout.RepeatedSectorSecondPatternByteOffset]);
                }
                break;
            case Td0SectorEncoding.Rle:
                for (var offset = 0; offset < encoded.Length;)
                {
                    if (offset + Td0Layout.RleControlSize > encoded.Length) throw Td0Exceptions.TruncatedEncoding(cylinder, head, sector, encoding, offset, Td0Layout.RleControlSize, encoded.Length - offset);
                    var patternWords = encoded[offset++];
                    var count = encoded[offset++];
                    if (patternWords == 0)
                    {
                        if (offset + count > encoded.Length) throw Td0Exceptions.TruncatedEncoding(cylinder, head, sector, encoding, offset, count, encoded.Length - offset);
                        output.AddRange(encoded.Slice(offset, count).ToArray());
                        offset += count;
                    }
                    else
                    {
                        var patternLength = patternWords * Td0Layout.PatternWordSize;
                        if (offset + patternLength > encoded.Length) throw Td0Exceptions.TruncatedEncoding(cylinder, head, sector, encoding, offset, patternLength, encoded.Length - offset);
                        var pattern = encoded.Slice(offset, patternLength).ToArray();
                        offset += patternLength;
                        for (var repeat = 0; repeat < count; repeat++) output.AddRange(pattern);
                    }
                }
                break;
            default:
                throw Td0Exceptions.UnsupportedEncoding(cylinder, head, sector, encoding);
        }

        if (output.Count != expectedLength) throw Td0Exceptions.InvalidDecodedLength(cylinder, head, sector, encoding, output.Count, expectedLength);
        return output.ToArray();
    }

    /// <summary>Lit un entier non signé 16 bits little-endian dans une charge utile.</summary>
    /// <param name="data">Données contenant l'entier.</param>
    /// <param name="offset">Position de l'entier, en octets.</param>
    /// <returns>Valeur entière lue.</returns>
    private static ushort ReadUInt16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
}
