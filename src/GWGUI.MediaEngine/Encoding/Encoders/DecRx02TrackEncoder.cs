using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format DEC RX02.</summary>
public sealed class DecRx02TrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => DecRx02Format.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => DecRx02Format.CodecDisplayName;

    /// <summary>Encode une piste DEC RX01 ou RX02 en fonction de la taille de chaque secteur.</summary>
    /// <param name="request">Piste logique contenant cylindre, face et secteurs FM ou M²FM.</param>
    /// <returns>Cellules FM et M²FM de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède aucune des tailles DEC RX admises.</exception>
    /// <remarks>Les secteurs RX02 utilisent une marque FM suivie d'une charge utile transformée en M²FM.</remarks>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            var m2fm = sector.Data.Count == DecRx02Format.M2FmSectorByteCount;
            if (!m2fm && sector.Data.Count != DecRx02Format.FmSectorByteCount) throw DecRx02Format.InvalidSectorSize(sector.Data.Count);
            var sizeCode = sector.SizeCode ?? (m2fm ? DecRx02Format.M2FmSectorSizeCode : DecRx02Format.FmSectorSizeCode);
            var headerCrc = Crc16Calculator.Compute([DecRx02Format.HeaderAddressMark, (byte)request.Cylinder, (byte)request.Head, (byte)sector.Number, sizeCode], DecRx02Format.CrcPolynomial, DecRx02Format.CrcInitialValue);
            bits.Raw(DecRx02Format.HeaderMark.ToArray());
            bits.DoubleFm([(byte)request.Cylinder, (byte)request.Head, (byte)sector.Number, sizeCode, (byte)(headerCrc >> BitPrimitives.BitsPerByte), (byte)headerCrc]);
            bits.Gap(DecRx02Format.GapBitCount, true);
            var markValue = m2fm ? (sector.Deleted ? DecRx02Format.M2FmDeletedDataMark : DecRx02Format.M2FmDataMark) : (sector.Deleted ? DecRx02Format.FmDeletedDataMark : DecRx02Format.FmDataMark);
            var mark = DecRx02Format.DataMarks.Single(definition => definition.Mark == markValue);
            bits.Raw(mark.Pattern.ToArray());
            var crc = Crc16Calculator.Compute(new[] { mark.Mark }.Concat(sector.Data), DecRx02Format.CrcPolynomial, DecRx02Format.CrcInitialValue);
            var payload = sector.Data.Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]).ToArray();
            if (mark.Encoding == DecRx02DataEncoding.M2Fm)
            {
                bits.Add(false);
                var encoded = TrackBitEncoding.Bits();
                encoded.Mfm(payload);
                DecRx02M2FmCodec.Encode(encoded);
                bits.AddRange(encoded);
            }
            else bits.DoubleFm(payload);
            bits.Gap(DecRx02Format.GapBitCount, true);
        }
        return bits;
    }
}
