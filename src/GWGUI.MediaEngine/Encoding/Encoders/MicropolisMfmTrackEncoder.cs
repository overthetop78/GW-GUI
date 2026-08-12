using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Micropolis MFM.</summary>
public sealed class MicropolisMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => MicropolisMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => MicropolisMfmFormat.CodecDisplayName;

    /// <summary>Encode les enregistrements sectoriels Micropolis avec leur adresse et leur contrôle.</summary>
    /// <param name="request">Piste logique contenant le cylindre et les secteurs à encoder.</param>
    /// <returns>Cellules MFM de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille Micropolis attendue.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != MicropolisMfmFormat.SectorSize) throw MicropolisMfmFormat.InvalidSectorSize(sector.Data.Count);
            if (sector.Number is < MicropolisMfmFormat.MinimumSectorNumber or > MicropolisMfmFormat.MaximumSectorNumber) throw MicropolisMfmFormat.InvalidSectorNumber(sector.Number);
            WriteRecord(bits, CreateRecord((byte)request.Cylinder, (byte)sector.Number, sector.Data));
        }
        return bits;
    }

    /// <summary>Construit un enregistrement Micropolis complet à partir de valeurs validées.</summary>
    /// <param name="cylinder">Numéro de cylindre.</param>
    /// <param name="sector">Numéro de secteur.</param>
    /// <param name="data">Données sectorielles.</param>
    /// <returns>Enregistrement prêt à encoder.</returns>
    private static MicropolisMfmRecord CreateRecord(byte cylinder, byte sector, IReadOnlyList<byte> data) => MicropolisMfmRecord.Create(cylinder, sector, data);

    /// <summary>Écrit le préambule nul, l'enregistrement MFM et le gap final.</summary>
    /// <param name="bits">Tampon recevant les cellules binaires.</param>
    /// <param name="record">Enregistrement à écrire.</param>
    private static void WriteRecord(List<bool> bits, MicropolisMfmRecord record)
    {
        bits.Mfm(new byte[MicropolisMfmFormat.PreambleByteCount]);
        bits.Mfm(record.Bytes);
        bits.Gap(MicropolisMfmFormat.GapBitCount);
    }
}
