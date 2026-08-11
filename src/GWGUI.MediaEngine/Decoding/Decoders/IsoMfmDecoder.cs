using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

public sealed class IsoMfmDecoder : IFluxDecoder
{
    public string Id => FluxDecoderIds.IsoMfm; public string DisplayName => "ISO MFM (Atari ST / IBM PC)";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var centre = FluxTimingEstimator.EstimateNonFmBitCell(revolution.FluxIntervals);
        var first = DecodeCore(FluxTransitionDecoder.DecodePll(revolution.FluxIntervals, centre));
        if (first.Sectors?.All(sector => sector.Data is not null && sector.IntegrityValid == true) == true) return first;

        var best = first;
        var bestScore = Score(first);
        foreach (var factor in new[] { .98, 1.02, .96, 1.04, .94, 1.06 })
        {
            var candidate = DecodeCore(FluxTransitionDecoder.DecodePll(revolution.FluxIntervals, centre * factor));
            var score = Score(candidate);
            if (score > bestScore) { best = candidate; bestScore = score; }
        }
        return best;
    }

    private FluxDecodeResult DecodeCore(FluxBitstream source)
    {
        var originalLength = source.Bits.Length;
        var stream = source.WithCircularTail(20_000);
        var structures = new List<FluxStructure>(); var bytes = new List<byte>(); var sectors = new List<DecodedSector>();
        var headers = new List<(int Offset, byte Cylinder, byte Head, byte Number, byte SizeCode, int Size, bool? Valid)>(); var dataMarks = new List<(int Offset, byte Mark)>();
        for (var offset = 0; offset + 64 <= originalLength; offset++)
        {
            if (!FluxBitReader.Match(stream, offset, 0x4489) || !FluxBitReader.Match(stream, offset + 16, 0x4489) || !FluxBitReader.Match(stream, offset + 32, 0x4489)) continue;
            if (!FluxBitReader.TryDecodeMfmByte(stream, offset + 48, out var mark)) continue;
            var kind = mark switch { 0xfe => FluxStructureKind.IdAddressMark, 0xfb => FluxStructureKind.DataAddressMark, 0xf8 => FluxStructureKind.DeletedDataAddressMark, _ => FluxStructureKind.Sync };
            var description = mark is 0xfe ? "En-tête de secteur MFM" : mark is 0xfb ? "Données de secteur MFM" : mark is 0xf8 ? "Données supprimées MFM" : $"Synchronisation MFM, marque 0x{mark:X2}";
            if (mark == 0xfe && offset + 160 <= stream.Bits.Length)
            {
                var headerBytes = TryDecodeMfmBytes(stream, offset + 64, 6);
                if (headerBytes is null) continue;
                var cylinder = headerBytes[0]; var head = headerBytes[1]; var number = headerBytes[2]; var sizeCode = headerBytes[3];
                var storedCrc = (ushort)((headerBytes[4] << 8) | headerBytes[5]);
                var calculatedCrc = Crc16([0xa1, 0xa1, 0xa1, 0xfe, cylinder, head, number, sizeCode]); var valid = storedCrc == calculatedCrc;
                headers.Add((offset, cylinder, head, number, sizeCode, sizeCode <= 7 ? 128 << sizeCode : 0, valid));
                description = $"Secteur C{cylinder} H{head} R{number} N{sizeCode} ({(sizeCode <= 7 ? 128 << sizeCode : 0)} octets), CRC {(valid ? "valide" : "incorrect")}";
            }
            else if (mark == 0xfe) headers.Add((offset, 0, 0, 0, 0, 0, null));
            else if (mark is 0xfb or 0xf8) dataMarks.Add((offset, mark));
            structures.Add(new(kind, offset, mark == 0xfe ? 160 : 64, description)); bytes.Add(mark); offset += 47;
        }
        dataMarks.AddRange(dataMarks.Where(mark => mark.Offset < stream.Bits.Length - originalLength)
            .Select(mark => (mark.Offset + originalLength, mark.Mark)).ToArray());
        structures.RemoveAll(structure => structure.Kind == FluxStructureKind.IdAddressMark);
        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index]; var nextHeader = index + 1 < headers.Count ? headers[index + 1].Offset : int.MaxValue;
            (int Offset, byte Mark)? data = dataMarks.Where(candidate => candidate.Offset > header.Offset + 159 && candidate.Offset < nextHeader).Select(candidate => ((int, byte)?)candidate).FirstOrDefault(); bool? dataValid = null; byte[]? payload = null;
            if (data is not null && header.Size > 0)
            {
                var end = data.Value.Offset + 64 + (header.Size + 2) * 16;
                if (end <= stream.Bits.Length)
                {
                    var dataBytes = TryDecodeMfmBytes(stream, data.Value.Offset + 64, header.Size + 2);
                    if (dataBytes is null) continue;
                    payload = dataBytes.AsSpan(0, header.Size).ToArray();
                    var stored = (ushort)((dataBytes[header.Size] << 8) | dataBytes[header.Size + 1]); dataValid = stored == Crc16(new byte[] { 0xa1,0xa1,0xa1,data.Value.Mark }.Concat(payload)); bytes.AddRange(payload);
                    structures.RemoveAll(structure => structure.BitOffset == data.Value.Offset); structures.Add(new(data.Value.Mark == 0xfb ? FluxStructureKind.DataAddressMark : FluxStructureKind.DeletedDataAddressMark, data.Value.Offset, end - data.Value.Offset, $"MFM {(data.Value.Mark == 0xf8 ? "deleted " : "")}data, {header.Size} bytes, CRC {(dataValid == true ? "valid" : "invalid")}"));
                }
            }
            bool? integrity = header.Valid == false || dataValid == false ? false : dataValid is null ? null : true; sectors.Add(new(header.Cylinder, header.Head, header.Number, header.SizeCode, header.Size, integrity, header.Offset, Data: payload));
            structures.Add(new(FluxStructureKind.IdAddressMark, header.Offset, header.Valid is null ? 64 : 160, $"MFM C{header.Cylinder} H{header.Head} R{header.Number} N{header.SizeCode}, header CRC {(header.Valid is null ? "unavailable" : header.Valid == true ? "valid" : "invalid")}, data CRC {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
        }
        var confidence = Math.Min(1, (sectors.Count * 2 + structures.Count(x => x.Kind is FluxStructureKind.DataAddressMark or FluxStructureKind.DeletedDataAddressMark)) / 12d);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, bytes, sectors);
    }

    private static int Score(FluxDecodeResult result)
    {
        var sectors = result.Sectors ?? [];
        return sectors.Count(sector => sector.IntegrityValid == true) * 1000
            + sectors.Count(sector => sector.Data is not null) * 10
            + sectors.Count;
    }

    private static ushort Crc16(IEnumerable<byte> values) => Primitives.Crc16Calculator.Compute(values);

    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
