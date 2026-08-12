using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Northstar MFM.</summary>
public sealed class NorthstarMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => NorthstarMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => NorthstarMfmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs NorthStar avec leur adresse compacte et leur contrôle rotatif.</summary>
    /// <param name="request">Piste logique contenant le cylindre et les secteurs à encoder.</param>
    /// <returns>Cellules MFM de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille NorthStar attendue.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != NorthstarMfmFormat.SectorSize) throw NorthstarMfmFormat.InvalidSectorSize(sector.Data.Count);
            WriteSector(bits, EncodeAddress(request.Cylinder, sector.Number), sector.Data, RotatingChecksumCalculator.Compute(sector.Data));
        }
        return bits;
    }

    /// <summary>Valide puis compose l'adresse compacte d'un secteur NorthStar.</summary>
    /// <param name="cylinder">Numéro de cylindre.</param>
    /// <param name="sector">Numéro de secteur.</param>
    /// <returns>Adresse composée des deux demi-octets.</returns>
    private static byte EncodeAddress(int cylinder, int sector)
    {
        if (cylinder is < NorthstarMfmFormat.MinimumCylinder or > NorthstarMfmFormat.MaximumCylinder) throw NorthstarMfmFormat.InvalidCylinder(cylinder);
        if (sector is < NorthstarMfmFormat.MinimumSector or > NorthstarMfmFormat.MaximumSector) throw NorthstarMfmFormat.InvalidSector(sector);
        return NorthstarMfmAddress.Pack(cylinder, sector);
    }

    /// <summary>Écrit la marque, l'adresse, les données, le checksum et le gap final d'un secteur.</summary>
    /// <param name="bits">Tampon recevant les cellules binaires.</param>
    /// <param name="address">Adresse compacte validée.</param>
    /// <param name="data">Données sectorielles validées.</param>
    /// <param name="checksum">Checksum rotatif des données.</param>
    private static void WriteSector(List<bool> bits, byte address, IReadOnlyList<byte> data, byte checksum)
    {
        bits.Raw(NorthstarMfmFormat.SectorMark.ToArray());
        bits.Mfm([address]);
        bits.Mfm(data);
        bits.Mfm([checksum]);
        bits.Gap(NorthstarMfmFormat.GapBitCount);
    }
}
