namespace GWGUI.Scp.Decoding;

public enum FluxStructureKind { Sync, IdAddressMark, DataAddressMark, DeletedDataAddressMark, AmigaSync, TimingAnomaly }
public sealed record FluxStructure(FluxStructureKind Kind, int BitOffset, int BitLength, string Description);
public sealed record FluxDecodeResult(string DecoderId, string DisplayName, double Confidence, double EstimatedBitCellTicks, IReadOnlyList<FluxStructure> Structures, IReadOnlyList<byte> DecodedBytes);

public interface IFluxDecoder
{
    string Id { get; }
    string DisplayName { get; }
    FluxDecodeResult Decode(ScpRevolution revolution);
}

public sealed class FluxDecoderRegistry
{
    public IReadOnlyList<IFluxDecoder> Decoders { get; } = [new IsoMfmDecoder(), new AmigaMfmDecoder(), new RawFluxDecoder()];
    public FluxDecodeResult DecodeAutomatic(ScpRevolution revolution) => Decoders.Select(x => x.Decode(revolution)).OrderByDescending(x => x.Confidence).First();
    public FluxDecodeResult Decode(string id, ScpRevolution revolution) => Decoders.First(x => x.Id == id).Decode(revolution);
}

public sealed class RawFluxDecoder : IFluxDecoder
{
    public string Id => "raw"; public string DisplayName => "Flux brut";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var median = FluxBitstream.EstimateBitCell(revolution.FluxIntervals);
        var anomalies = revolution.FluxIntervals.Select((value, index) => (value, index)).Where(x => x.value > median * 10).Select(x => new FluxStructure(FluxStructureKind.TimingAnomaly, x.index, 1, "Intervalle de flux exceptionnellement long.")).ToArray();
        return new(Id, DisplayName, .05, median, anomalies, []);
    }
}

public sealed class IsoMfmDecoder : IFluxDecoder
{
    public string Id => "iso.mfm"; public string DisplayName => "ISO MFM (Atari ST / IBM PC)";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var bytes = new List<byte>();
        for (var offset = 0; offset + 64 <= stream.Bits.Length; offset++)
        {
            if (!stream.Match(offset, 0x4489) || !stream.Match(offset + 16, 0x4489) || !stream.Match(offset + 32, 0x4489)) continue;
            var mark = stream.DecodeMfmByte(offset + 48); var kind = mark switch { 0xfe => FluxStructureKind.IdAddressMark, 0xfb => FluxStructureKind.DataAddressMark, 0xf8 => FluxStructureKind.DeletedDataAddressMark, _ => FluxStructureKind.Sync };
            structures.Add(new(kind, offset, 64, mark is 0xfe ? "En-tête de secteur MFM" : mark is 0xfb ? "Données de secteur MFM" : mark is 0xf8 ? "Données supprimées MFM" : $"Synchronisation MFM, marque 0x{mark:X2}")); bytes.Add(mark); offset += 47;
        }
        var confidence = Math.Min(1, structures.Count(x => x.Kind != FluxStructureKind.Sync) / 6d);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, bytes);
    }
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

internal sealed class FluxBitstream(bool[] bits, double bitCellTicks)
{
    public bool[] Bits { get; } = bits; public double BitCellTicks { get; } = bitCellTicks;
    public static FluxBitstream FromIntervals(IReadOnlyList<uint> intervals)
    {
        var bitCell = EstimateBitCell(intervals); var bits = new List<bool>(intervals.Count * 3);
        foreach (var interval in intervals) { var cells = Math.Clamp((int)Math.Round(interval / bitCell), 1, 32); for (var zero = 1; zero < cells; zero++) bits.Add(false); bits.Add(true); }
        return new(bits.ToArray(), bitCell);
    }
    public static double EstimateBitCell(IReadOnlyList<uint> intervals)
    {
        if (intervals.Count == 0) return 1; var sorted = intervals.Where(x => x > 0).Order().ToArray(); if (sorted.Length == 0) return 1;
        var lower = sorted[Math.Min(sorted.Length - 1, Math.Max(0, sorted.Length / 10))]; return Math.Max(1, lower / 2d);
    }
    public bool Match(int offset, ushort pattern) { if (offset + 16 > Bits.Length) return false; for (var bit = 0; bit < 16; bit++) if (Bits[offset + bit] != ((pattern & (1 << (15 - bit))) != 0)) return false; return true; }
    public byte DecodeMfmByte(int offset) { byte value = 0; for (var bit = 0; bit < 8 && offset + bit * 2 + 1 < Bits.Length; bit++) if (Bits[offset + bit * 2 + 1]) value |= (byte)(1 << (7 - bit)); return value; }
}
