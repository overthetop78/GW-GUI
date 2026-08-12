using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format ISO MFM.</summary>
public sealed class IsoMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique.</summary>
    public override string Id => IsoMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché.</summary>
    public override string DisplayName => IsoMfmFormat.CodecDisplayName;

    /// <summary>Encode une piste ISO MFM avec marques de synchronisation, tailles sectorielles et CRC.</summary>
    /// <param name="request">Piste logique contenant cylindre, face et secteurs à encoder.</param>
    /// <returns>Cellules MFM de la piste dans leur ordre d'émission.</returns>
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

    /// <summary>Calcule ou valide le code de taille sectorielle.</summary>
    private static byte ResolveSizeCode(TrackSector sector)
    {
        var sizeCode = sector.SizeCode ?? SectorSizeCode.FromByteCount(sector.Data.Count);
        if (IsoMfmFormat.SectorSize(sizeCode) != sector.Data.Count) throw IsoMfmFormat.InvalidSizeCode(sizeCode, sector.Data.Count);
        return sizeCode;
    }

    /// <summary>Écrit les synchronisations spéciales, l'adresse CHRN, son CRC et le gap.</summary>
    private static void WriteAddress(List<bool> bits, TrackEncodeRequest request, int sector, byte sizeCode)
    {
        byte[] values = [IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.IdAddressMark, (byte)request.Cylinder, (byte)request.Head, (byte)sector, sizeCode];
        WriteField(bits, values.Skip(IsoMfmFormat.SyncByteCount), Crc16Calculator.Compute(values, IsoMfmFormat.CrcPolynomial, IsoMfmFormat.CrcInitialValue), IsoMfmFormat.HeaderGapBitCount);
    }

    /// <summary>Écrit les synchronisations spéciales, la marque de données, la charge utile, son CRC et le gap.</summary>
    private static void WriteData(List<bool> bits, TrackSector sector)
    {
        var mark = IsoMfmFormat.DataMark(sector.Deleted);
        var values = Enumerable.Repeat(IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByteCount).Append(mark).Concat(sector.Data).ToArray();
        WriteField(bits, values.Skip(IsoMfmFormat.SyncByteCount), Crc16Calculator.Compute(values, IsoMfmFormat.CrcPolynomial, IsoMfmFormat.CrcInitialValue), IsoMfmFormat.DataGapBitCount);
    }

    /// <summary>Écrit une synchronisation spéciale unique, son contenu MFM, le CRC fort puis faible et le gap.</summary>
    private static void WriteField(List<bool> bits, IEnumerable<byte> values, ushort crc, int gapBitCount)
    {
        bits.Raw(IsoMfmFormat.EncodedSync.ToArray());
        bits.Mfm(values.Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]));
        bits.Gap(gapBitCount);
    }

    /// <summary>Valide un champ CHRN avant sa conversion en octet.</summary>
    private static void ValidateAddress(string field, int value)
    {
        if (value is < 0 || value > IsoMfmFormat.MaximumAddressValue) throw TrackEncodingExceptions.FormatValueOutOfRange("ISO MFM", field, value, IsoMfmFormat.MaximumAddressValue);
    }
}
