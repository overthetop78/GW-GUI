using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Iso FM.</summary>
public sealed class IsoFmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.IsoFm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.IsoFm;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveFm(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        var headers = new List<(int Offset, byte Cylinder, byte Head, byte Number, byte SizeCode, int Size, bool? Valid)>(); var dataMarks = new List<(int Offset, byte Mark)>();
        for (var offset = 0; offset + IsoFmFormat.EncodedMarkBitCount <= stream.Bits.Length; offset++)
        {
            var mark = FluxBitReader.Match(stream, offset, IsoFmFormat.EncodedIdAddressMark) ? IsoFmFormat.IdAddressMark : FluxBitReader.Match(stream, offset, IsoFmFormat.EncodedDataAddressMark) ? IsoFmFormat.DataAddressMark : FluxBitReader.Match(stream, offset, IsoFmFormat.EncodedDeletedDataAddressMark) ? IsoFmFormat.DeletedDataAddressMark : (byte)0;
            if (mark == 0) continue; bytes.Add(mark);
            var kind = mark == IsoFmFormat.IdAddressMark ? FluxStructureKind.IdAddressMark : mark == IsoFmFormat.DataAddressMark ? FluxStructureKind.DataAddressMark : FluxStructureKind.DeletedDataAddressMark;
            var description = mark == IsoFmFormat.IdAddressMark ? "En-tête de secteur FM" : mark == IsoFmFormat.DataAddressMark ? "Données de secteur FM" : "Données supprimées FM";
            if (mark == IsoFmFormat.IdAddressMark && offset + IsoFmFormat.HeaderBitCount <= stream.Bits.Length)
            {
                var headerBytes = TryDecodeMfmBytes(stream, offset + IsoFmFormat.EncodedMarkBitCount, IsoFmFormat.HeaderBytesAfterMark);
                if (headerBytes is null) continue;
                var cylinder = headerBytes[0]; var head = headerBytes[1]; var number = headerBytes[2]; var sizeCode = headerBytes[3];
                var storedCrc = (ushort)((headerBytes[IsoFmFormat.HeaderFieldByteCount] << BitPrimitives.BitsPerByte) | headerBytes[IsoFmFormat.HeaderFieldByteCount + 1]); var calculatedCrc = Primitives.Crc16Calculator.Compute([IsoFmFormat.IdAddressMark, cylinder, head, number, sizeCode], IsoFmFormat.CrcPolynomial, IsoFmFormat.CrcInitialValue); var valid = storedCrc == calculatedCrc;
                headers.Add((offset, cylinder, head, number, sizeCode, sizeCode <= IsoFmFormat.MaximumSectorSizeCode ? IsoFmFormat.BaseSectorSize << sizeCode : 0, valid)); description = $"Secteur FM C{cylinder} H{head} R{number} N{sizeCode}, CRC {(valid ? "valide" : "incorrect")}";
            }
            else if (mark == IsoFmFormat.IdAddressMark) headers.Add((offset, 0, 0, 0, 0, 0, null));
            else dataMarks.Add((offset, mark));
            structures.Add(new(kind, offset, mark == IsoFmFormat.IdAddressMark ? IsoFmFormat.HeaderBitCount : IsoFmFormat.EncodedMarkBitCount, description)); offset += IsoFmFormat.EncodedMarkBitCount - 1;
        }
        structures.RemoveAll(structure => structure.Kind == FluxStructureKind.IdAddressMark);
        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index]; var nextHeader = index + 1 < headers.Count ? headers[index + 1].Offset : int.MaxValue;
            (int Offset, byte Mark)? data = dataMarks.Where(candidate => candidate.Offset > header.Offset + IsoFmFormat.HeaderBitCount - 1 && candidate.Offset < nextHeader).Select(candidate => ((int, byte)?)candidate).FirstOrDefault(); bool? dataValid = null; byte[]? payload = null;
            if (data is not null && header.Size > 0)
            {
                var end = data.Value.Offset + IsoFmFormat.EncodedMarkBitCount + (header.Size + IsoFmFormat.CrcByteCount) * IsoFmFormat.EncodedByteBitCount;
                if (end <= stream.Bits.Length)
                {
                    var dataBytes = TryDecodeMfmBytes(stream, data.Value.Offset + IsoFmFormat.EncodedMarkBitCount, header.Size + IsoFmFormat.CrcByteCount);
                    if (dataBytes is null) continue;
                    payload = dataBytes.AsSpan(0, header.Size).ToArray();
                    var stored = (ushort)((dataBytes[header.Size] << BitPrimitives.BitsPerByte) | dataBytes[header.Size + 1]); dataValid = stored == Primitives.Crc16Calculator.Compute(new[] { data.Value.Mark }.Concat(payload), IsoFmFormat.CrcPolynomial, IsoFmFormat.CrcInitialValue); bytes.AddRange(payload);
                    structures.RemoveAll(structure => structure.BitOffset == data.Value.Offset); structures.Add(new(data.Value.Mark == IsoFmFormat.DataAddressMark ? FluxStructureKind.DataAddressMark : FluxStructureKind.DeletedDataAddressMark, data.Value.Offset, end - data.Value.Offset, $"FM {(data.Value.Mark == IsoFmFormat.DeletedDataAddressMark ? "deleted " : "")}data, {header.Size} bytes, CRC {(dataValid == true ? "valid" : "invalid")}"));
                }
            }
            bool? integrity = header.Valid == false || dataValid == false ? false : dataValid is null ? null : true; sectors.Add(new(header.Cylinder, header.Head, header.Number, header.SizeCode, header.Size, integrity, header.Offset, Data: payload));
            structures.Add(new(FluxStructureKind.IdAddressMark, header.Offset, header.Valid is null ? IsoFmFormat.EncodedMarkBitCount : IsoFmFormat.HeaderBitCount, $"FM C{header.Cylinder} H{header.Head} R{header.Number} N{header.SizeCode}, header CRC {(header.Valid is null ? "unavailable" : header.Valid == true ? "valid" : "invalid")}, data CRC {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 18d), stream.BitCellTicks, structures, bytes, sectors);
    }
    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
