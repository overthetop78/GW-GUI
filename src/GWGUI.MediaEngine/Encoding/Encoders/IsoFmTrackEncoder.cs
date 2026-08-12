using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format ISO FM.</summary>
public sealed class IsoFmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique.</summary>
    public override string Id => IsoFmFormat.CodecId;
    /// <summary>Obtient le nom affiché.</summary>
    public override string DisplayName => IsoFmFormat.CodecDisplayName;

    /// <summary>Encode une piste ISO FM avec marques d'adresse, tailles sectorielles et CRC.</summary>
    /// <param name="request">Piste logique contenant cylindre, face et secteurs à encoder.</param>
    /// <returns>Cellules FM de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentOutOfRangeException">La taille d'un secteur ne correspond à aucun code ISO pris en charge.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            var sizeCode = sector.SizeCode ?? TrackEncoding.SizeCode(sector.Data.Count);
            byte[] header = [IsoFmFormat.IdAddressMark, (byte)request.Cylinder, (byte)request.Head, (byte)sector.Number, sizeCode];
            var headerCrc = Crc16Calculator.Compute(header, IsoFmFormat.CrcPolynomial, IsoFmFormat.CrcInitialValue);
            AddMark(bits, IsoFmFormat.EncodedIdAddressMark);
            bits.Fm(header.Skip(1).Concat([(byte)(headerCrc >> BitPrimitives.BitsPerByte), (byte)headerCrc]));
            bits.Gap(IsoFmFormat.HeaderGapBitCount);
            var definition = IsoFmFormat.Marks.Single(mark => mark.Deleted == sector.Deleted && mark.Mark != IsoFmFormat.IdAddressMark);
            var dataCrc = Crc16Calculator.Compute(new[] { definition.Mark }.Concat(sector.Data), IsoFmFormat.CrcPolynomial, IsoFmFormat.CrcInitialValue);
            AddMark(bits, definition.Pattern);
            bits.Fm(sector.Data.Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte), (byte)dataCrc]));
            bits.Gap(IsoFmFormat.DataGapBitCount);
        }
        return bits;
    }

    /// <summary>Ajoute une marque binaire au flux.</summary>
    private static void AddMark(List<bool> bits, ushort mark) => bits.Raw((byte)(mark >> BitPrimitives.BitsPerByte), (byte)mark);
}
