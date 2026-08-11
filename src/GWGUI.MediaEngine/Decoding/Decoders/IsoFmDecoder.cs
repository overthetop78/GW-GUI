using GWGUI.MediaEngine.Containers.Scp;

using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

public sealed class IsoFmDecoder : IFluxDecoder
{
    public string Id => FluxCodecIds.IsoFm; public string DisplayName => "ISO FM (simple densité)";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveFm(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        var headers = new List<(int Offset, byte Cylinder, byte Head, byte Number, byte SizeCode, int Size, bool? Valid)>(); var dataMarks = new List<(int Offset, byte Mark)>();
        for (var offset = 0; offset + 16 <= stream.Bits.Length; offset++)
        {
            var mark = FluxBitReader.Match(stream, offset, 0xf57e) ? (byte)0xfe : FluxBitReader.Match(stream, offset, 0xf56f) ? (byte)0xfb : FluxBitReader.Match(stream, offset, 0xf56a) ? (byte)0xf8 : (byte)0;
            if (mark == 0) continue; bytes.Add(mark);
            var kind = mark == 0xfe ? FluxStructureKind.IdAddressMark : mark == 0xfb ? FluxStructureKind.DataAddressMark : FluxStructureKind.DeletedDataAddressMark;
            var description = mark == 0xfe ? "En-tête de secteur FM" : mark == 0xfb ? "Données de secteur FM" : "Données supprimées FM";
            if (mark == 0xfe && offset + 112 <= stream.Bits.Length)
            {
                var headerBytes = TryDecodeMfmBytes(stream, offset + 16, 6);
                if (headerBytes is null) continue;
                var cylinder = headerBytes[0]; var head = headerBytes[1]; var number = headerBytes[2]; var sizeCode = headerBytes[3];
                var storedCrc = (ushort)((headerBytes[4] << BitPrimitives.BitsPerByte) | headerBytes[5]); var calculatedCrc = Primitives.Crc16Calculator.Compute([0xfe, cylinder, head, number, sizeCode]); var valid = storedCrc == calculatedCrc;
                headers.Add((offset, cylinder, head, number, sizeCode, sizeCode <= 7 ? 128 << sizeCode : 0, valid)); description = $"Secteur FM C{cylinder} H{head} R{number} N{sizeCode}, CRC {(valid ? "valide" : "incorrect")}";
            }
            else if (mark == 0xfe) headers.Add((offset, 0, 0, 0, 0, 0, null));
            else dataMarks.Add((offset, mark));
            structures.Add(new(kind, offset, mark == 0xfe ? 112 : 16, description)); offset += 15;
        }
        structures.RemoveAll(structure => structure.Kind == FluxStructureKind.IdAddressMark);
        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index]; var nextHeader = index + 1 < headers.Count ? headers[index + 1].Offset : int.MaxValue;
            (int Offset, byte Mark)? data = dataMarks.Where(candidate => candidate.Offset > header.Offset + 111 && candidate.Offset < nextHeader).Select(candidate => ((int, byte)?)candidate).FirstOrDefault(); bool? dataValid = null; byte[]? payload = null;
            if (data is not null && header.Size > 0)
            {
                var end = data.Value.Offset + 16 + (header.Size + 2) * 16;
                if (end <= stream.Bits.Length)
                {
                    var dataBytes = TryDecodeMfmBytes(stream, data.Value.Offset + 16, header.Size + 2);
                    if (dataBytes is null) continue;
                    payload = dataBytes.AsSpan(0, header.Size).ToArray();
                    var stored = (ushort)((dataBytes[header.Size] << BitPrimitives.BitsPerByte) | dataBytes[header.Size + 1]); dataValid = stored == Primitives.Crc16Calculator.Compute(new[] { data.Value.Mark }.Concat(payload)); bytes.AddRange(payload);
                    structures.RemoveAll(structure => structure.BitOffset == data.Value.Offset); structures.Add(new(data.Value.Mark == 0xfb ? FluxStructureKind.DataAddressMark : FluxStructureKind.DeletedDataAddressMark, data.Value.Offset, end - data.Value.Offset, $"FM {(data.Value.Mark == 0xf8 ? "deleted " : "")}data, {header.Size} bytes, CRC {(dataValid == true ? "valid" : "invalid")}"));
                }
            }
            bool? integrity = header.Valid == false || dataValid == false ? false : dataValid is null ? null : true; sectors.Add(new(header.Cylinder, header.Head, header.Number, header.SizeCode, header.Size, integrity, header.Offset, Data: payload));
            structures.Add(new(FluxStructureKind.IdAddressMark, header.Offset, header.Valid is null ? 16 : 112, $"FM C{header.Cylinder} H{header.Head} R{header.Number} N{header.SizeCode}, header CRC {(header.Valid is null ? "unavailable" : header.Valid == true ? "valid" : "invalid")}, data CRC {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 18d), stream.BitCellTicks, structures, bytes, sectors);
    }
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
