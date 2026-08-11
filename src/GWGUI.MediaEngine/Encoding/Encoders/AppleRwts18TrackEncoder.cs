using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encodes the standard Brøderbund RWTS18 six-sector track layout.</summary>
public sealed class AppleRwts18TrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.AppleRwts18;
    public override string DisplayName => FluxCodecDisplayNames.AppleRwts18;

    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors.OrderByDescending(sector => sector.Number))
        {
            if (sector.Number is < 0 or >= AppleRwts18Format.SectorCount || sector.Data.Count != AppleRwts18Format.SectorByteCount)
                throw AppleRwts18Format.InvalidTrackLayout(request.Sectors.Count);
            bits.Gap(sector.Number == AppleRwts18Format.LastSectorNumber ? AppleRwts18Format.FirstSectorGapBitCount : AppleRwts18Format.OtherSectorGapBitCount, true);
            bits.Raw((byte)(AppleRwts18Format.EncodedAddressMark >> BitPrimitives.BitsPerByte), (byte)(AppleRwts18Format.EncodedAddressMark & byte.MaxValue), AppleRwts18Format.NibbleTable[request.Cylinder & AppleRwts18Format.SixBitMask], AppleRwts18Format.NibbleTable[sector.Number],
                AppleRwts18Format.NibbleTable[(request.Cylinder ^ sector.Number) & AppleRwts18Format.SixBitMask], AppleRwts18Format.AddressTrailer, AppleRwts18Format.SyncByte, AppleRwts18Format.SyncByte);
            bits.Raw((byte)Attribute(request, AppleRwts18Format.IdentifierAttributeName, AppleRwts18Format.DefaultIdentifier));
            bits.Raw(EncodePayload(sector.Data));
            bits.Raw(AppleRwts18Format.DataEpilogue, AppleRwts18Format.SyncByte);
        }
        return bits;
    }

    private static byte[] EncodePayload(IReadOnlyList<byte> data)
    {
        var encoded = new byte[AppleRwts18Format.PayloadWithChecksumSymbolCount]; byte checksum = 0;
        for (var index = 0; index < AppleRwts18Format.PageByteCount; index++)
        {
            var one = data[index]; var two = data[AppleRwts18Format.PageByteCount + index]; var three = data[AppleRwts18Format.PageByteCount * 2 + index];
            var high = (byte)(((one >> 6) << 4) | ((two >> 6) << 2) | (three >> 6));
            var values = new[] { high, (byte)(one & 0x3f), (byte)(two & 0x3f), (byte)(three & 0x3f) };
            for (var valueIndex = 0; valueIndex < values.Length; valueIndex++)
            {
                var value = values[valueIndex];
                checksum ^= value;
                encoded[index * 4 + valueIndex] = AppleRwts18Format.NibbleTable[value];
            }
        }
        encoded[AppleRwts18Format.PayloadSymbolCount] = AppleRwts18Format.NibbleTable[checksum & AppleRwts18Format.SixBitMask];
        return encoded;
    }
}
