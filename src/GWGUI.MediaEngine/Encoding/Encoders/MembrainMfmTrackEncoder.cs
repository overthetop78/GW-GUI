using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Membrain MFM.</summary>
public sealed class MembrainMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => MembrainMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => MembrainMfmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs Membrain avec leur adresse compacte et leurs CRC.</summary>
    /// <param name="request">Piste logique contenant cylindre, face et secteurs à encoder.</param>
    /// <returns>Cellules MFM de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille Membrain attendue.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        ValidateAddress(nameof(request.Cylinder), request.Cylinder, MembrainMfmFormat.MaximumCylinder);
        ValidateAddress(nameof(request.Head), request.Head, MembrainMfmFormat.MaximumHead);
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != MembrainMfmFormat.SectorSize) throw MembrainMfmFormat.InvalidSectorSize(sector.Data.Count);
            ValidateAddress(nameof(sector.Number), sector.Number, MembrainMfmFormat.MaximumSector);
            var (cylinderHigh, packed) = MembrainMfmAddress.Pack(request.Cylinder, request.Head, sector.Number);
            WriteHeader(bits, cylinderHigh, packed);
            WriteData(bits, sector.Data);
        }
        return bits;
    }

    /// <summary>Construit l'en-tête logique, écrit son motif spécial, ses deux octets d'adresse, son CRC et son gap.</summary>
    private static void WriteHeader(List<bool> bits, byte cylinderHigh, byte packedAddress)
    {
        byte[] header = [MembrainMfmFormat.SyncByte, MembrainMfmFormat.HeaderAddressMark, cylinderHigh, packedAddress];
        var crc = Crc16Calculator.Compute(header, MembrainMfmFormat.CrcPolynomial, MembrainMfmFormat.CrcInitialValue);
        bits.Raw(MembrainMfmFormat.HeaderPattern.ToArray());
        bits.Mfm([cylinderHigh, packedAddress, (byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]);
        bits.Gap(MembrainMfmFormat.HeaderGapBitCount);
    }

    /// <summary>Écrit le motif de données, la charge utile, son CRC fort puis faible et le gap final.</summary>
    private static void WriteData(List<bool> bits, IReadOnlyList<byte> data)
    {
        var crc = Crc16Calculator.Compute(new[] { MembrainMfmFormat.SyncByte, MembrainMfmFormat.DataAddressMark }.Concat(data), MembrainMfmFormat.CrcPolynomial, MembrainMfmFormat.CrcInitialValue);
        bits.Raw(MembrainMfmFormat.DataPattern.ToArray());
        bits.Mfm(data.Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]));
        bits.Gap(MembrainMfmFormat.DataGapBitCount);
    }

    /// <summary>Valide une composante avant son empaquetage dans l'adresse Membrain.</summary>
    private static void ValidateAddress(string field, int value, int maximum)
    {
        if (value is < 0 || value > maximum) throw TrackEncodingExceptions.FormatValueOutOfRange("Membrain MFM", field, value, maximum);
    }
}
