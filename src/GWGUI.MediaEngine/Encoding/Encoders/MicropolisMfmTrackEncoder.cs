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
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != MicropolisMfmFormat.SectorSize) throw MicropolisMfmFormat.InvalidSectorSize(sector.Data.Count);
            var record = MicropolisMfmRecord.Create((byte)request.Cylinder, (byte)sector.Number, sector.Data);
            bits.Mfm(new byte[MicropolisMfmFormat.PreambleByteCount]);
            bits.Mfm(record.Bytes);
            bits.Gap(MicropolisMfmFormat.GapBitCount);
        }
        return bits;
    }
}
