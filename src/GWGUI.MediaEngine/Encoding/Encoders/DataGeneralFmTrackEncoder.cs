using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

public sealed class DataGeneralFmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.DataGeneralFm;
    public override string DisplayName => FluxCodecDisplayNames.DataGeneralFm;

    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != DataGeneralFmFormat.SectorSize) throw DataGeneralFmFormat.InvalidSectorSize(sector.Data.Count);
            bits.Raw(DataGeneralFmFormat.Sync.ToArray());
            bits.Fm([(byte)(request.Cylinder | request.Head << DataGeneralFmFormat.HeadShift), (byte)(sector.Number << DataGeneralFmFormat.SectorShift)]);
            bits.Gap(DataGeneralFmFormat.HeaderGapBitCount);
            bits.Raw(DataGeneralFmFormat.Sync.ToArray());
            var checksum = Checksum(sector.Data);
            bits.Fm(sector.Data.Concat([(byte)(checksum >> BitPrimitives.BitsPerByte), (byte)checksum]));
            bits.Gap(DataGeneralFmFormat.DataGapBitCount);
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
