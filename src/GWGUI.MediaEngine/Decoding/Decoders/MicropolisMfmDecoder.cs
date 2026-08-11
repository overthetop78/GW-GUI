using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.MediaEngine.Decoding;

public sealed class MicropolisMfmDecoder : IFluxDecoder
{
    private static readonly byte[] Sync = FluxEncoding.EncodeMfm(0x00, 0x00, 0x00, 0xff);

    public string Id => "micropolis.mfm";
    public string DisplayName => "Micropolis MFM";

    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveClock(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        const int recordBytes = 275;

        for (var offset = 0; offset + Sync.Length * 8 <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, Sync)) continue;
            var recordStart = offset + 3 * 16;
            if (recordStart + recordBytes * 16 > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, Sync.Length * 8, "Incomplete Micropolis sector"));
                offset += Sync.Length * 8 - 1;
                continue;
            }

            var record = Enumerable.Range(0, recordBytes).Select(index => stream.DecodeMfmByte(recordStart + index * 16)).ToArray();
            var cylinder = record[1];
            var sectorNumber = record[2];
            var valid = Checksum(record.AsSpan(1, recordBytes - 7)) == record[recordBytes - 6];
            var payload = record.AsSpan(13, 256).ToArray();
            bytes.AddRange(payload);
            sectors.Add(new(cylinder, 0, sectorNumber, 1, 256, valid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, (3 + recordBytes) * 16,
                $"Micropolis C{cylinder} R{sectorNumber}, 256 bytes, checksum {(valid ? "valid" : "invalid")}"));
            offset += Sync.Length * 8 - 1;
        }

        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 24d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static byte Checksum(ReadOnlySpan<byte> data)
    {
        var value = 0;
        foreach (var item in data)
        {
            if (value > 255) value -= 255;
            value += item;
        }
        return (byte)value;
    }
}
