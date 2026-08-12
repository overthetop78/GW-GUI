using GWGUI.MediaEngine.Primitives;
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
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != TycomFmFormat.SectorSize) throw TycomFmFormat.InvalidSectorSize(sector.Data.Count);
            var headerCrc = Primitives.Crc16Calculator.Compute([TycomFmFormat.HeaderAddressMark,(byte)request.Cylinder,(byte)sector.Number],TycomFmFormat.CrcPolynomial,TycomFmFormat.CrcInitialValue);
            bits.Raw(TycomFmFormat.HeaderMark.ToArray());
            bits.DoubleFm([(byte)request.Cylinder,(byte)sector.Number,(byte)(headerCrc >> BitPrimitives.BitsPerByte),(byte)headerCrc]);
            bits.Gap(TycomFmFormat.GapBitCount, true);
            var mark = sector.Deleted ? TycomFmFormat.DeletedDataMark : TycomFmFormat.DataMark;
            var dataCrc = Primitives.Crc16Calculator.Compute(new[] { mark }.Concat(sector.Data),TycomFmFormat.CrcPolynomial,TycomFmFormat.CrcInitialValue);
            bits.Raw(TycomFmFormat.DataMarks.Single(item=>item.Mark==mark).Pattern.ToArray());
            bits.DoubleFm(sector.Data.Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte),(byte)dataCrc]));
            bits.Gap(TycomFmFormat.GapBitCount, true);
        }
        return bits;
    }
}
