using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format TYCOM FM.</summary>
public sealed class TycomFmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => TycomFmFormat.CodecId;

    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => TycomFmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var decodedBytes = new List<byte>();
        var pairedDataOffsets = new HashSet<int>();
        for (var offset = 0; offset + TycomFmFormat.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, TycomFmFormat.HeaderMark)) continue;
            var header = TryDecodeHeader(stream, offset);
            if (header is null)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, TycomFmFormat.MarkBitCount, FluxStructureDescriptions.Truncated(TycomFmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, null, "sector header")));
                offset += TycomFmFormat.MarkBitCount - 1;
                continue;
            }
            if (!header.CrcValid)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, TycomFmFormat.HeaderBitCount, $"{FluxStructureDescriptions.Identity(TycomFmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, header.Cylinder, TycomFmFormat.LogicalHead, header.Sector, 0, TycomFmFormat.HeaderAddressMark, null)}, {FluxStructureDescriptions.Integrity("header CRC", false)}"));
                offset += TycomFmFormat.MarkBitCount - 1;
                continue;
            }
            decodedBytes.Add(header.Cylinder);
            decodedBytes.Add(header.Sector);
            var dataMark = FindNextDataMark(stream, offset + TycomFmFormat.HeaderBitCount, TycomFmFormat.MaximumDataSearchDistanceBits);
            var data = dataMark is null ? null : TryDecodeData(stream, dataMark);
            if (dataMark is not null)
            {
                pairedDataOffsets.Add(dataMark.Offset);
                if (data is null) structures.Add(new(FluxStructureKind.FormatData, dataMark.Offset, TycomFmFormat.MarkBitCount, FluxStructureDescriptions.Truncated(TycomFmFormat.StructureDescriptionName, FluxStructureKind.FormatData, dataMark.Definition.Mark, "sector data")));
                else
                {
                    decodedBytes.AddRange(data.Payload);
                    structures.Add(new(FluxStructureKind.FormatData, dataMark.Offset, TycomFmFormat.DataBlockBitCount, $"{FluxStructureDescriptions.Identity(TycomFmFormat.StructureDescriptionName, FluxStructureKind.FormatData, header.Cylinder, TycomFmFormat.LogicalHead, header.Sector, TycomFmFormat.SectorSize, dataMark.Definition.Mark, null)}, {FluxStructureDescriptions.Integrity("CRC", data.CrcValid)}"));
                }
            }
            sectors.Add(new(header.Cylinder, TycomFmFormat.LogicalHead, header.Sector, TycomFmFormat.SectorSizeCode, TycomFmFormat.SectorSize, data?.CrcValid, offset, Data: data?.Payload));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, TycomFmFormat.HeaderBitCount, FluxStructureDescriptions.Complete(TycomFmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, header.Cylinder, TycomFmFormat.LogicalHead, header.Sector, TycomFmFormat.SectorSize, dataMark?.Definition.Mark, null, true, data?.CrcValid)));
            offset += TycomFmFormat.MarkBitCount - 1;
        }
        CollectUnpairedDataMarks(stream, pairedDataOffsets, structures);
        var ordered = structures.OrderBy(item => item.BitOffset).ToArray();
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, ordered.Length, TycomFmFormat.ConfidenceSectorWeight, TycomFmFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, ordered, decodedBytes, sectors);
    }

    /// <summary>Lit l'en-tête TYCOM et valide son CRC.</summary>
    internal static TycomFmHeader? TryDecodeHeader(FluxBitstream stream, int offset)
    {
        var bytes = TryDecodeFmBytes(stream, offset + TycomFmFormat.MarkBitCount, TycomFmFormat.HeaderDecodedByteCount);
        if (bytes is null) return null;
        var crcBytes = new[] { TycomFmFormat.HeaderAddressMark }.Concat(bytes).ToArray();
        var valid = Crc16Calculator.Compute(crcBytes, TycomFmFormat.CrcPolynomial, TycomFmFormat.CrcInitialValue) == 0;
        return new(offset, bytes[TycomFmFormat.HeaderCylinderOffset], bytes[TycomFmFormat.HeaderSectorOffset], valid, bytes);
    }

    /// <summary>Recherche la prochaine marque de données et s'arrête devant un nouvel en-tête.</summary>
    internal static TycomFmDataMark? FindNextDataMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - TycomFmFormat.MarkBitCount, start + maximumDistance);
        for (var offset = start; offset <= end; offset++)
        {
            if (FluxBitReader.MatchBytes(stream, offset, TycomFmFormat.HeaderMark)) return null;
            var definition = TycomFmFormat.DataMarks.FirstOrDefault(mark => FluxBitReader.MatchBytes(stream, offset, mark.Pattern));
            if (definition is not null) return new(offset, definition);
        }
        return null;
    }

    /// <summary>Lit la marque, la charge utile et le CRC d'un bloc de données.</summary>
    internal static TycomFmData? TryDecodeData(FluxBitstream stream, TycomFmDataMark mark)
    {
        var bytes = TryDecodeFmBytes(stream, mark.Offset, TycomFmFormat.DataBlockByteCount);
        if (bytes is null || bytes[0] != mark.Definition.Mark) return null;
        var payload = bytes.Skip(1).Take(TycomFmFormat.SectorSize).ToArray();
        var valid = Crc16Calculator.Compute(bytes, TycomFmFormat.CrcPolynomial, TycomFmFormat.CrcInitialValue) == 0;
        return new(mark, payload, bytes, valid);
    }

    /// <summary>Ajoute les marques de données qui n'ont été associées à aucun en-tête.</summary>
    internal static void CollectUnpairedDataMarks(FluxBitstream stream, IReadOnlySet<int> pairedOffsets, ICollection<FluxStructure> structures)
    {
        for (var offset = 0; offset + TycomFmFormat.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (pairedOffsets.Contains(offset)) continue;
            var definition = TycomFmFormat.DataMarks.FirstOrDefault(mark => FluxBitReader.MatchBytes(stream, offset, mark.Pattern));
            if (definition is null) continue;
            structures.Add(new(FluxStructureKind.FormatData, offset, TycomFmFormat.MarkBitCount, FluxStructureDescriptions.UnclassifiedMark(TycomFmFormat.StructureDescriptionName, FluxStructureKind.FormatData, definition.Mark, "data")));
            offset += TycomFmFormat.MarkBitCount - 1;
        }
    }

    /// <summary>Tente de décoder une suite d'octets FM double largeur.</summary>
    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * TycomFmFormat.EncodedByteBitCount > stream.Bits.Length) return null;
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeFmByte32(stream, offset + index * TycomFmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }
}
