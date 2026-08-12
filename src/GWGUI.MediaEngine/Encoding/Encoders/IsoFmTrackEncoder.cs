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
        var bits = TrackBitEncoding.Bits();
        ValidateAddress(nameof(request.Cylinder), request.Cylinder);
        ValidateAddress(nameof(request.Head), request.Head);
        foreach (var sector in request.Sectors)
        {
            ValidateAddress(nameof(sector.Number), sector.Number);
            var sizeCode = ResolveSizeCode(sector);
            WriteAddress(bits, request, sector.Number, sizeCode);
            WriteData(bits, sector);
        }
        return bits;
    }

    /// <summary>Calcule ou valide le code de taille associé aux données du secteur.</summary>
    private static byte ResolveSizeCode(TrackSector sector)
    {
        var sizeCode = sector.SizeCode ?? SectorSizeCode.FromByteCount(sector.Data.Count);
        if (IsoFmFormat.SectorSize(sizeCode) != sector.Data.Count) throw IsoFmFormat.InvalidSizeCode(sizeCode, sector.Data.Count);
        return sizeCode;
    }

    /// <summary>Écrit la marque spéciale, les champs CHRN, leur CRC fort puis faible et le gap d'adresse.</summary>
    private static void WriteAddress(List<bool> bits, TrackEncodeRequest request, int sector, byte sizeCode)
    {
        byte[] header = [IsoFmFormat.IdAddressMark, (byte)request.Cylinder, (byte)request.Head, (byte)sector, sizeCode];
        WriteField(bits, IsoFmFormat.EncodedIdAddressMark, header.Skip(1), Crc16Calculator.Compute(header, IsoFmFormat.CrcPolynomial, IsoFmFormat.CrcInitialValue), IsoFmFormat.HeaderGapBitCount);
    }

    /// <summary>Écrit la marque normale ou supprimée, les données, leur CRC et le gap final.</summary>
    private static void WriteData(List<bool> bits, TrackSector sector)
    {
        var mark = IsoFmFormat.DataMark(sector.Deleted);
        var crc = Crc16Calculator.Compute(new[] { mark.Mark }.Concat(sector.Data), IsoFmFormat.CrcPolynomial, IsoFmFormat.CrcInitialValue);
        WriteField(bits, mark.Pattern, sector.Data, crc, IsoFmFormat.DataGapBitCount);
    }

    /// <summary>Écrit un motif à horloge spéciale, son contenu, le CRC fort puis faible et son gap.</summary>
    private static void WriteField(List<bool> bits, ushort pattern, IEnumerable<byte> values, ushort crc, int gapBitCount)
    {
        AddMark(bits, pattern);
        bits.Fm(values.Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]));
        bits.Gap(gapBitCount);
    }

    /// <summary>Valide un champ CHRN avant sa conversion en octet.</summary>
    private static void ValidateAddress(string field, int value)
    {
        if (value is < 0 || value > IsoFmFormat.MaximumAddressValue) throw TrackEncodingExceptions.FormatValueOutOfRange("ISO FM", field, value, IsoFmFormat.MaximumAddressValue);
    }

    /// <summary>Ajoute une marque binaire au flux.</summary>
    private static void AddMark(List<bool> bits, ushort mark) => bits.Raw((byte)(mark >> BitPrimitives.BitsPerByte), (byte)mark);
}
