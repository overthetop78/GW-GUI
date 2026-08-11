using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

public sealed class MicropolisMfmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.MicropolisMfm;
    public override string DisplayName => FluxCodecDisplayNames.MicropolisMfm;

    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != MicropolisMfmFormat.SectorSize) throw new ArgumentException("Micropolis sectors contain 256 bytes.");
            var record = new List<byte> { MicropolisMfmFormat.AddressMark, (byte)request.Cylinder, (byte)sector.Number };
            record.AddRange(Enumerable.Repeat((byte)0, MicropolisMfmFormat.HeaderPaddingByteCount));
            record.AddRange(sector.Data);
            record.Add(Checksum(record.Skip(1)));
            record.AddRange(Enumerable.Repeat((byte)0, MicropolisMfmFormat.TrailerPaddingByteCount));
            bits.Mfm(new byte[MicropolisMfmFormat.PreambleByteCount]);
            bits.Mfm(record);
            bits.Gap(MicropolisMfmFormat.GapBitCount);
        }
        return bits;
    }

    private static byte Checksum(IEnumerable<byte> data)
    {
        var value = 0;
        foreach (var item in data) { if (value > MicropolisMfmFormat.ChecksumModulus) value -= MicropolisMfmFormat.ChecksumModulus; value += item; }
        return (byte)value;
    }
}
