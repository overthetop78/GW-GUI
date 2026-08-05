namespace GWGUI.Scp.Decoding;

public enum FluxStructureKind { Sync, IdAddressMark, DataAddressMark, DeletedDataAddressMark, AmigaSync, AppleAddress, AppleData, CommodoreSync, CommodoreHeader, TimingAnomaly }
public sealed record FluxStructure(FluxStructureKind Kind, int BitOffset, int BitLength, string Description);
public sealed record DecodedSector(byte Cylinder, byte Head, byte Number, byte SizeCode, int SizeBytes, bool HeaderCrcValid, int BitOffset);
public sealed record FluxDecodeResult(string DecoderId, string DisplayName, double Confidence, double EstimatedBitCellTicks, IReadOnlyList<FluxStructure> Structures, IReadOnlyList<byte> DecodedBytes, IReadOnlyList<DecodedSector>? Sectors = null);

public interface IFluxDecoder
{
    string Id { get; }
    string DisplayName { get; }
    FluxDecodeResult Decode(ScpRevolution revolution);
}

public sealed class FluxDecoderRegistry
{
    public IReadOnlyList<IFluxDecoder> Decoders { get; } = [new IsoMfmDecoder(), new IsoFmDecoder(), new AmigaMfmDecoder(), new AppleGcrDecoder(), new CommodoreGcrDecoder(), new RawFluxDecoder()];
    public FluxDecodeResult DecodeAutomatic(ScpRevolution revolution) => Decoders.Select(x => x.Decode(revolution)).OrderByDescending(x => x.Confidence).First();
    public FluxDecodeResult Decode(string id, ScpRevolution revolution) => Decoders.First(x => x.Id == id).Decode(revolution);
    public (int RevolutionIndex, FluxDecodeResult Result)? DecodeBest(IReadOnlyList<ScpRevolution> revolutions, string? decoderId = null)
    {
        if (revolutions.Count == 0) return null;
        return revolutions.Select((revolution, index) => (RevolutionIndex: index, Result: decoderId is null ? DecodeAutomatic(revolution) : Decode(decoderId, revolution)))
            .OrderByDescending(candidate => candidate.Result.Confidence)
            .ThenByDescending(candidate => candidate.Result.Structures.Count)
            .First();
    }
}

public sealed class IsoFmDecoder : IFluxDecoder
{
    public string Id => "iso.fm"; public string DisplayName => "ISO FM (simple densité)";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals, fm: true); var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        for (var offset = 0; offset + 16 <= stream.Bits.Length; offset++)
        {
            var mark = stream.Match(offset, 0xf57e) ? (byte)0xfe : stream.Match(offset, 0xf56f) ? (byte)0xfb : stream.Match(offset, 0xf56a) ? (byte)0xf8 : (byte)0;
            if (mark == 0) continue; bytes.Add(mark);
            var kind = mark == 0xfe ? FluxStructureKind.IdAddressMark : mark == 0xfb ? FluxStructureKind.DataAddressMark : FluxStructureKind.DeletedDataAddressMark;
            var description = mark == 0xfe ? "En-tête de secteur FM" : mark == 0xfb ? "Données de secteur FM" : "Données supprimées FM";
            if (mark == 0xfe && offset + 112 <= stream.Bits.Length)
            {
                var cylinder = stream.DecodeMfmByte(offset + 16); var head = stream.DecodeMfmByte(offset + 32); var number = stream.DecodeMfmByte(offset + 48); var sizeCode = stream.DecodeMfmByte(offset + 64);
                var storedCrc = (ushort)((stream.DecodeMfmByte(offset + 80) << 8) | stream.DecodeMfmByte(offset + 96)); var calculatedCrc = Crc16([0xfe, cylinder, head, number, sizeCode]); var valid = storedCrc == calculatedCrc;
                sectors.Add(new(cylinder, head, number, sizeCode, sizeCode <= 7 ? 128 << sizeCode : 0, valid, offset)); description = $"Secteur FM C{cylinder} H{head} R{number} N{sizeCode}, CRC {(valid ? "valide" : "incorrect")}";
            }
            structures.Add(new(kind, offset, mark == 0xfe ? 112 : 16, description)); offset += 15;
        }
        return new(Id, DisplayName, Math.Min(1, sectors.Count / 8d), stream.BitCellTicks, structures, bytes, sectors);
    }
    private static ushort Crc16(IEnumerable<byte> values) { ushort crc = 0xffff; foreach (var value in values) { crc ^= (ushort)(value << 8); for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1); } return crc; }
}

public sealed class RawFluxDecoder : IFluxDecoder
{
    public string Id => "raw"; public string DisplayName => "Flux brut";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var median = FluxBitstream.EstimateBitCell(revolution.FluxIntervals);
        var anomalies = new List<FluxStructure>();
        var bitOffset = 0;
        foreach (var interval in revolution.FluxIntervals)
        {
            var bitLength = Math.Clamp((int)Math.Round(interval / median), 1, 64);
            if (interval > median * 10) anomalies.Add(new(FluxStructureKind.TimingAnomaly, bitOffset, bitLength, "Intervalle de flux exceptionnellement long."));
            bitOffset += bitLength;
        }
        return new(Id, DisplayName, .05, median, anomalies, []);
    }
}

public sealed class IsoMfmDecoder : IFluxDecoder
{
    public string Id => "iso.mfm"; public string DisplayName => "ISO MFM (Atari ST / IBM PC)";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var bytes = new List<byte>(); var sectors = new List<DecodedSector>();
        for (var offset = 0; offset + 64 <= stream.Bits.Length; offset++)
        {
            if (!stream.Match(offset, 0x4489) || !stream.Match(offset + 16, 0x4489) || !stream.Match(offset + 32, 0x4489)) continue;
            var mark = stream.DecodeMfmByte(offset + 48); var kind = mark switch { 0xfe => FluxStructureKind.IdAddressMark, 0xfb => FluxStructureKind.DataAddressMark, 0xf8 => FluxStructureKind.DeletedDataAddressMark, _ => FluxStructureKind.Sync };
            var description = mark is 0xfe ? "En-tête de secteur MFM" : mark is 0xfb ? "Données de secteur MFM" : mark is 0xf8 ? "Données supprimées MFM" : $"Synchronisation MFM, marque 0x{mark:X2}";
            if (mark == 0xfe && offset + 160 <= stream.Bits.Length)
            {
                var cylinder = stream.DecodeMfmByte(offset + 64); var head = stream.DecodeMfmByte(offset + 80); var number = stream.DecodeMfmByte(offset + 96); var sizeCode = stream.DecodeMfmByte(offset + 112);
                var storedCrc = (ushort)((stream.DecodeMfmByte(offset + 128) << 8) | stream.DecodeMfmByte(offset + 144));
                var calculatedCrc = Crc16([0xa1, 0xa1, 0xa1, 0xfe, cylinder, head, number, sizeCode]); var valid = storedCrc == calculatedCrc;
                sectors.Add(new(cylinder, head, number, sizeCode, sizeCode <= 7 ? 128 << sizeCode : 0, valid, offset));
                description = $"Secteur C{cylinder} H{head} R{number} N{sizeCode} ({(sizeCode <= 7 ? 128 << sizeCode : 0)} octets), CRC {(valid ? "valide" : "incorrect")}";
            }
            structures.Add(new(kind, offset, mark == 0xfe ? 160 : 64, description)); bytes.Add(mark); offset += 47;
        }
        var confidence = Math.Min(1, (sectors.Count * 2 + structures.Count(x => x.Kind is FluxStructureKind.DataAddressMark or FluxStructureKind.DeletedDataAddressMark)) / 12d);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, bytes, sectors);
    }

    private static ushort Crc16(IEnumerable<byte> values) { ushort crc = 0xffff; foreach (var value in values) { crc ^= (ushort)(value << 8); for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1); } return crc; }
}

public sealed class AmigaMfmDecoder : IFluxDecoder
{
    public string Id => "amiga.mfm"; public string DisplayName => "Amiga MFM";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>();
        for (var offset = 0; offset + 32 <= stream.Bits.Length; offset++) if (stream.Match(offset, 0x4489) && stream.Match(offset + 16, 0x4489)) { structures.Add(new(FluxStructureKind.AmigaSync, offset, 32, "Mot de synchronisation Amiga 0x44894489")); offset += 31; }
        return new(Id, DisplayName, Math.Min(1, structures.Count / 11d), stream.BitCellTicks, structures, []);
    }
}

public sealed class AppleGcrDecoder : IFluxDecoder
{
    public string Id => "apple2.gcr"; public string DisplayName => "Apple II GCR";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromNrziIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var bytes = new List<byte>();
        for (var offset = 0; offset + 24 <= stream.Bits.Length; offset++)
        {
            var kind = stream.Match(offset, 0xD5AA96, 24) ? FluxStructureKind.AppleAddress : stream.Match(offset, 0xD5AAAD, 24) ? FluxStructureKind.AppleData : (FluxStructureKind?)null;
            if (kind is null) continue;
            structures.Add(new(kind.Value, offset, 24, kind == FluxStructureKind.AppleAddress ? "Apple II address prologue D5 AA 96" : "Apple II data prologue D5 AA AD"));
            bytes.AddRange(kind == FluxStructureKind.AppleAddress ? [0xd5, 0xaa, 0x96] : [0xd5, 0xaa, 0xad]); offset += 23;
        }
        return new(Id, DisplayName, Math.Min(1, structures.Count / 16d), stream.BitCellTicks, structures, bytes);
    }
}

public sealed class CommodoreGcrDecoder : IFluxDecoder
{
    private static readonly Dictionary<int, int> Gcr = new() { [0b01010]=0,[0b01011]=1,[0b10010]=2,[0b10011]=3,[0b01110]=4,[0b01111]=5,[0b10110]=6,[0b10111]=7,[0b01001]=8,[0b11001]=9,[0b11010]=10,[0b11011]=11,[0b01101]=12,[0b11101]=13,[0b11110]=14,[0b10101]=15 };
    public string Id => "commodore.gcr"; public string DisplayName => "Commodore GCR";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromNrziIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var bytes = new List<byte>();
        for (var offset = 0; offset < stream.Bits.Length; offset++)
        {
            if (!stream.Bits[offset]) continue; var end = offset; while (end < stream.Bits.Length && stream.Bits[end]) end++;
            var length = end - offset;
            if (length >= 10)
            {
                structures.Add(new(FluxStructureKind.CommodoreSync, offset, length, "Commodore GCR sync"));
                if (TryDecodeByte(stream.Bits, end, out var value))
                {
                    bytes.Add(value);
                    if (value == 0x08) structures.Add(new(FluxStructureKind.CommodoreHeader, end, 10, "Commodore GCR header block"));
                }
            }
            offset = end;
        }
        var headers = structures.Count(x => x.Kind == FluxStructureKind.CommodoreHeader);
        return new(Id, DisplayName, Math.Min(1, (headers * 2 + structures.Count(x => x.Kind == FluxStructureKind.CommodoreSync)) / 24d), stream.BitCellTicks, structures, bytes);
    }

    private static bool TryDecodeByte(bool[] bits, int offset, out byte value)
    {
        value = 0; if (offset + 10 > bits.Length) return false;
        var high = 0; var low = 0;
        for (var bit = 0; bit < 5; bit++) { high = (high << 1) | (bits[offset + bit] ? 1 : 0); low = (low << 1) | (bits[offset + 5 + bit] ? 1 : 0); }
        if (!Gcr.TryGetValue(high, out var highNibble) || !Gcr.TryGetValue(low, out var lowNibble)) return false;
        value = (byte)((highNibble << 4) | lowNibble); return true;
    }
}

internal sealed class FluxBitstream(bool[] bits, double bitCellTicks)
{
    public bool[] Bits { get; } = bits; public double BitCellTicks { get; } = bitCellTicks;
    public static FluxBitstream FromIntervals(IReadOnlyList<uint> intervals, bool fm = false)
    {
        var bitCell = EstimateBitCell(intervals, fm); var bits = new List<bool>(intervals.Count * 3);
        foreach (var interval in intervals) { var cells = Math.Clamp((int)Math.Round(interval / bitCell), 1, 32); for (var zero = 1; zero < cells; zero++) bits.Add(false); bits.Add(true); }
        return new(bits.ToArray(), bitCell);
    }
    public static FluxBitstream FromNrziIntervals(IReadOnlyList<uint> intervals)
    {
        var bitCell = EstimateBitCell(intervals, true); var bits = new List<bool>(intervals.Count * 4);
        foreach (var interval in intervals) { var cells = Math.Clamp((int)Math.Round(interval / bitCell), 1, 64); for (var zero = 1; zero < cells; zero++) bits.Add(false); bits.Add(true); }
        return new(bits.ToArray(), bitCell);
    }
    public static double EstimateBitCell(IReadOnlyList<uint> intervals, bool fm = false)
    {
        if (intervals.Count == 0) return 1; var sorted = intervals.Where(x => x > 0).Order().ToArray(); if (sorted.Length == 0) return 1;
        var sampleLength = Math.Max(1, sorted.Length / 5); var lowerCluster = sorted.Take(sampleLength).ToArray(); var robustLower = lowerCluster[lowerCluster.Length / 2];
        return Math.Max(1, fm ? robustLower : robustLower / 2d);
    }
    public bool Match(int offset, ushort pattern) { if (offset + 16 > Bits.Length) return false; for (var bit = 0; bit < 16; bit++) if (Bits[offset + bit] != ((pattern & (1 << (15 - bit))) != 0)) return false; return true; }
    public bool Match(int offset, uint pattern, int length) { if (length is < 1 or > 32 || offset + length > Bits.Length) return false; for (var bit = 0; bit < length; bit++) if (Bits[offset + bit] != ((pattern & (1u << (length - 1 - bit))) != 0)) return false; return true; }
    public byte DecodeMfmByte(int offset) { byte value = 0; for (var bit = 0; bit < 8 && offset + bit * 2 + 1 < Bits.Length; bit++) if (Bits[offset + bit * 2 + 1]) value |= (byte)(1 << (7 - bit)); return value; }
}
