using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format ISO MFM.</summary>
public sealed class IsoMfmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique.</summary>
    public string Id => IsoMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché.</summary>
    public string DisplayName => IsoMfmFormat.CodecDisplayName;

    /// <summary>Décode une révolution et sélectionne la meilleure tentative PLL.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var centre = FluxTimingEstimator.EstimateNonFmBitCell(revolution.FluxIntervals);
        return IsoMfmPllSelector.Select(revolution, centre, DecodeCore);
    }

    /// <summary>Décode un flux binaire avec une temporisation PLL déterminée.</summary>
    internal FluxDecodeResult DecodeCore(FluxBitstream source)
    {
        var originalLength = source.Bits.Length;
        var stream = source.WithCircularTail(IsoMfmFormat.CircularTailBitCount);
        CollectMarks(stream, originalLength, out var headers, out var dataMarks, out var unknownMarks);
        DuplicateCircularDataMarks(dataMarks, originalLength);
        var structures = unknownMarks.Select(item => new FluxStructure(FluxStructureKind.Sync, item.Offset, IsoMfmFormat.SyncAndMarkBitCount, FluxStructureDescriptions.UnclassifiedMark(IsoMfmFormat.StructureDescriptionName, FluxStructureKind.Sync, item.Mark, null))).ToList();
        var sectors = new List<DecodedSector>();
        var byteSegments = new List<(int Offset, byte[] Bytes)>();
        var pairedData = new HashSet<IsoMfmDataMark>();
        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index];
            byteSegments.Add((header.Offset, new[] { IsoMfmFormat.IdAddressMark }.Concat(header.Bytes ?? []).ToArray()));
            var nextHeaderOffset = index + 1 < headers.Count ? headers[index + 1].Offset : header.Offset + originalLength;
            var dataMark = dataMarks.FirstOrDefault(mark => mark.Offset >= header.Offset + IsoMfmFormat.HeaderBitCount && mark.Offset < nextHeaderOffset);
            var data = dataMark is null || header.Size == 0 ? null : TryDecodeData(stream, dataMark, header.Size);
            if (dataMark is not null)
            {
                pairedData.Add(dataMark);
                byteSegments.Add((dataMark.Offset, new[] { dataMark.Definition.Mark }.Concat(data?.Bytes ?? []).ToArray()));
                structures.Add(new(dataMark.Definition.Kind, dataMark.Offset, data is null ? IsoMfmFormat.SyncAndMarkBitCount : IsoMfmFormat.SyncAndMarkBitCount + data.Bytes.Length * IsoMfmFormat.EncodedByteBitCount, IsoMfmDescriptions.Data(header, dataMark, data?.CrcValid)));
            }
            bool? integrity = header.CrcValid == false || data?.CrcValid == false ? false : data is null ? null : true;
            sectors.Add(new(header.Cylinder, header.Head, header.Sector, header.SizeCode, header.Size, integrity, header.Offset, Data: data?.Payload));
            structures.Add(new(FluxStructureKind.IdAddressMark, header.Offset, header.CrcValid is null ? IsoMfmFormat.SyncAndMarkBitCount : IsoMfmFormat.HeaderBitCount, IsoMfmDescriptions.Header(header, data?.CrcValid)));
        }
        foreach (var mark in dataMarks.Where(mark => mark.Offset < originalLength && !pairedData.Any(paired => paired.Offset % originalLength == mark.Offset)))
        {
            byteSegments.Add((mark.Offset, [mark.Definition.Mark]));
            structures.Add(new(mark.Definition.Kind, mark.Offset, IsoMfmFormat.SyncAndMarkBitCount, IsoMfmDescriptions.Unclassified(mark)));
        }
        var decodedBytes = byteSegments.OrderBy(segment => segment.Offset).SelectMany(segment => segment.Bytes).ToArray();
        var ordered = structures.OrderBy(structure => structure.BitOffset).ToArray();
        var dataStructureCount = ordered.Count(structure => structure.Kind is FluxStructureKind.DataAddressMark or FluxStructureKind.DeletedDataAddressMark);
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, dataStructureCount, IsoMfmFormat.ConfidenceSectorWeight, IsoMfmFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, ordered, decodedBytes, sectors);
    }

    /// <summary>Reconnaît les trois synchronisations puis la marque suivante.</summary>
    internal static byte? RecognizeMark(FluxBitstream stream, int offset)
    {
        for (var index = 0; index < IsoMfmFormat.SyncByteCount; index++) if (!FluxBitReader.Match(stream, offset + index * IsoMfmFormat.EncodedByteBitCount, IsoMfmFormat.EncodedSyncByte)) return null;
        return FluxBitReader.TryDecodeMfmByte(stream, offset + IsoMfmFormat.SyncBitCount, out var mark) ? mark : null;
    }

    /// <summary>Collecte les en-têtes, données et marques inconnues.</summary>
    private static void CollectMarks(FluxBitstream stream, int originalLength, out List<IsoMfmHeader> headers, out List<IsoMfmDataMark> dataMarks, out List<(int Offset, byte Mark)> unknownMarks)
    {
        headers = [];
        dataMarks = [];
        unknownMarks = [];
        for (var offset = 0; offset + IsoMfmFormat.SyncAndMarkBitCount <= originalLength; offset++)
        {
            var markValue = RecognizeMark(stream, offset);
            if (markValue is null) continue;
            var definition = IsoMfmFormat.Marks.FirstOrDefault(mark => mark.Mark == markValue.Value);
            if (definition is null) unknownMarks.Add((offset, markValue.Value));
            else if (definition.Mark == IsoMfmFormat.IdAddressMark) headers.Add(TryDecodeHeader(stream, offset));
            else dataMarks.Add(new(offset, definition));
            offset += definition?.Mark == IsoMfmFormat.IdAddressMark && offset + IsoMfmFormat.HeaderBitCount <= stream.Bits.Length ? IsoMfmFormat.HeaderScanAdvance : IsoMfmFormat.MarkScanAdvance;
        }
    }

    /// <summary>Duplique les marques comprises dans la portion circulaire réellement copiée.</summary>
    private static void DuplicateCircularDataMarks(List<IsoMfmDataMark> marks, int originalLength)
    {
        var tailLength = Math.Min(originalLength, IsoMfmFormat.CircularTailBitCount);
        marks.AddRange(marks.Where(mark => mark.Offset < tailLength).Select(mark => new IsoMfmDataMark(mark.Offset + originalLength, mark.Definition)).ToArray());
    }

    /// <summary>Décode CHRN, la taille et le CRC d'en-tête.</summary>
    internal static IsoMfmHeader TryDecodeHeader(FluxBitstream stream, int offset)
    {
        if (offset + IsoMfmFormat.HeaderBitCount > stream.Bits.Length) return new(offset, 0, 0, 0, 0, 0, null, null);
        var bytes = DecodeBytes(stream, offset + IsoMfmFormat.SyncAndMarkBitCount, IsoMfmFormat.HeaderBytesAfterMark);
        if (bytes is null) return new(offset, 0, 0, 0, 0, 0, null, null);
        var cylinder = bytes[IsoMfmFormat.HeaderCylinderOffset];
        var head = bytes[IsoMfmFormat.HeaderHeadOffset];
        var sector = bytes[IsoMfmFormat.HeaderSectorOffset];
        var sizeCode = bytes[IsoMfmFormat.HeaderSizeCodeOffset];
        var stored = (ushort)((bytes[IsoMfmFormat.HeaderFieldByteCount] << BitPrimitives.BitsPerByte) | bytes[IsoMfmFormat.HeaderFieldByteCount + 1]);
        var calculated = Crc16Calculator.Compute(new[] { IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.IdAddressMark, cylinder, head, sector, sizeCode }, IsoMfmFormat.CrcPolynomial, IsoMfmFormat.CrcInitialValue);
        return new(offset, cylinder, head, sector, sizeCode, IsoMfmFormat.SectorSize(sizeCode), stored == calculated, bytes);
    }

    /// <summary>Lit la charge utile et valide son CRC.</summary>
    internal static IsoMfmData? TryDecodeData(FluxBitstream stream, IsoMfmDataMark mark, int size)
    {
        var bytes = DecodeBytes(stream, mark.Offset + IsoMfmFormat.SyncAndMarkBitCount, size + IsoMfmFormat.CrcByteCount);
        if (bytes is null) return null;
        var payload = bytes.Take(size).ToArray();
        var stored = (ushort)((bytes[size] << BitPrimitives.BitsPerByte) | bytes[size + 1]);
        var calculated = Crc16Calculator.Compute(new[] { IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, mark.Definition.Mark }.Concat(payload), IsoMfmFormat.CrcPolynomial, IsoMfmFormat.CrcInitialValue);
        return new(payload, bytes, stored == calculated);
    }

    /// <summary>Décode des octets MFM consécutifs.</summary>
    private static byte[]? DecodeBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * IsoMfmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }

    /// <summary>Regroupe la charge utile, les octets stockés et leur CRC.</summary>
    internal sealed record IsoMfmData(byte[] Payload, byte[] Bytes, bool CrcValid);
}
