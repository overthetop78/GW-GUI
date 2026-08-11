using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format E-mu FM.</summary>
public sealed class EmuFmDecoder : IFluxDecoder
{
    private static readonly byte[] SectorMark = EmuFmFormat.SectorMark.ToArray();

    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => EmuFmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => EmuFmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        var classifiedMarks = new HashSet<int>();
        for (var offset = 0; offset + EmuFmFormat.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark) || offset + EmuFmFormat.HeaderBitCount > stream.Bits.Length) continue;
            var identity = TryDecodeHeader(stream, offset);
            if (identity is null) continue;
            classifiedMarks.Add(offset);
            bytes.Add((byte)((identity.Cylinder << EmuFmFormat.TrackShift) | identity.Head));
            var dataOffset = FindNextMark(stream, offset + EmuFmFormat.HeaderBitCount, EmuFmFormat.MaximumDataSearchDistanceBits);
            var data = dataOffset < 0 ? null : TryDecodeData(stream, dataOffset);
            if (data is not null)
            {
                classifiedMarks.Add(dataOffset);
                bytes.AddRange(data.Payload);
                structures.Add(new(FluxStructureKind.FormatData, dataOffset, EmuFmFormat.MarkBitCount + EmuFmFormat.DataBlockByteCount * EmuFmFormat.EncodedFmByteBitCount, EmuFmDescriptions.Data(identity, data.CrcValid)));
            }
            sectors.Add(new(identity.Cylinder, identity.Head, EmuFmFormat.SectorNumber, EmuFmFormat.SectorSizeCode, EmuFmFormat.SectorSize, data?.CrcValid, offset, Data: data?.Payload));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, EmuFmFormat.HeaderBitCount, EmuFmDescriptions.Header(identity, data?.CrcValid)));
            offset += EmuFmFormat.MarkBitCount - 1;
        }
        CollectUnclassifiedMarks(stream, classifiedMarks, structures);
        var ordered = structures.OrderBy(item => item.BitOffset).ToArray();
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, ordered.Length, EmuFmFormat.ConfidenceSectorWeight, EmuFmFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, ordered, bytes, sectors);
    }

    /// <summary>Décode la piste brute, valide son CRC et sépare le cylindre de la face.</summary>
    private static EmuTrackIdentity? TryDecodeHeader(FluxBitstream stream, int offset)
    {
        var header = TryDecodeFmBytes(stream, offset + EmuFmFormat.MarkBitCount, EmuFmFormat.HeaderDecodedByteCount);
        if (header is null || Crc16Calculator.Compute(header, EmuFmFormat.CrcPolynomial, EmuFmFormat.CrcInitialValue) != 0) return null;
        var track = BitPrimitives.ReverseBits(header[EmuFmFormat.HeaderRawTrackOffset]);
        return new((byte)(track >> EmuFmFormat.TrackShift), (byte)(track & EmuFmFormat.HeadMask));
    }

    /// <summary>Lit la charge utile et les deux octets de CRC.</summary>
    internal static EmuDecodedData? TryDecodeData(FluxBitstream stream, int offset)
    {
        var block = TryDecodeFmBytes(stream, offset + EmuFmFormat.MarkBitCount, EmuFmFormat.DataBlockByteCount);
        if (block is null) return null;
        var payload = block.Take(EmuFmFormat.PayloadByteCount).ToArray();
        var valid = Crc16Calculator.Compute(block, EmuFmFormat.CrcPolynomial, EmuFmFormat.CrcInitialValue) == 0;
        return new(payload, valid);
    }

    /// <summary>Recherche la prochaine marque après l'en-tête complet.</summary>
    private static int FindNextMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - EmuFmFormat.MarkBitCount, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, SectorMark)) return offset;
        return -1;
    }

    /// <summary>Collecte les marques qui ne sont associées à aucun en-tête ni bloc de données.</summary>
    private static void CollectUnclassifiedMarks(FluxBitstream stream, ISet<int> classifiedMarks, ICollection<FluxStructure> structures)
    {
        for (var offset = 0; offset + EmuFmFormat.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!classifiedMarks.Contains(offset) && FluxBitReader.MatchBytes(stream, offset, SectorMark)) structures.Add(new(FluxStructureKind.FormatHeader, offset, EmuFmFormat.MarkBitCount, EmuFmDescriptions.UnclassifiedMark()));
        }
    }

    /// <summary>Tente de décoder une suite d'octets FM.</summary>
    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeFmByte32(stream, offset + index * EmuFmFormat.EncodedFmByteBitCount, out result[index])) return null;
        return result;
    }

    /// <summary>Regroupe la charge utile et l'état de son CRC.</summary>
    internal sealed record EmuDecodedData(byte[] Payload, bool CrcValid);
}
