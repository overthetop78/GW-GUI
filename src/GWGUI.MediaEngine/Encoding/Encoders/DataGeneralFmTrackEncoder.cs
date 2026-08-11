using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Data General 2F.</summary>
public sealed class DataGeneralFmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => DataGeneralFmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => DataGeneralFmFormat.CodecDisplayName;

    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    /// <param name="request">Piste et secteurs à encoder.</param>
    /// <returns>Cellules binaires produites.</returns>
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
            var checksum = DataGeneralChecksum.Calculate(sector.Data);
            bits.Fm(sector.Data.Concat([(byte)(checksum >> BitPrimitives.BitsPerByte), (byte)checksum]));
            bits.Gap(DataGeneralFmFormat.DataGapBitCount);
        }
        return bits;
    }
}
