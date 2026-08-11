using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format HP MMFM.</summary>
public sealed class HpMmfmDecoder : IFluxDecoder
{
    private static readonly byte[] SectorSync = HpMmfmFormat.SectorSync.ToArray();
    private static readonly byte[] DataSync = HpMmfmFormat.DataSync.ToArray();

    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => HpMmfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => HpMmfmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        var usedDataOffsets = new HashSet<int>();
        for (var offset = 0; offset + HpMmfmFormat.HeaderBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorSync)) continue;
            var header = TryDecodeHeader(stream, offset);
            if (header is null) continue;
            var dataOffset = FindDataSync(stream, offset);
            var data = dataOffset >= 0 && usedDataOffsets.Add(dataOffset) ? TryDecodeData(stream, dataOffset) : null;
            if (data is not null)
            {
                bytes.AddRange(data.Payload);
                structures.Add(new(FluxStructureKind.FormatData, dataOffset, HpMmfmFormat.SyncBitCount + HpMmfmFormat.EncodedDataByteCount * HpMmfmFormat.EncodedByteBitCount, HpMmfmDescriptions.Data(header, data.CrcValid)));
            }
            bool? integrity = !header.CrcValid || data?.CrcValid == false ? false : data is null ? null : true;
            sectors.Add(new(header.Cylinder, header.Head, header.Sector, HpMmfmFormat.SectorSizeCode, HpMmfmFormat.SectorSize, integrity, offset, Data: data?.Payload));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, HpMmfmFormat.HeaderBitCount, HpMmfmDescriptions.Header(header)));
            offset += HpMmfmFormat.SyncBitCount - 1;
        }
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, HpMmfmFormat.ConfidenceSectorWeight, HpMmfmFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Lit et valide les quatre octets de l'en-tête.</summary>
    internal static HpMmfmHeader? TryDecodeHeader(FluxBitstream stream, int offset)
    {
        var id = HpMmfmCodec.DecodeBytes(stream, offset + HpMmfmFormat.SyncBitCount, HpMmfmFormat.HeaderByteCount);
        if (id is null) return null;
        var cylinder = BitPrimitives.ReverseBits(id[HpMmfmFormat.HeaderCylinderOffset]);
        var encodedSector = BitPrimitives.ReverseBits(id[HpMmfmFormat.HeaderSectorOffset]);
        return new(cylinder, (byte)(encodedSector >> HpMmfmFormat.HeadShift), (byte)(encodedSector & HpMmfmFormat.SectorMask), Crc16Calculator.Compute(id) == 0);
    }

    /// <summary>Recherche la synchronisation de données dans les bornes du format.</summary>
    internal static int FindDataSync(FluxBitstream stream, int headerOffset)
    {
        var start = headerOffset + HpMmfmFormat.SyncBitCount + HpMmfmFormat.MinimumDataSearchOffsetBits;
        var end = Math.Min(stream.Bits.Length, headerOffset + HpMmfmFormat.SyncBitCount + HpMmfmFormat.MaximumDataSearchOffsetBits);
        for (var offset = start; offset + HpMmfmFormat.SyncBitCount <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, DataSync)) return offset;
        return -1;
    }

    /// <summary>Lit, valide et remet dans l'ordre logique un bloc de données.</summary>
    internal static HpMmfmData? TryDecodeData(FluxBitstream stream, int dataOffset)
    {
        var encoded = HpMmfmCodec.DecodeBytes(stream, dataOffset + HpMmfmFormat.SyncBitCount, HpMmfmFormat.EncodedDataByteCount);
        if (encoded is null) return null;
        return new(HpMmfmCodec.DecodePayload(encoded.Take(HpMmfmFormat.SectorSize).ToArray()), Crc16Calculator.Compute(encoded) == 0);
    }

    /// <summary>Regroupe la charge utile et l'état de son CRC.</summary>
    internal sealed record HpMmfmData(byte[] Payload, bool CrcValid);
}
