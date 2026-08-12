using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Victor 9000 GCR.</summary>
public sealed class Victor9kGcrDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => Victor9kGcrFormat.CodecId;

    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => Victor9kGcrFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveDoubledNrzi(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var decodedBytes = new List<byte>();
        var pairedDataOffsets = new HashSet<int>();
        for (var offset = 0; offset + Victor9kGcrFormat.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, Victor9kGcrFormat.HeaderMark)) continue;
            var header = TryDecodeHeader(stream.Bits, offset);
            if (header is not null) decodedBytes.AddRange(header.Bytes);
            var headerEnd = offset + Victor9kGcrFormat.EncodedDataStartBitOffset + Victor9kGcrFormat.HeaderByteCount * CommodoreGcrCodec.EncodedByteBitCount * Victor9kGcrFormat.EncodedCellStride;
            var dataOffset = FindDataMark(stream, headerEnd, Math.Min(stream.Bits.Length, offset + Victor9kGcrFormat.MaximumDataSearchDistanceBits));
            var data = dataOffset < 0 ? null : TryDecodeData(stream.Bits, dataOffset);
            if (dataOffset >= 0)
            {
                pairedDataOffsets.Add(dataOffset);
                if (data is null) structures.Add(new(FluxStructureKind.FormatData, dataOffset, Victor9kGcrFormat.MarkBitCount, FluxStructureDescriptions.Truncated(Victor9kGcrFormat.StructureDescriptionName, FluxStructureKind.FormatData, null, "checksum unavailable")));
                else
                {
                    decodedBytes.AddRange(data.Payload);
                    structures.Add(new(FluxStructureKind.FormatData, dataOffset, data.EndOffset - dataOffset, $"{FluxStructureDescriptions.Identity(Victor9kGcrFormat.StructureDescriptionName, FluxStructureKind.FormatData, header?.Cylinder ?? 0, Victor9kGcrFormat.LogicalHead, header?.Sector ?? 0, Victor9kGcrFormat.SectorByteCount, data.Prefix, "data block")}, {FluxStructureDescriptions.Integrity("checksum", data.ChecksumValid)}"));
                }
            }
            bool? integrity = header?.Valid == false || data?.ChecksumValid == false ? false : data is null ? null : true;
            sectors.Add(new(header?.Cylinder ?? 0, Victor9kGcrFormat.LogicalHead, header?.Sector ?? 0, Victor9kGcrFormat.SectorSizeCode, Victor9kGcrFormat.SectorByteCount, integrity, offset, SectorIntegrityKind.Checksum, Data: data?.Payload));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, Math.Max(Victor9kGcrFormat.MarkBitCount, headerEnd - offset), FluxStructureDescriptions.Complete(Victor9kGcrFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, header?.Cylinder ?? 0, Victor9kGcrFormat.LogicalHead, header?.Sector ?? 0, Victor9kGcrFormat.SectorByteCount, null, null, header?.Valid, data?.ChecksumValid, "header", "data checksum")));
            offset = Math.Max(offset + Victor9kGcrFormat.MarkBitCount - 1, (data?.EndOffset ?? headerEnd) - 1);
        }
        CollectUnpairedDataMarks(stream, pairedDataOffsets, structures);
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, Victor9kGcrFormat.ConfidenceSectorWeight, Victor9kGcrFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, decodedBytes, sectors);
    }

    /// <summary>Décode et valide les six octets d'un en-tête Victor 9000.</summary>
    internal static Victor9kHeader? TryDecodeHeader(IReadOnlyList<bool> bits, int markOffset)
    {
        var bytes = CommodoreGcrCodec.TryDecodeBytes(bits, markOffset + Victor9kGcrFormat.EncodedDataStartBitOffset, Victor9kGcrFormat.HeaderByteCount, Victor9kGcrFormat.EncodedCellStride, out _);
        if (bytes is null) return null;
        var valid = bytes[Victor9kGcrFormat.HeaderTypeOffset] == Victor9kGcrFormat.HeaderType && bytes[Victor9kGcrFormat.HeaderId2Offset] == Victor9kGcrFormat.HeaderId2 && bytes[Victor9kGcrFormat.HeaderId1Offset] == Victor9kGcrFormat.HeaderId1 && bytes[Victor9kGcrFormat.HeaderSumOffset] == (byte)(bytes[Victor9kGcrFormat.HeaderCylinderOffset] + bytes[Victor9kGcrFormat.HeaderSectorOffset]);
        return new(bytes[Victor9kGcrFormat.HeaderCylinderOffset], bytes[Victor9kGcrFormat.HeaderSectorOffset], bytes, valid);
    }

    /// <summary>Recherche la marque de données dans la plage fournie.</summary>
    internal static int FindDataMark(FluxBitstream stream, int start, int end)
    {
        for (var offset = Math.Max(0, start); offset + Victor9kGcrFormat.MarkBitCount <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, Victor9kGcrFormat.DataMark)) return offset;
        return -1;
    }

    /// <summary>Lit le préfixe, la charge utile et le checksum faible puis fort.</summary>
    internal static Victor9kData? TryDecodeData(IReadOnlyList<bool> bits, int markOffset)
    {
        var bytes = CommodoreGcrCodec.TryDecodeBytes(bits, markOffset + Victor9kGcrFormat.EncodedDataStartBitOffset, Victor9kGcrFormat.DecodedDataByteCount, Victor9kGcrFormat.EncodedCellStride, out var endOffset);
        if (bytes is null) return null;
        var payload = bytes.Skip(Victor9kGcrFormat.DataOffset).Take(Victor9kGcrFormat.SectorByteCount).ToArray();
        var storedChecksum = (ushort)(bytes[Victor9kGcrFormat.ChecksumLowOffset] | bytes[Victor9kGcrFormat.ChecksumHighOffset] << BitPrimitives.BitsPerByte);
        return new(bytes[Victor9kGcrFormat.DataPrefixOffset], payload, storedChecksum, Victor9kChecksum.Compute(payload) == storedChecksum, endOffset);
    }

    /// <summary>Ajoute les marques de données qui n'ont été associées à aucun en-tête.</summary>
    internal static void CollectUnpairedDataMarks(FluxBitstream stream, IReadOnlySet<int> pairedOffsets, ICollection<FluxStructure> structures)
    {
        for (var offset = 0; offset + Victor9kGcrFormat.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, Victor9kGcrFormat.DataMark) || pairedOffsets.Contains(offset)) continue;
            structures.Add(new(FluxStructureKind.FormatData, offset, Victor9kGcrFormat.MarkBitCount, FluxStructureDescriptions.UnpairedData(Victor9kGcrFormat.StructureDescriptionName, null, "data block")));
            offset += Victor9kGcrFormat.MarkBitCount - 1;
        }
    }
}
