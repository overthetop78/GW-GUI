using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Heathkit FM.</summary>
public sealed class HeathkitFmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => HeathkitFmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => HeathkitFmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs Heathkit avec leur volume, leur adresse et leurs contrôles rotatifs.</summary>
    /// <param name="request">Piste logique contenant les secteurs et l'attribut de volume éventuel.</param>
    /// <returns>Cellules FM de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille Heathkit attendue.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        var volume = (byte)Attribute(request, HeathkitFmFormat.VolumeAttributeName, HeathkitFmFormat.DefaultVolume);
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != HeathkitFmFormat.SectorSize) throw HeathkitFmFormat.InvalidSectorSize(sector.Data.Count);
            byte[] identity = [volume, (byte)request.Cylinder, (byte)sector.Number];
            bits.Raw(HeathkitFmFormat.SectorMark.ToArray());
            bits.Fm(identity.Append(TrackEncoding.RotatingChecksum(identity)).Select(Primitives.BitPrimitives.ReverseBits));
            bits.Gap(HeathkitFmFormat.HeaderGapBitCount);
            bits.Raw(HeathkitFmFormat.SectorMark.ToArray());
            bits.Fm(sector.Data.Append(TrackEncoding.RotatingChecksum(sector.Data)).Select(Primitives.BitPrimitives.ReverseBits));
            bits.Gap(HeathkitFmFormat.DataGapBitCount);
        }
        return bits;
    }
}
