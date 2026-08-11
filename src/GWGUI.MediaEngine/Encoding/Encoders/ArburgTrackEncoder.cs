using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Arburg.</summary>
public sealed class ArburgTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => ArburgFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => ArburgFormat.CodecDisplayName;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits();
        foreach(var sector in request.Sectors)
        {
            var system=Attribute(sector,ArburgFormat.SystemAttribute,0)!=0;
            var useful=system?ArburgFormat.SystemUsefulSize:ArburgFormat.DataUsefulSize; var total=system?ArburgFormat.SystemBlockSize:ArburgFormat.DataBlockSize;
            if(sector.Data.Count!=useful&&sector.Data.Count!=total) throw ArburgFormat.InvalidPayloadSize(system,sector.Data.Count);
            var data=sector.Data.Take(useful).ToArray(); ushort checksum=0; foreach(var value in data) checksum+=value;
            var block=data.Concat([(byte)checksum,(byte)(checksum>>8)]).Concat(Enumerable.Repeat((byte)0,total-useful-2));
            if(system)
            {
                bits.Raw(ArburgFormat.SystemMark.ToArray());
                foreach(var value in block) for(var bit=0;bit<8;bit++) bits.RawBits(((value>>bit)&1)!=0?"001":"01");
            }
            else { bits.Raw(ArburgFormat.DataMark.ToArray()); bits.DoubleFm(block.Select(Primitives.BitPrimitives.ReverseBits)); }
            bits.Gap(ArburgFormat.GapBitCount,true);
        }
        return bits;
    }
}
