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
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            var sizeCode = sector.SizeCode ?? TrackEncoding.SizeCode(sector.Data.Count);
            byte[] header = [IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.IdAddressMark, (byte)request.Cylinder, (byte)request.Head, (byte)sector.Number, sizeCode];
            var headerCrc = Crc16Calculator.Compute(header, IsoMfmFormat.CrcPolynomial, IsoMfmFormat.CrcInitialValue);
            bits.RawHex(IsoMfmFormat.EncodedSyncHex);
            bits.Mfm(header.Skip(IsoMfmFormat.SyncByteCount).Concat([(byte)(headerCrc >> BitPrimitives.BitsPerByte), (byte)headerCrc]));
            bits.Gap(IsoMfmFormat.HeaderGapBitCount);
            var mark = sector.Deleted ? IsoMfmFormat.DeletedDataAddressMark : IsoMfmFormat.DataAddressMark;
            var dataCrc = Crc16Calculator.Compute(new[] { IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, mark }.Concat(sector.Data), IsoMfmFormat.CrcPolynomial, IsoMfmFormat.CrcInitialValue);
            bits.RawHex(IsoMfmFormat.EncodedSyncHex);
            bits.Mfm(new[] { mark }.Concat(sector.Data).Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte), (byte)dataCrc]));
            bits.Gap(IsoMfmFormat.DataGapBitCount);
        }
        return bits;
    }
}
