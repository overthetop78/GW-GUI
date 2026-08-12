using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Arburg.</summary>
public sealed class ArburgTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => ArburgFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => ArburgFormat.CodecDisplayName;
    /// <summary>Encode les blocs système ou de données d'une piste Arburg.</summary>
    /// <param name="request">Piste logique et attribut indiquant la nature système de chaque secteur.</param>
    /// <returns>Cellules binaires de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas une taille Arburg admise.</exception>
    /// <remarks>Chaque bloc est complété avec son contrôle avant l'encodage des cellules.</remarks>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits=TrackEncoding.Bits();
        foreach(var sector in request.Sectors)
        {
            var system=Attribute(sector,ArburgFormat.SystemAttribute,0)!=0;
            var useful=system?ArburgFormat.SystemUsefulSize:ArburgFormat.DataUsefulSize; var total=system?ArburgFormat.SystemBlockSize:ArburgFormat.DataBlockSize;
            if(sector.Data.Count!=useful&&sector.Data.Count!=total) throw ArburgFormat.InvalidPayloadSize(system,sector.Data.Count);
            var data=sector.Data.Take(useful).ToArray();
            var block=ArburgChecksum.CreateBlock(data,total);
            if(system)
            {
                bits.Raw(ArburgFormat.SystemMark.ToArray());
                bits.AddRange(ArburgSystemCodec.Encode(block));
            }
            else { bits.Raw(ArburgFormat.DataMark.ToArray()); bits.DoubleFm(block.Select(Primitives.BitPrimitives.ReverseBits)); }
            bits.Gap(ArburgFormat.GapBitCount,true);
        }
        return bits;
    }
}
