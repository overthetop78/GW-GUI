using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Micral NFM.</summary>
public sealed class MicralNFmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => MicralNFmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => MicralNFmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs Micral N avec leur adresse et leur somme de contrôle.</summary>
    /// <param name="request">Piste logique contenant le cylindre et les secteurs à encoder.</param>
    /// <returns>Cellules FM de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille Micral N attendue.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != MicralNFmFormat.SectorSize) throw MicralNFmFormat.InvalidSectorSize(sector.Data.Count);
            if (sector.Number is < MicralNFmFormat.MinimumSectorNumber or > MicralNFmFormat.MaximumSectorNumber) throw MicralNFmFormat.InvalidSectorNumber(sector.Number);
            WriteSector(bits, BuildSector((byte)sector.Number, (byte)request.Cylinder, sector.Data));
        }
        return bits;
    }

    /// <summary>Construit l'adresse, les données et le checksum d'un secteur Micral N.</summary>
    /// <param name="sector">Numéro de secteur validé.</param>
    /// <param name="cylinder">Numéro de cylindre validé.</param>
    /// <param name="data">Données sectorielles validées.</param>
    /// <returns>Octets FM situés après le préambule.</returns>
    private static byte[] BuildSector(byte sector, byte cylinder, IReadOnlyList<byte> data) => new byte[] { sector, cylinder }.Concat(data).Append(MicralNChecksum.Compute(data)).ToArray();

    /// <summary>Écrit le préambule, le bloc FM et le gap final d'un secteur Micral N.</summary>
    /// <param name="bits">Constructeur de cellules binaires de la piste.</param>
    /// <param name="sectorBytes">Adresse, données et checksum du secteur.</param>
    private static void WriteSector(List<bool> bits, IReadOnlyList<byte> sectorBytes)
    {
        bits.Raw(MicralNFmFormat.SectorMark.ToArray());
        bits.Fm(sectorBytes);
        bits.Gap(MicralNFmFormat.GapBitCount);
    }
}
