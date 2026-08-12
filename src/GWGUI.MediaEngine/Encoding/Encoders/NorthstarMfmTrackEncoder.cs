using GWGUI.MediaEngine.Decoding.Definitions;

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
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != NorthstarMfmFormat.SectorSize) throw NorthstarMfmFormat.InvalidSectorSize(sector.Data.Count);
            bits.Raw(NorthstarMfmFormat.SectorMark.ToArray());
            bits.Mfm([NorthstarMfmAddress.Pack(request.Cylinder, sector.Number)]);
            bits.Mfm(sector.Data);
            bits.Mfm([TrackEncoding.RotatingChecksum(sector.Data)]);
            bits.Gap(NorthstarMfmFormat.GapBitCount);
        }
        return bits;
    }
}
