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
        ValidateAddress(nameof(request.Cylinder), request.Cylinder);
        ValidateAddress(nameof(request.Head), request.Head);
        foreach (var sector in request.Sectors)
        {
            ValidateAddress(nameof(sector.Number), sector.Number);
            var format = DecRx02Format.EncodingForSize(sector.Data.Count);
            if (sector.SizeCode is not null && sector.SizeCode != format.SizeCode) throw new ArgumentException($"RX02 size code {sector.SizeCode} does not match {sector.Data.Count} bytes.", nameof(sector));
            WriteAddress(bits, request, sector.Number, format.SizeCode);
            WriteData(bits, sector, DecRx02Format.DataMarkFor(format.Encoding, sector.Deleted));
        }
        return bits;
    }

    /// <summary>Écrit la marque, l'adresse, son CRC puis le gap commun.</summary>
    private static void WriteAddress(List<bool> bits, TrackEncodeRequest request, int sector, byte sizeCode)
    {
        var fields = new byte[] { DecRx02Format.HeaderAddressMark, (byte)request.Cylinder, (byte)request.Head, (byte)sector, sizeCode };
        var crc = Crc16Calculator.Compute(fields, DecRx02Format.CrcPolynomial, DecRx02Format.CrcInitialValue);
        bits.Raw(DecRx02Format.HeaderMark.ToArray());
        bits.DoubleFm(fields.Skip(1).Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]));
        bits.Gap(DecRx02Format.GapBitCount, true);
    }

    /// <summary>Écrit la marque et la charge utile FM ou M²FM avec son CRC.</summary>
    private static void WriteData(List<bool> bits, TrackSector sector, DecRx02DataMarkDefinition mark)
    {
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

    /// <summary>Valide une adresse avant sa conversion en octet.</summary>
    private static void ValidateAddress(string field, int value)
    {
        if (value is < 0 || value > DecRx02Format.MaximumAddressValue) throw TrackEncodingExceptions.FormatValueOutOfRange("DEC RX02", field, value, DecRx02Format.MaximumAddressValue);
    }
}
