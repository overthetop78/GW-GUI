using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Micropolis MFM.</summary>
public sealed class MicropolisMfmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => MicropolisMfmFormat.CodecId;

    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => MicropolisMfmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var decodedBytes = new List<byte>();
        for (var offset = 0; offset + MicropolisMfmFormat.SyncBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, MicropolisMfmFormat.Sync)) continue;
            var recordStart = offset + MicropolisMfmFormat.SyncZeroCount * MicropolisMfmFormat.EncodedByteBitCount;
            var record = TryDecodeRecord(stream, recordStart);
            if (record is null)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, MicropolisMfmFormat.SyncBitCount, FluxStructureDescriptions.Truncated(MicropolisMfmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, null, "sector")));
                offset += MicropolisMfmFormat.SyncBitCount - 1;
                continue;
            }
            decodedBytes.AddRange(record.Data);
            sectors.Add(new(record.Cylinder, MicropolisMfmFormat.LogicalHead, record.Sector, MicropolisMfmFormat.SectorSizeCode, MicropolisMfmFormat.SectorSize, record.ChecksumValid, offset, SectorIntegrityKind.Checksum, Data: record.Data));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, (MicropolisMfmFormat.SyncZeroCount + MicropolisMfmFormat.RecordByteCount) * MicropolisMfmFormat.EncodedByteBitCount, $"{FluxStructureDescriptions.Identity(MicropolisMfmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, record.Cylinder, MicropolisMfmFormat.LogicalHead, record.Sector, MicropolisMfmFormat.SectorSize, MicropolisMfmFormat.AddressMark, null)}, {FluxStructureDescriptions.Integrity("checksum", record.ChecksumValid)}"));
            offset += MicropolisMfmFormat.SyncBitCount - 1;
        }
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, MicropolisMfmFormat.ConfidenceSectorWeight, MicropolisMfmFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, decodedBytes, sectors);
    }

    /// <summary>Lit et valide un record Micropolis complet.</summary>
    internal static MicropolisMfmRecord? TryDecodeRecord(FluxBitstream stream, int offset)
    {
        var bytes = TryDecodeMfmBytes(stream, offset, MicropolisMfmFormat.RecordByteCount);
        return bytes is null ? null : MicropolisMfmRecord.Parse(bytes);
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * MicropolisMfmFormat.EncodedByteBitCount > stream.Bits.Length) return null;
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * MicropolisMfmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }
}
