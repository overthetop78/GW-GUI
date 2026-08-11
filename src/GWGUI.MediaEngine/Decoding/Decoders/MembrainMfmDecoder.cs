using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Membrain MFM.</summary>
public sealed class MembrainMfmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => MembrainMfmFormat.CodecId;

    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => MembrainMfmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var decodedBytes = new List<byte>();
        var pairedDataOffsets = new HashSet<int>();
        for (var offset = 0; offset + MembrainMfmFormat.HeaderPatternBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, MembrainMfmFormat.HeaderPattern)) continue;
            var header = TryDecodeHeader(stream, offset);
            if (header is null)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, MembrainMfmFormat.HeaderPatternBitCount, FluxStructureDescriptions.Truncated(MembrainMfmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, null, "sector header")));
                offset += MembrainMfmFormat.HeaderPatternBitCount - 1;
                continue;
            }
            decodedBytes.AddRange(header.Bytes);
            var dataOffset = FindDataMark(stream, offset + MembrainMfmFormat.DataSearchInitialBitOffset, Math.Min(stream.Bits.Length, offset + MembrainMfmFormat.DataSearchBitCount));
            var data = dataOffset < 0 ? null : TryDecodeData(stream, dataOffset);
            if (dataOffset >= 0)
            {
                pairedDataOffsets.Add(dataOffset);
                if (data is null) structures.Add(new(FluxStructureKind.FormatData, dataOffset, MembrainMfmFormat.DataPatternBitCount, FluxStructureDescriptions.Truncated(MembrainMfmFormat.StructureDescriptionName, FluxStructureKind.FormatData, null, "CRC unavailable")));
                else
                {
                    decodedBytes.AddRange(data.Payload);
                    structures.Add(new(FluxStructureKind.FormatData, data.Offset, MembrainMfmFormat.DataBlockByteCount * MembrainMfmFormat.EncodedByteBitCount, $"{FluxStructureDescriptions.Identity(MembrainMfmFormat.StructureDescriptionName, FluxStructureKind.FormatData, header.Cylinder, header.Head, header.Sector, MembrainMfmFormat.SectorSize, data.Mark, "data block")}, {FluxStructureDescriptions.Integrity("CRC", data.CrcValid)}"));
                }
            }
            bool? integrity = !header.CrcValid || data?.CrcValid == false ? false : data is null ? null : true;
            sectors.Add(new(header.Cylinder, header.Head, header.Sector, MembrainMfmFormat.SectorSizeCode, MembrainMfmFormat.SectorSize, integrity, offset, Data: data?.Payload));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, MembrainMfmFormat.HeaderBitCount, FluxStructureDescriptions.Complete(MembrainMfmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, header.Cylinder, header.Head, header.Sector, MembrainMfmFormat.SectorSize, MembrainMfmFormat.HeaderAddressMark, null, header.CrcValid, data?.CrcValid)));
            var structureEnd = data is null ? offset + MembrainMfmFormat.HeaderBitCount : data.Offset + MembrainMfmFormat.DataBlockByteCount * MembrainMfmFormat.EncodedByteBitCount;
            offset = Math.Max(offset + MembrainMfmFormat.HeaderPatternBitCount - 1, structureEnd - 1);
        }
        CollectUnpairedDataMarks(stream, pairedDataOffsets, structures);
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, MembrainMfmFormat.ConfidenceSectorWeight, MembrainMfmFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, decodedBytes, sectors);
    }

    /// <summary>Lit et valide les six octets d'un en-tête Membrain.</summary>
    internal static MembrainMfmHeader? TryDecodeHeader(FluxBitstream stream, int offset)
    {
        var bytes = TryDecodeMfmBytes(stream, offset, MembrainMfmFormat.HeaderByteCount);
        if (bytes is null || bytes[MembrainMfmFormat.HeaderMarkOffset] != MembrainMfmFormat.HeaderAddressMark) return null;
        var address = MembrainMfmAddress.Unpack(bytes[MembrainMfmFormat.HeaderCylinderHighOffset], bytes[MembrainMfmFormat.HeaderPackedAddressOffset]);
        var valid = Crc16Calculator.Compute(bytes, MembrainMfmFormat.CrcPolynomial, MembrainMfmFormat.CrcInitialValue) == 0;
        return new(offset, address.Cylinder, address.Head, address.Sector, valid, bytes);
    }

    /// <summary>Recherche une marque de données Membrain dans la plage indiquée.</summary>
    internal static int FindDataMark(FluxBitstream stream, int start, int end)
    {
        for (var offset = Math.Max(0, start); offset + MembrainMfmFormat.DataPatternBitCount <= end; offset++)
        {
            if (!FluxBitReader.Match(stream, offset, MembrainMfmFormat.EncodedSyncByte)) continue;
            if (FluxBitReader.TryDecodeMfmByte(stream, offset + MembrainMfmFormat.EncodedByteBitCount, out var mark) && MembrainMfmFormat.IsDataAddressMark(mark)) return offset;
        }
        return -1;
    }

    /// <summary>Lit la charge utile et valide le CRC d'un bloc de données Membrain.</summary>
    internal static MembrainMfmData? TryDecodeData(FluxBitstream stream, int offset)
    {
        var bytes = TryDecodeMfmBytes(stream, offset, MembrainMfmFormat.DataBlockByteCount);
        if (bytes is null || !MembrainMfmFormat.IsDataAddressMark(bytes[1])) return null;
        var payload = bytes.Skip(MembrainMfmFormat.DataPrefixByteCount).Take(MembrainMfmFormat.SectorSize).ToArray();
        var valid = Crc16Calculator.Compute(bytes, MembrainMfmFormat.CrcPolynomial, MembrainMfmFormat.CrcInitialValue) == 0;
        return new(offset, bytes[1], payload, bytes, valid);
    }

    /// <summary>Ajoute les marques de données qui n'ont été associées à aucun en-tête.</summary>
    internal static void CollectUnpairedDataMarks(FluxBitstream stream, IReadOnlySet<int> pairedOffsets, ICollection<FluxStructure> structures)
    {
        for (var offset = 0; offset + MembrainMfmFormat.DataPatternBitCount <= stream.Bits.Length; offset++)
        {
            var found = FindDataMark(stream, offset, stream.Bits.Length);
            if (found < 0) return;
            if (!pairedOffsets.Contains(found)) structures.Add(new(FluxStructureKind.FormatData, found, MembrainMfmFormat.DataPatternBitCount, FluxStructureDescriptions.UnpairedData(MembrainMfmFormat.StructureDescriptionName, null, "data block")));
            offset = found + MembrainMfmFormat.DataPatternBitCount - 1;
        }
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * MembrainMfmFormat.EncodedByteBitCount > stream.Bits.Length) return null;
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * MembrainMfmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }
}
