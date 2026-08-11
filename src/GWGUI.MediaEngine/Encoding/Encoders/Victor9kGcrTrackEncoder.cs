using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Victor9k GCR.</summary>
public sealed class Victor9kGcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id=>FluxCodecIds.Victor9kGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName=>FluxCodecDisplayNames.Victor9kGcr;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits();
        foreach(var sector in request.Sectors)
        {
            if(sector.Data.Count!=Victor9kGcrFormat.SectorByteCount) throw Victor9kGcrFormat.InvalidSectorSize(sector.Data.Count);
            byte[] header=[Victor9kGcrFormat.HeaderType,(byte)request.Cylinder,(byte)sector.Number,(byte)(request.Cylinder+sector.Number),Victor9kGcrFormat.HeaderId2,Victor9kGcrFormat.HeaderId1];
            ushort checksum=0; foreach(var value in sector.Data) checksum+=value;
            AddBlock(bits,Victor9kGcrFormat.HeaderMarkHex,header); bits.Gap(Victor9kGcrFormat.HeaderGapBitCount);
            AddBlock(bits,Victor9kGcrFormat.DataMarkHex,new byte[]{0}.Concat(sector.Data).Concat([(byte)checksum,(byte)(checksum>>8)])); bits.Gap(Victor9kGcrFormat.DataGapBitCount);
        }
        return bits;
    }
    /// <summary>Exécute le traitement « Add Block » propre à ce format.</summary>
    private static void AddBlock(List<bool> target,string markerHex,IEnumerable<byte> values)
    {
        var marker=new List<bool>(); marker.RawHex(markerHex); var encoded=new List<bool>();
        foreach(var value in values) foreach(var nibble in new[]{value>>4,value&Victor9kGcrFormat.NibbleMask}) for(var bit=Victor9kGcrFormat.EncodedNibbleBitCount-1;bit>=0;bit--) encoded.Add((Victor9kGcrFormat.EncodingTable[nibble]&(1<<bit))!=0);
        while(marker.Count<Victor9kGcrFormat.EncodedDataStartBitOffset+encoded.Count*Victor9kGcrFormat.EncodedCellStride) marker.Add(false);
        for(var index=0;index<encoded.Count;index++) marker[Victor9kGcrFormat.EncodedDataStartBitOffset+index*Victor9kGcrFormat.EncodedCellStride]=encoded[index];
        target.AddRange(marker);
    }
}
