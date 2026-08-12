using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Centurion MFM.</summary>
public sealed class CenturionMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => CenturionMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => CenturionMfmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs d'une piste Centurion avec leurs en-têtes, blocs et CRC.</summary>
    /// <param name="request">Piste logique contenant le cylindre et les secteurs à encoder.</param>
    /// <returns>Cellules MFM de la piste dans leur ordre d'émission.</returns>
    /// <remarks>Les charges utiles sont complétées par blocs d'allocation avant le calcul du CRC.</remarks>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        ValidateValue(nameof(request.Cylinder), request.Cylinder, CenturionMfmFormat.MaximumAddressValue);
        foreach (var sector in request.Sectors)
        {
            ValidateValue(nameof(sector.Number), sector.Number, CenturionMfmFormat.MaximumAddressValue);
            bits.Raw(CenturionMfmFormat.SectorMark.ToArray());
            bits.Mfm(BuildHeader((byte)request.Cylinder, (byte)sector.Number));
            bits.Gap(CenturionMfmFormat.HeaderGapBitCount);
            bits.Raw(CenturionMfmFormat.DataMark.ToArray());
            bits.Mfm(BuildDataField(sector.Data));
            bits.Gap(CenturionMfmFormat.DataGapBitCount);
        }
        return bits;
    }

    /// <summary>Construit l'identité du secteur suivie de son CRC Centurion.</summary>
    private static byte[] BuildHeader(byte cylinder, byte sector)
    {
        byte[] identity = [cylinder, sector];
        var crc = Crc16Calculator.Compute(identity, CenturionMfmFormat.CrcPolynomial, CenturionMfmFormat.CrcInitialValue);
        return identity.Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]).ToArray();
    }

    /// <summary>Complète la charge utile à un nombre entier de blocs, ajoute le préfixe et le CRC.</summary>
    private static byte[] BuildDataField(IReadOnlyList<byte> data)
    {
        var blockCount = Math.Max(CenturionMfmFormat.MinimumAllocationBlockCount, (data.Count + CenturionMfmFormat.AllocationBlockSize - 1) / CenturionMfmFormat.AllocationBlockSize);
        ValidateValue(nameof(blockCount), blockCount, CenturionMfmFormat.MaximumAllocationBlockCount);
        var payload = data.Concat(Enumerable.Repeat(CenturionMfmFormat.PaddingByte, blockCount * CenturionMfmFormat.AllocationBlockSize - data.Count)).ToArray();
        var crcInput = new byte[] { (byte)blockCount, CenturionMfmFormat.SupportedDataKey }.Concat(payload).ToArray();
        var crc = Crc16Calculator.Compute(crcInput, CenturionMfmFormat.CrcPolynomial, CenturionMfmFormat.CrcInitialValue);
        return new byte[] { CenturionMfmFormat.ReservedDataPrefixByte, (byte)blockCount, CenturionMfmFormat.SupportedDataKey }.Concat(payload).Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]).ToArray();
    }

    /// <summary>Valide une valeur avant son écriture dans un champ Centurion sur un octet.</summary>
    private static void ValidateValue(string field, int value, int maximum)
    {
        if (value is < 0 || value > maximum) throw TrackEncodingExceptions.FormatValueOutOfRange("Centurion MFM", field, value, maximum);
    }
}
