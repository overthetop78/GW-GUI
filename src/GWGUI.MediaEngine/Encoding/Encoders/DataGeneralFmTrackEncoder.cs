using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

public sealed class DataGeneralFmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.DataGeneralFm;
    public override string DisplayName => "Data General 2F";

    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != 512) throw new ArgumentException("Data General sectors contain 512 bytes.");
            bits.Fm([0, 1]);
            bits.Fm([(byte)(request.Cylinder | request.Head << 7), (byte)(sector.Number << 2)]);
            bits.Gap(64);
            bits.Fm([0, 1]);
            var checksum = Checksum(sector.Data);
            bits.Fm(sector.Data.Concat([(byte)(checksum >> BitPrimitives.BitsPerByte), (byte)checksum]));
            bits.Gap(128);
        }
        return bits;
    }

    private static ushort Checksum(IReadOnlyList<byte> data)
    {
        ushort value = 0;
        for (var index = 0; index <= data.Count; index++)
        {
            var input = index < data.Count ? data[index] : (byte)0;
            value = (ushort)(((value & 0xff) ^ (value >> BitPrimitives.BitsPerByte)) | (((value & 0xff) ^ input) << BitPrimitives.BitsPerByte));
        }
        return value;
    }
}
