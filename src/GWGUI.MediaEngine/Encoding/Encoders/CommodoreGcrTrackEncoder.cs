using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Commodore GCR.</summary>
public sealed class CommodoreGcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.CommodoreGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.CommodoreGcr;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits(); var id2=(byte)Attribute(request,CommodoreGcrFormat.Id2AttributeName,CommodoreGcrFormat.DefaultId2); var id1=(byte)Attribute(request,CommodoreGcrFormat.Id1AttributeName,CommodoreGcrFormat.DefaultId1);
        var diskTrack=Attribute(request,CommodoreGcrFormat.TrackAttributeName,request.Cylinder+1+request.Head*CommodoreGcrFormat.TracksPerSide);
        foreach(var sector in request.Sectors)
        {
            if(sector.Data.Count!=CommodoreGcrFormat.SectorByteCount) throw CommodoreGcrFormat.InvalidSectorSize(sector.Data.Count);
            byte[] header=[CommodoreGcrFormat.HeaderMark,(byte)(sector.Number^diskTrack^id2^id1),(byte)sector.Number,(byte)diskTrack,id2,id1];
            byte checksum=0; foreach(var value in sector.Data) checksum^=value;
            bits.Gap(CommodoreGcrFormat.LeadingGapBitCount,true); bits.RawBits(new string('0', CommodoreGcrFormat.RawGapBitCount)); bits.Gap(CommodoreGcrFormat.SyncGapBitCount,true); Gcr(bits,header); bits.Gap(CommodoreGcrFormat.HeaderDataGapBitCount); bits.Gap(CommodoreGcrFormat.SyncGapBitCount,true);
            Gcr(bits,new byte[]{CommodoreGcrFormat.DataMark}.Concat(sector.Data).Append(checksum)); bits.Gap(CommodoreGcrFormat.TrailingGapBitCount);
        }
        return bits;
    }
    /// <summary>Exécute le traitement « GCR » propre à ce format.</summary>
    private static void Gcr(List<bool> bits,IEnumerable<byte> values)
    {
        foreach(var value in values) foreach(var nibble in new[]{value>>4,value&CommodoreGcrFormat.NibbleMask}) for(var bit=CommodoreGcrFormat.EncodedNibbleBitCount-1;bit>=0;bit--) bits.Add((CommodoreGcrFormat.EncodingTable[nibble]&(1<<bit))!=0);
    }
}
