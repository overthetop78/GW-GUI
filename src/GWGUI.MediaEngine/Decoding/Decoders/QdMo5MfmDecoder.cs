using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format QD MO5 MFM.</summary>
public sealed class QdMo5MfmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => QdMo5MfmFormat.CodecId;

    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => QdMo5MfmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var decodedBytes = new List<byte>();
        var pairedDataOffsets = new HashSet<int>();
        for (var offset = 0; offset + QdMo5MfmFormat.PhysicalMarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, QdMo5MfmFormat.HeaderMark)) continue;
            var header = TryDecodeHeader(stream, offset);
            if (header is null)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, QdMo5MfmFormat.PhysicalMarkBitCount, FluxStructureDescriptions.Truncated(QdMo5MfmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, null, "sector header")));
                offset += QdMo5MfmFormat.PhysicalMarkBitCount - 1;
                continue;
            }
            decodedBytes.Add((byte)(header.Sector >> BitPrimitives.BitsPerByte));
            decodedBytes.Add((byte)header.Sector);
            var dataOffset = FindNextData(stream, offset + QdMo5MfmFormat.HeaderBitCount, QdMo5MfmFormat.DataSearchBitCount);
            var data = dataOffset < 0 ? null : TryDecodeData(stream, dataOffset);
            if (dataOffset >= 0)
            {
                pairedDataOffsets.Add(dataOffset);
                if (data is null) structures.Add(new(FluxStructureKind.FormatData, dataOffset, QdMo5MfmFormat.PhysicalMarkBitCount, FluxStructureDescriptions.Truncated(QdMo5MfmFormat.StructureDescriptionName, FluxStructureKind.FormatData, null, "sector data, checksum unavailable")));
                else
                {
                    decodedBytes.AddRange(data.Payload);
                    structures.Add(new(FluxStructureKind.FormatData, dataOffset, QdMo5MfmFormat.DataBlockBitCount, $"{FluxStructureDescriptions.Identity(QdMo5MfmFormat.StructureDescriptionName, FluxStructureKind.FormatData, QdMo5MfmFormat.LogicalCylinder, QdMo5MfmFormat.LogicalHead, header.Sector, QdMo5MfmFormat.SectorSize, data.Prefix, null)}, {FluxStructureDescriptions.Integrity("checksum", data.ChecksumValid)}"));
                }
            }
            sectors.Add(new(QdMo5MfmFormat.LogicalCylinder, QdMo5MfmFormat.LogicalHead, header.Sector, QdMo5MfmFormat.SectorSizeCode, QdMo5MfmFormat.SectorSize, data?.ChecksumValid, offset, SectorIntegrityKind.Checksum, Data: data?.Payload));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, QdMo5MfmFormat.HeaderBitCount, FluxStructureDescriptions.Complete(QdMo5MfmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, QdMo5MfmFormat.LogicalCylinder, QdMo5MfmFormat.LogicalHead, header.Sector, QdMo5MfmFormat.SectorSize, QdMo5MfmFormat.HeaderAddressMark, null, true, data?.ChecksumValid, "header", "data checksum")));
            offset += QdMo5MfmFormat.PhysicalMarkBitCount - 1;
        }
        CollectUnpairedDataMarks(stream, pairedDataOffsets, structures);
        var ordered = structures.OrderBy(item => item.BitOffset).ToArray();
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, ordered.Length, QdMo5MfmFormat.ConfidenceSectorWeight, QdMo5MfmFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, ordered, decodedBytes, sectors);
    }

    /// <summary>Lit le numéro de secteur et les treize octets réservés de l'en-tête.</summary>
    internal static QdMo5MfmHeader? TryDecodeHeader(FluxBitstream stream, int offset)
    {
        var bytes = TryDecodeMfmBytes(stream, offset + QdMo5MfmFormat.PhysicalMarkBitCount, QdMo5MfmFormat.SectorNumberByteCount + QdMo5MfmFormat.HeaderPaddingByteCount);
        if (bytes is null) return null;
        var sector = bytes[0] << BitPrimitives.BitsPerByte | bytes[1];
        return new(offset, sector, bytes.Skip(QdMo5MfmFormat.SectorNumberByteCount).ToArray());
    }

    /// <summary>Recherche les données jusqu'à la limite ou jusqu'au prochain en-tête.</summary>
    internal static int FindNextData(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - QdMo5MfmFormat.PhysicalMarkBitCount, start + maximumDistance);
        for (var offset = start; offset <= end; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, QdMo5MfmFormat.Preamble)) continue;
            if (!FluxBitReader.TryDecodeMfmByte(stream, offset + QdMo5MfmFormat.PreambleBitCount, out var value)) continue;
            if (value == QdMo5MfmFormat.HeaderAddressMark) return -1;
            return offset;
        }
        return -1;
    }

    /// <summary>Lit le préfixe, la charge utile et le checksum stocké.</summary>
    internal static QdMo5MfmData? TryDecodeData(FluxBitstream stream, int offset)
    {
        var bytes = TryDecodeMfmBytes(stream, offset + QdMo5MfmFormat.PreambleBitCount, QdMo5MfmFormat.DataBytesAfterPreamble);
        if (bytes is null) return null;
        var prefix = bytes[0];
        var payload = bytes.Skip(QdMo5MfmFormat.DataPrefixByteCount).Take(QdMo5MfmFormat.SectorSize).ToArray();
        var storedChecksum = bytes[^1];
        return new(offset, prefix, payload, storedChecksum, QdMo5Checksum.Compute(prefix, payload) == storedChecksum);
    }

    /// <summary>Ajoute les marques de données qui n'ont été associées à aucun en-tête.</summary>
    internal static void CollectUnpairedDataMarks(FluxBitstream stream, IReadOnlySet<int> pairedOffsets, ICollection<FluxStructure> structures)
    {
        for (var offset = 0; offset + QdMo5MfmFormat.PhysicalMarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, QdMo5MfmFormat.Preamble)) continue;
            if (!FluxBitReader.TryDecodeMfmByte(stream, offset + QdMo5MfmFormat.PreambleBitCount, out var value) || value == QdMo5MfmFormat.HeaderAddressMark || pairedOffsets.Contains(offset)) continue;
            structures.Add(new(FluxStructureKind.FormatData, offset, QdMo5MfmFormat.PhysicalMarkBitCount, FluxStructureDescriptions.UnpairedData(QdMo5MfmFormat.StructureDescriptionName, value, "sector data")));
            offset += QdMo5MfmFormat.PhysicalMarkBitCount - 1;
        }
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * QdMo5MfmFormat.EncodedByteBitCount > stream.Bits.Length) return null;
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * QdMo5MfmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }
}
