using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Heathkit FM.</summary>
public sealed class HeathkitFmDecoder : IFluxDecoder
{
    private static readonly byte[] SectorMark = HeathkitFmFormat.SectorMark.ToArray();

    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => HeathkitFmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => HeathkitFmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveFm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        var pairedMarks = new HashSet<int>();
        for (var offset = 0; offset + HeathkitFmFormat.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (pairedMarks.Contains(offset)) continue;
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark)) continue;
            if (offset + HeathkitFmFormat.HeaderBitCount > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, HeathkitFmFormat.MarkBitCount, HeathkitFmDescriptions.TruncatedHeader()));
                offset += HeathkitFmFormat.MarkBitCount - 1;
                continue;
            }
            var header = TryDecodeHeader(stream, offset);
            if (header is null) continue;
            bytes.AddRange([header.Volume, header.Cylinder, header.Sector]);
            var dataOffset = FindNextMark(stream, offset + HeathkitFmFormat.HeaderBitCount, HeathkitFmFormat.MaximumDataSearchDistanceBits);
            var data = dataOffset < 0 ? null : TryDecodeData(stream, dataOffset);
            if (dataOffset >= 0) pairedMarks.Add(dataOffset);
            if (data is not null)
            {
                bytes.AddRange(data.Payload);
                structures.Add(new(FluxStructureKind.FormatData, dataOffset, HeathkitFmFormat.MarkBitCount + HeathkitFmFormat.DataBlockByteCount * HeathkitFmFormat.EncodedFmByteBitCount, HeathkitFmDescriptions.Data(header, data.ChecksumValid)));
            }
            bool? integrity = !header.ChecksumValid || data?.ChecksumValid == false ? false : data is null ? null : true;
            sectors.Add(new(header.Cylinder, HeathkitFmFormat.LogicalHead, header.Sector, HeathkitFmFormat.SectorSizeCode, HeathkitFmFormat.SectorSize, integrity, offset, SectorIntegrityKind.Checksum, data?.Payload));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, HeathkitFmFormat.HeaderBitCount, HeathkitFmDescriptions.Header(header, data?.ChecksumValid)));
            offset += HeathkitFmFormat.MarkBitCount - 1;
        }
        CollectUnpairedMarks(stream, pairedMarks, structures);
        var ordered = structures.OrderBy(item => item.BitOffset).ToArray();
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, ordered.Length, HeathkitFmFormat.ConfidenceSectorWeight, HeathkitFmFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, ordered, bytes, sectors);
    }

    /// <summary>Lit et valide les quatre octets suivant la marque d'en-tête.</summary>
    private static HeathkitHeader? TryDecodeHeader(FluxBitstream stream, int offset)
    {
        var decoded = TryDecodeFmBytes(stream, offset + HeathkitFmFormat.MarkBitCount, HeathkitFmFormat.HeaderByteCount);
        if (decoded is null) return null;
        var record = HeathkitFmCodec.DecodeRecord(decoded);
        return new(record.Payload[HeathkitFmFormat.HeaderVolumeOffset], record.Payload[HeathkitFmFormat.HeaderCylinderOffset], record.Payload[HeathkitFmFormat.HeaderSectorOffset], record.Valid);
    }

    /// <summary>Lit, inverse et valide les données suivant une marque.</summary>
    internal static HeathkitData? TryDecodeData(FluxBitstream stream, int offset)
    {
        var decoded = TryDecodeFmBytes(stream, offset + HeathkitFmFormat.MarkBitCount, HeathkitFmFormat.DataBlockByteCount);
        if (decoded is null) return null;
        var record = HeathkitFmCodec.DecodeRecord(decoded);
        return new(record.Payload, record.Valid);
    }

    /// <summary>Recherche la marque suivante dans la distance autorisée.</summary>
    private static int FindNextMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - HeathkitFmFormat.MarkBitCount, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, SectorMark)) return offset;
        return -1;
    }

    /// <summary>Collecte les marques qui n'ont pas été appariées comme données.</summary>
    private static void CollectUnpairedMarks(FluxBitstream stream, ISet<int> pairedMarks, ICollection<FluxStructure> structures)
    {
        for (var offset = 0; offset + HeathkitFmFormat.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark) || pairedMarks.Contains(offset) || structures.Any(item => item.BitOffset == offset)) continue;
            structures.Add(new(FluxStructureKind.FormatData, offset, HeathkitFmFormat.MarkBitCount, HeathkitFmDescriptions.UnpairedData()));
            offset += HeathkitFmFormat.MarkBitCount - 1;
        }
    }

    /// <summary>Tente de décoder une suite d'octets FM.</summary>
    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * HeathkitFmFormat.EncodedFmByteBitCount, out result[index])) return null;
        return result;
    }

    /// <summary>Regroupe la charge utile et son checksum.</summary>
    internal sealed record HeathkitData(byte[] Payload, bool ChecksumValid);
}
