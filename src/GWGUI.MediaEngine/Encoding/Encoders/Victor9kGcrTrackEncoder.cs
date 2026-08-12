using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

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
        var bits = TrackBitEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != Victor9kGcrFormat.SectorByteCount) throw Victor9kGcrFormat.InvalidSectorSize(sector.Data.Count);
            if (request.Cylinder is < Victor9kGcrFormat.MinimumCylinder or > Victor9kGcrFormat.MaximumCylinder) throw Victor9kGcrFormat.InvalidCylinder(request.Cylinder);
            if (sector.Number is < Victor9kGcrFormat.MinimumSector or > Victor9kGcrFormat.MaximumSector) throw Victor9kGcrFormat.InvalidSector(sector.Number);
            AddBlock(bits, Victor9kGcrFormat.HeaderMark, BuildHeader((byte)request.Cylinder, (byte)sector.Number));
            bits.Gap(Victor9kGcrFormat.HeaderGapBitCount);
            AddBlock(bits, Victor9kGcrFormat.DataMark, BuildData(sector.Data));
            bits.Gap(Victor9kGcrFormat.DataGapBitCount);
        }
        return bits;
    }

    /// <summary>Construit les six octets d'un en-tête Victor 9000.</summary>
    /// <param name="cylinder">Cylindre validé.</param>
    /// <param name="sector">Secteur validé.</param>
    /// <returns>En-tête complet.</returns>
    private static byte[] BuildHeader(byte cylinder, byte sector) => [Victor9kGcrFormat.HeaderType, cylinder, sector, Victor9kHeaderChecksum.Compute(cylinder, sector), Victor9kGcrFormat.HeaderId2, Victor9kGcrFormat.HeaderId1];

    /// <summary>Construit le préfixe, les données et le checksum petit-boutiste d'un bloc.</summary>
    /// <param name="data">Données sectorielles validées.</param>
    /// <returns>Octets du bloc de données.</returns>
    private static IEnumerable<byte> BuildData(IReadOnlyList<byte> data) => new[] { Victor9kGcrFormat.DataPrefix }.Concat(data).Concat(Victor9kChecksum.ToLittleEndianBytes(Victor9kChecksum.Compute(data)));

    /// <summary>Ajoute un bloc Victor 9000 précédé de sa marque au tampon de piste.</summary>
    /// <param name="target">Tampon de cellules binaires recevant le bloc.</param>
    /// <param name="marker">Marque binaire placée avant le contenu encodé.</param>
    /// <param name="values">Octets du bloc à encoder en GCR.</param>
    internal static void AddBlock(List<bool> target, IReadOnlyList<byte> marker, IEnumerable<byte> values)
    {
        var block = PrepareMarker(marker);
        InsertGcr(block, values);
        target.AddRange(block);
    }

    /// <summary>Valide puis copie le marqueur dans un nouveau tampon de bloc.</summary>
    private static List<bool> PrepareMarker(IReadOnlyList<byte> marker)
    {
        if (marker.Count * BitPrimitives.BitsPerByte < Victor9kGcrFormat.EncodedDataStartBitOffset) throw Victor9kGcrFormat.InvalidMarkerLength(marker.Count * BitPrimitives.BitsPerByte);
        var block = new List<bool>();
        block.Raw(marker.ToArray());
        return block;
    }

    /// <summary>Insère les cellules GCR aux positions Victor 9000 définies.</summary>
    /// <param name="block">Bloc contenant déjà le marqueur.</param>
    /// <param name="values">Octets à encoder.</param>
    private static void InsertGcr(IList<bool> block, IEnumerable<byte> values) => CommodoreGcrCodec.Write(block, Victor9kGcrFormat.EncodedDataStartBitOffset, values, Victor9kGcrFormat.EncodedCellStride);
}
