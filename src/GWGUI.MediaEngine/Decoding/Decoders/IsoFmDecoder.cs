using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format ISO FM.</summary>
public sealed class IsoFmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => IsoFmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => IsoFmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveFm(revolution.FluxIntervals);
        CollectMarks(stream, out var headers, out var dataMarks);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var byteSegments = new List<(int Offset, byte[] Bytes)>();
        var pairedData = new HashSet<IsoFmDataMark>();
        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index];
            byteSegments.Add((header.Offset, new[] { IsoFmFormat.IdAddressMark }.Concat(header.Bytes ?? []).ToArray()));
            var nextHeaderOffset = index + 1 < headers.Count ? headers[index + 1].Offset : int.MaxValue;
            var dataMark = SelectDataMark(header, nextHeaderOffset, dataMarks);
            var data = dataMark is null || header.Size == 0 ? null : TryDecodeData(stream, dataMark, header.Size);
            if (dataMark is not null)
            {
                pairedData.Add(dataMark);
                byteSegments.Add((dataMark.Offset, new[] { dataMark.Definition.Mark }.Concat(data?.Bytes ?? []).ToArray()));
                structures.Add(new(dataMark.Definition.Kind, dataMark.Offset, data is null ? IsoFmFormat.EncodedMarkBitCount : IsoFmFormat.EncodedMarkBitCount + data.Bytes.Length * IsoFmFormat.EncodedByteBitCount, IsoFmDescriptions.Data(header, dataMark.Definition, data?.CrcValid)));
            }
            bool? integrity = header.CrcValid == false || data?.CrcValid == false ? false : data is null ? null : true;
            sectors.Add(new(header.Cylinder, header.Head, header.Sector, header.SizeCode, header.Size, integrity, header.Offset, Data: data?.Payload));
            structures.Add(new(FluxStructureKind.IdAddressMark, header.Offset, header.CrcValid is null ? IsoFmFormat.EncodedMarkBitCount : IsoFmFormat.HeaderBitCount, IsoFmDescriptions.Header(header, data?.CrcValid)));
        }
        foreach (var mark in dataMarks.Where(mark => !pairedData.Contains(mark)))
        {
            byteSegments.Add((mark.Offset, [mark.Definition.Mark]));
            structures.Add(new(mark.Definition.Kind, mark.Offset, IsoFmFormat.EncodedMarkBitCount, IsoFmDescriptions.Unclassified(mark.Definition)));
        }
        var decodedBytes = byteSegments.OrderBy(segment => segment.Offset).SelectMany(segment => segment.Bytes).ToArray();
        var ordered = structures.OrderBy(structure => structure.BitOffset).ToArray();
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, ordered.Length, IsoFmFormat.ConfidenceSectorWeight, IsoFmFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, ordered, decodedBytes, sectors);
    }

    /// <summary>Collecte les marques et décode immédiatement les en-têtes complets.</summary>
    private static void CollectMarks(FluxBitstream stream, out List<IsoFmHeader> headers, out List<IsoFmDataMark> dataMarks)
    {
        headers = [];
        dataMarks = [];
        for (var offset = 0; offset + IsoFmFormat.EncodedMarkBitCount <= stream.Bits.Length; offset++)
        {
            var mark = RecognizeMark(stream, offset);
            if (mark is null) continue;
            if (mark.Mark == IsoFmFormat.IdAddressMark) headers.Add(TryDecodeHeader(stream, offset));
            else dataMarks.Add(new(offset, mark));
            offset += mark.Mark == IsoFmFormat.IdAddressMark && offset + IsoFmFormat.HeaderBitCount <= stream.Bits.Length ? IsoFmFormat.HeaderScanAdvance : IsoFmFormat.MarkScanAdvance;
        }
    }

    /// <summary>Reconnaît une marque ISO FM à la position indiquée.</summary>
    internal static IsoFmMarkDefinition? RecognizeMark(FluxBitstream stream, int offset) => IsoFmFormat.Marks.FirstOrDefault(mark => FluxBitReader.Match(stream, offset, mark.Pattern));

    /// <summary>Décode CHRN, la taille et le CRC d'un en-tête.</summary>
    internal static IsoFmHeader TryDecodeHeader(FluxBitstream stream, int offset)
    {
        if (offset + IsoFmFormat.HeaderBitCount > stream.Bits.Length) return new(offset, 0, 0, 0, 0, 0, null, null);
        var bytes = DecodeBytes(stream, offset + IsoFmFormat.EncodedMarkBitCount, IsoFmFormat.HeaderBytesAfterMark);
        if (bytes is null) return new(offset, 0, 0, 0, 0, 0, null, null);
        var cylinder = bytes[IsoFmFormat.HeaderCylinderOffset];
        var head = bytes[IsoFmFormat.HeaderHeadOffset];
        var sector = bytes[IsoFmFormat.HeaderSectorOffset];
        var sizeCode = bytes[IsoFmFormat.HeaderSizeCodeOffset];
        var stored = (ushort)((bytes[IsoFmFormat.HeaderFieldByteCount] << BitPrimitives.BitsPerByte) | bytes[IsoFmFormat.HeaderFieldByteCount + 1]);
        var calculated = Crc16Calculator.Compute([IsoFmFormat.IdAddressMark, cylinder, head, sector, sizeCode], IsoFmFormat.CrcPolynomial, IsoFmFormat.CrcInitialValue);
        return new(offset, cylinder, head, sector, sizeCode, IsoFmFormat.SectorSize(sizeCode), stored == calculated, bytes);
    }

    /// <summary>Sélectionne la première marque de données comprise avant l'en-tête suivant.</summary>
    private static IsoFmDataMark? SelectDataMark(IsoFmHeader header, int nextHeaderOffset, IEnumerable<IsoFmDataMark> marks) => marks.FirstOrDefault(mark => mark.Offset >= header.Offset + IsoFmFormat.HeaderBitCount && mark.Offset < nextHeaderOffset);

    /// <summary>Lit la charge utile et valide le CRC des données.</summary>
    internal static IsoFmData? TryDecodeData(FluxBitstream stream, IsoFmDataMark mark, int size)
    {
        var bytes = DecodeBytes(stream, mark.Offset + IsoFmFormat.EncodedMarkBitCount, size + IsoFmFormat.CrcByteCount);
        if (bytes is null) return null;
        var payload = bytes.Take(size).ToArray();
        var stored = (ushort)((bytes[size] << BitPrimitives.BitsPerByte) | bytes[size + 1]);
        var calculated = Crc16Calculator.Compute(new[] { mark.Definition.Mark }.Concat(payload), IsoFmFormat.CrcPolynomial, IsoFmFormat.CrcInitialValue);
        return new(payload, bytes, stored == calculated);
    }

    private static byte[]? DecodeBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * IsoFmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }

    /// <summary>Regroupe une charge utile, les octets stockés et leur CRC.</summary>
    internal sealed record IsoFmData(byte[] Payload, byte[] Bytes, bool CrcValid);
}
