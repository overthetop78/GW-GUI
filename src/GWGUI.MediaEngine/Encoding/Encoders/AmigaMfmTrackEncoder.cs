namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Amiga MFM.</summary>
public sealed class AmigaMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => AmigaMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => AmigaMfmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != AmigaMfmFormat.SectorByteCount) throw AmigaMfmFormat.InvalidSectorSize(sector.Data.Count);
            byte[] info = [AmigaMfmFormat.FormatByte, (byte)(request.Cylinder << AmigaMfmFormat.TrackCylinderShift | request.Head & AmigaMfmFormat.TrackHeadMask), (byte)sector.Number, (byte)request.Sectors.Count];
            var headerAndLabel = AmigaMfmCodec.EncodeOddEven(info).Concat(new byte[AmigaMfmFormat.LabelByteCount]).ToArray();
            var headerParity = AmigaMfmCodec.CalculateParity(headerAndLabel, 0, headerAndLabel.Length);
            var data = AmigaMfmCodec.EncodeOddEven(sector.Data);
            var dataParity = AmigaMfmCodec.CalculateSplitParity(data, 0, data.Length);
            var encoded = headerAndLabel.Concat(new byte[] { 0,0,headerParity.High,headerParity.Low,0,0,dataParity.High,dataParity.Low }).Concat(data);
            bits.Gap(AmigaMfmFormat.LeadingGapBitCount);
            bits.Raw((byte)(AmigaMfmFormat.SyncWord >> 8), (byte)(AmigaMfmFormat.SyncWord & byte.MaxValue), (byte)(AmigaMfmFormat.SyncWord >> 8), (byte)(AmigaMfmFormat.SyncWord & byte.MaxValue));
            bits.Mfm(encoded);
            bits.Gap(AmigaMfmFormat.TrailingGapBitCount);
        }
        return bits;
    }
}
