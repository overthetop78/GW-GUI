using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

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
        var bits = TrackBitEncoding.Bits();
        var volumeValue = Attribute(request, HeathkitFmFormat.VolumeAttributeName, HeathkitFmFormat.DefaultVolume);
        ValidateAddress(HeathkitFmFormat.VolumeAttributeName, volumeValue);
        ValidateAddress(nameof(request.Cylinder), request.Cylinder);
        var volume = (byte)volumeValue;
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != HeathkitFmFormat.SectorSize) throw HeathkitFmFormat.InvalidSectorSize(sector.Data.Count);
            ValidateAddress(nameof(sector.Number), sector.Number);
            WriteRecord(bits, [volume, (byte)request.Cylinder, (byte)sector.Number], HeathkitFmFormat.HeaderGapBitCount);
            WriteRecord(bits, sector.Data, HeathkitFmFormat.DataGapBitCount);
        }
        return bits;
    }

    /// <summary>Écrit le préambule, les octets protégés puis le gap demandé.</summary>
    private static void WriteRecord(List<bool> bits, IReadOnlyList<byte> values, int gapBitCount)
    {
        bits.Raw(HeathkitFmFormat.SectorMark.ToArray());
        bits.Fm(HeathkitFmCodec.EncodeRecord(values));
        bits.Gap(gapBitCount);
    }

    /// <summary>Valide une adresse avant sa conversion en octet.</summary>
    private static void ValidateAddress(string field, int value)
    {
        if (value is < 0 || value > HeathkitFmFormat.MaximumAddressValue) throw TrackEncodingExceptions.FormatValueOutOfRange("Heathkit FM", field, value, HeathkitFmFormat.MaximumAddressValue);
    }
}
