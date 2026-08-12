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
            var checksum = MicralNChecksum.Compute(sector.Data);
            bits.Raw(MicralNFmFormat.SectorMark.ToArray());
            bits.Fm(new byte[] { (byte)sector.Number, (byte)request.Cylinder }.Concat(sector.Data).Append(checksum));
            bits.Gap(MicralNFmFormat.GapBitCount);
        }
        return bits;
    }
}
