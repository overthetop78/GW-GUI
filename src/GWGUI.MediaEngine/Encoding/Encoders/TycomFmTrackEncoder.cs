using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Tycom FM.</summary>
public sealed class TycomFmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => TycomFmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => TycomFmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs TYCOM avec leurs marques FM doublées et leurs CRC.</summary>
    /// <param name="request">Piste logique contenant le cylindre et les secteurs à encoder.</param>
    /// <returns>Cellules FM doublées de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille TYCOM attendue.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != TycomFmFormat.SectorSize) throw TycomFmFormat.InvalidSectorSize(sector.Data.Count);
            if (request.Cylinder is < TycomFmFormat.MinimumCylinder or > TycomFmFormat.MaximumCylinder) throw TycomFmFormat.InvalidCylinder(request.Cylinder);
            if (sector.Number is < TycomFmFormat.MinimumSector or > TycomFmFormat.MaximumSector) throw TycomFmFormat.InvalidSector(sector.Number);
            WriteHeader(bits, (byte)request.Cylinder, (byte)sector.Number);
            WriteData(bits, TycomFmFormat.SelectDataMark(sector.Deleted), sector.Data);
        }
        return bits;
    }

    /// <summary>Écrit le motif d'adresse, le cylindre, le secteur, leur CRC et le premier gap.</summary>
    /// <param name="bits">Tampon recevant les cellules binaires.</param>
    /// <param name="cylinder">Cylindre validé.</param>
    /// <param name="sector">Secteur validé.</param>
    private static void WriteHeader(List<bool> bits, byte cylinder, byte sector)
    {
        bits.Raw(TycomFmFormat.HeaderMark.ToArray());
        bits.DoubleFm(new[] { cylinder, sector }.Concat(TycomFmCrc.ToBigEndianBytes(TycomFmCrc.ComputeHeader(cylinder, sector))));
        bits.Gap(TycomFmFormat.GapBitCount, true);
    }

    /// <summary>Écrit le motif de données, la charge utile, son CRC et le second gap.</summary>
    /// <param name="bits">Tampon recevant les cellules binaires.</param>
    /// <param name="mark">Marque de données et motif physique cohérents.</param>
    /// <param name="data">Données sectorielles validées.</param>
    private static void WriteData(List<bool> bits, TycomFmMarkDefinition mark, IReadOnlyList<byte> data)
    {
        bits.Raw(mark.Pattern.ToArray());
        bits.DoubleFm(data.Concat(TycomFmCrc.ToBigEndianBytes(TycomFmCrc.ComputeData(mark.Mark, data))));
        bits.Gap(TycomFmFormat.GapBitCount, true);
    }
}
