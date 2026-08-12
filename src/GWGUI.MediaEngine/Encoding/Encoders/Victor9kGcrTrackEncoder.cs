using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Victor9k GCR.</summary>
public sealed class Victor9kGcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => Victor9kGcrFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => Victor9kGcrFormat.CodecDisplayName;
    /// <summary>Encode les secteurs Victor 9000 avec leurs blocs d'en-tête et de données.</summary>
    /// <param name="request">Piste logique contenant le cylindre et les secteurs à encoder.</param>
    /// <returns>Cellules GCR de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille Victor 9000 attendue.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != Victor9kGcrFormat.SectorByteCount) throw Victor9kGcrFormat.InvalidSectorSize(sector.Data.Count);
            byte[] header = [Victor9kGcrFormat.HeaderType, (byte)request.Cylinder, (byte)sector.Number, (byte)(request.Cylinder + sector.Number), Victor9kGcrFormat.HeaderId2, Victor9kGcrFormat.HeaderId1];
            var checksum = Victor9kChecksum.Compute(sector.Data);
            AddBlock(bits, Victor9kGcrFormat.HeaderMark, header);
            bits.Gap(Victor9kGcrFormat.HeaderGapBitCount);
            AddBlock(bits, Victor9kGcrFormat.DataMark, new[] { Victor9kGcrFormat.DataPrefix }.Concat(sector.Data).Concat([(byte)checksum, (byte)(checksum >> 8)]));
            bits.Gap(Victor9kGcrFormat.DataGapBitCount);
        }
        return bits;
    }
    /// <summary>Ajoute un bloc Victor 9000 précédé de sa marque au tampon de piste.</summary>
    /// <param name="target">Tampon de cellules binaires recevant le bloc.</param>
    /// <param name="marker">Marque binaire placée avant le contenu encodé.</param>
    /// <param name="values">Octets du bloc à encoder en GCR.</param>
    private static void AddBlock(List<bool> target, IReadOnlyList<byte> marker, IEnumerable<byte> values)
    {
        var block = new List<bool>();
        block.Raw(marker.ToArray());
        CommodoreGcrCodec.Write(block, Victor9kGcrFormat.EncodedDataStartBitOffset, values, Victor9kGcrFormat.EncodedCellStride);
        target.AddRange(block);
    }
}
