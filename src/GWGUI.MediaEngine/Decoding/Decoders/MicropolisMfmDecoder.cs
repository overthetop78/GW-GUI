using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Encoding.Definitions;

using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

public sealed class MicropolisMfmDecoder : IFluxDecoder
{
    private static readonly byte[] Sync = MicropolisMfmFormat.Sync.ToArray();

    public string Id => FluxCodecIds.MicropolisMfm;
    public string DisplayName => FluxCodecDisplayNames.MicropolisMfm;

    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        const int recordBytes = MicropolisMfmFormat.RecordByteCount;

        for (var offset = 0; offset + Sync.Length * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, Sync)) continue;
            var recordStart = offset + 3 * 16;
            if (recordStart + recordBytes * 16 > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, Sync.Length * BitPrimitives.BitsPerByte, "Incomplete Micropolis sector"));
                offset += Sync.Length * BitPrimitives.BitsPerByte - 1;
                continue;
            }

            var record = TryDecodeMfmBytes(stream, recordStart, recordBytes);
            if (record is null) continue;
            var cylinder = record[1];
            var sectorNumber = record[2];
            var valid = Checksum(record.AsSpan(1, recordBytes - 7)) == record[recordBytes - 6];
            var payload = record.AsSpan(MicropolisMfmFormat.RecordIdentityByteCount + MicropolisMfmFormat.HeaderPaddingByteCount, MicropolisMfmFormat.SectorSize).ToArray();
            bytes.AddRange(payload);
            sectors.Add(new(cylinder, 0, sectorNumber, 1, MicropolisMfmFormat.SectorSize, valid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, (3 + recordBytes) * 16,
                $"Micropolis C{cylinder} R{sectorNumber}, 256 bytes, checksum {(valid ? "valid" : "invalid")}"));
            offset += Sync.Length * BitPrimitives.BitsPerByte - 1;
        }

        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 24d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static byte Checksum(ReadOnlySpan<byte> data)
    {
        var value = 0;
        foreach (var item in data)
        {
            if (value > MicropolisMfmFormat.ChecksumModulus) value -= MicropolisMfmFormat.ChecksumModulus;
            value += item;
        }
        return (byte)value;
    }

    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
