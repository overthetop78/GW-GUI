using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format DEC RX02.</summary>
public sealed class DecRx02Decoder : IFluxDecoder
{
    private static readonly byte[] HeaderMark = DecRx02Format.HeaderMark.ToArray();

    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => DecRx02Format.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => DecRx02Format.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution de flux à décoder.</param>
    /// <returns>Résultat du décodage RX02.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        var classifiedData = new HashSet<int>();
        for (var offset = 0; offset + DecRx02Format.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, HeaderMark)) continue;
            if (offset + DecRx02Format.HeaderBitCount > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, DecRx02Format.MarkBitCount, DecRx02Descriptions.TruncatedHeader()));
                offset += DecRx02Format.MarkBitCount - 1;
                continue;
            }
            var header = TryDecodeHeader(stream, offset);
            if (header is null) continue;
            if (!header.CrcValid)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, DecRx02Format.HeaderBitCount, DecRx02Descriptions.Header(header, null, null)));
                offset += DecRx02Format.MarkBitCount - 1;
                continue;
            }
            bytes.AddRange([header.Cylinder, header.Head, header.Sector, header.SizeCode]);
            var match = FindNextDataMark(stream, offset + DecRx02Format.HeaderBitCount, DecRx02Format.MaximumDataSearchDistanceBits);
            if (match is not null && match.Definition.SizeCode != header.SizeCode)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, DecRx02Format.HeaderBitCount, DecRx02Descriptions.Header(header, match.Definition, false)));
                offset += DecRx02Format.MarkBitCount - 1;
                continue;
            }
            var data = match is null ? null : TryDecodeData(stream, match);
            if (match is not null && data is not null)
            {
                classifiedData.Add(match.Offset);
                bytes.AddRange(data.Payload);
                structures.Add(new(FluxStructureKind.FormatData, match.Offset, data.BitLength, DecRx02Descriptions.Data(header, match.Definition, data.CrcValid)));
            }
            sectors.Add(new(header.Cylinder, header.Head, header.Sector, header.SizeCode, match?.Definition.SectorSize ?? DecRx02Format.FmSectorByteCount, data?.CrcValid, offset, Data: data?.Payload));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, DecRx02Format.HeaderBitCount, DecRx02Descriptions.Header(header, match?.Definition, data?.CrcValid)));
            offset += DecRx02Format.MarkBitCount - 1;
        }
        CollectUnpairedDataMarks(stream, classifiedData, structures);
        var ordered = structures.OrderBy(structure => structure.BitOffset).ToArray();
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, ordered.Length, DecRx02Format.ConfidenceSectorWeight, DecRx02Format.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, ordered, bytes, sectors);
    }

    /// <summary>Décode et valide l'en-tête situé à la position indiquée.</summary>
    private static DecRx02Header? TryDecodeHeader(FluxBitstream stream, int offset)
    {
        var bytes = TryDecodeFmBytes(stream, offset + DecRx02Format.MarkBitCount, DecRx02Format.HeaderDecodedByteCount);
        if (bytes is null) return null;
        var crcBytes = new[] { DecRx02Format.HeaderAddressMark }.Concat(bytes).ToArray();
        var valid = Crc16Calculator.Compute(crcBytes, DecRx02Format.CrcPolynomial, DecRx02Format.CrcInitialValue) == 0;
        return new(bytes[DecRx02Format.HeaderCylinderOffset], bytes[DecRx02Format.HeaderHeadOffset], bytes[DecRx02Format.HeaderSectorOffset], bytes[DecRx02Format.HeaderSizeCodeOffset], valid);
    }

    /// <summary>Recherche la prochaine marque de données dans la distance autorisée.</summary>
    private static DecRx02DataMarkMatch? FindNextDataMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - DecRx02Format.MarkBitCount, start + maximumDistance);
        for (var offset = start; offset <= end; offset++)
        {
            foreach (var definition in DecRx02Format.DataMarks)
            {
                if (FluxBitReader.MatchBytes(stream, offset, definition.Pattern)) return new(offset, definition);
            }
        }
        return null;
    }

    /// <summary>Décode les données suivant une marque reconnue.</summary>
    private static DecRx02DecodedData? TryDecodeData(FluxBitstream stream, DecRx02DataMarkMatch match)
    {
        var count = match.Definition.SectorSize + DecRx02Format.CrcByteCount;
        var start = match.Offset + DecRx02Format.MarkBitCount;
        var decoded = match.Definition.Encoding == DecRx02DataEncoding.M2Fm ? TryDecodeM2FmData(stream, start, count) : TryDecodeFmData(stream, start, count);
        var bitLength = match.Definition.Encoding == DecRx02DataEncoding.M2Fm ? DecRx02Format.MarkBitCount + DecRx02Format.M2FmPhaseBitCount + count * DecRx02Format.EncodedMfmByteBitCount : DecRx02Format.MarkBitCount + count * DecRx02Format.EncodedFmByteBitCount;
        if (decoded is null) return null;
        var crc = Crc16Calculator.Compute(new[] { match.Definition.Mark }.Concat(decoded), DecRx02Format.CrcPolynomial, DecRx02Format.CrcInitialValue);
        return new(decoded.Take(match.Definition.SectorSize).ToArray(), crc == 0, bitLength);
    }

    /// <summary>Lit un bloc de données encodé en FM.</summary>
    private static byte[]? TryDecodeFmData(FluxBitstream stream, int start, int count) => TryDecodeFmBytes(stream, start, count);

    /// <summary>Lit un bloc de données encodé en M²FM.</summary>
    private static byte[]? TryDecodeM2FmData(FluxBitstream stream, int start, int count) => DecRx02M2FmCodec.Decode(stream, start + DecRx02Format.M2FmPhaseBitCount, count);

    /// <summary>Ajoute les marques de données qui n'ont pas été appariées à un en-tête.</summary>
    private static void CollectUnpairedDataMarks(FluxBitstream stream, ISet<int> classifiedData, ICollection<FluxStructure> structures)
    {
        for (var offset = 0; offset + DecRx02Format.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (classifiedData.Contains(offset)) continue;
            var definition = DecRx02Format.DataMarks.FirstOrDefault(candidate => FluxBitReader.MatchBytes(stream, offset, candidate.Pattern));
            if (definition is null) continue;
            structures.Add(new(FluxStructureKind.FormatData, offset, DecRx02Format.MarkBitCount, DecRx02Descriptions.UnpairedData(definition)));
            offset += DecRx02Format.MarkBitCount - 1;
        }
    }

    /// <summary>Tente de décoder une suite d'octets FM.</summary>
    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            if (!FluxBitReader.TryDecodeFmByte32(stream, offset + index * DecRx02Format.EncodedFmByteBitCount, out result[index])) return null;
        }
        return result;
    }

    /// <summary>Associe une position à sa définition de marque.</summary>
    private sealed record DecRx02DataMarkMatch(int Offset, DecRx02DataMarkDefinition Definition);
    /// <summary>Regroupe la charge utile, le CRC et la longueur d'un bloc décodé.</summary>
    private sealed record DecRx02DecodedData(byte[] Payload, bool CrcValid, int BitLength);
}
