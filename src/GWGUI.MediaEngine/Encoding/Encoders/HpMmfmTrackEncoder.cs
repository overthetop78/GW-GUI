using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format HP MMFM.</summary>
public sealed class HpMmfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => HpMmfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => HpMmfmFormat.CodecDisplayName;

    /// <summary>Encode les secteurs HP avec leur adresse, leur charge utile transformée et leurs CRC.</summary>
    /// <param name="request">Piste logique contenant cylindre, face et secteurs à encoder.</param>
    /// <returns>Cellules MMFM de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille HP attendue.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        ValidateAddress(nameof(request.Cylinder), request.Cylinder, HpMmfmFormat.MaximumCylinder);
        ValidateAddress(nameof(request.Head), request.Head, HpMmfmFormat.MaximumHead);
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != HpMmfmFormat.SectorSize) throw HpMmfmFormat.InvalidSectorSize(sector.Data.Count);
            ValidateAddress(nameof(sector.Number), sector.Number, HpMmfmFormat.MaximumSector);
            WriteField(bits, HpMmfmFormat.SectorSync, BuildIdentity(request.Cylinder, request.Head, sector.Number), HpMmfmFormat.HeaderGapBitCount);
            WriteField(bits, HpMmfmFormat.DataSync, HpMmfmCodec.EncodePayload(sector.Data), HpMmfmFormat.DataGapBitCount);
        }
        return bits;
    }

    /// <summary>Compose le secteur et la face puis inverse les deux octets de l'identité HP.</summary>
    private static byte[] BuildIdentity(int cylinder, int head, int sector)
    {
        var encodedSector = (byte)(sector | head << HpMmfmFormat.HeadShift);
        return [BitPrimitives.ReverseBits((byte)cylinder), BitPrimitives.ReverseBits(encodedSector)];
    }

    /// <summary>Écrit une synchronisation, les valeurs protégées par CRC et le gap associé.</summary>
    private static void WriteField(List<bool> bits, IReadOnlyList<byte> sync, IReadOnlyList<byte> values, int gapBitCount)
    {
        bits.Raw(sync.ToArray());
        bits.Mfm(Crc16Calculator.Append(values));
        bits.Gap(gapBitCount);
    }

    /// <summary>Valide une composante de l'adresse HP avant son empaquetage.</summary>
    private static void ValidateAddress(string field, int value, int maximum)
    {
        if (value is < 0 || value > maximum) throw TrackEncodingExceptions.FormatValueOutOfRange("HP MMFM", field, value, maximum);
    }
}
