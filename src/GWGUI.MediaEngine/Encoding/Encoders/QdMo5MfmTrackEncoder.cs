using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Qd Mo5 MFM.</summary>
public sealed class QdMo5MfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => QdMo5MfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => QdMo5MfmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs QD MO5 avec leur préfixe, leur adresse et leurs CRC.</summary>
    /// <param name="request">Piste logique contenant les secteurs et leurs attributs de préfixe éventuels.</param>
    /// <returns>Cellules MFM de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille QD MO5 attendue.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != QdMo5MfmFormat.SectorSize) throw QdMo5MfmFormat.InvalidSectorSize(sector.Data.Count);
            var address = EncodeSectorNumber(sector.Number);
            var prefixValue = Attribute(sector, QdMo5MfmFormat.PrefixAttribute, QdMo5MfmFormat.DefaultPrefix);
            if (prefixValue is < QdMo5MfmFormat.MinimumPrefix or > QdMo5MfmFormat.MaximumPrefix) throw QdMo5MfmFormat.InvalidPrefix(prefixValue);
            WriteHeader(bits, address);
            WriteData(bits, (byte)prefixValue, sector.Data);
        }
        return bits;
    }

    /// <summary>Valide puis convertit le numéro de secteur en deux octets grand-boutistes.</summary>
    /// <param name="sectorNumber">Numéro de secteur.</param>
    /// <returns>Deux octets, poids fort puis poids faible.</returns>
    private static byte[] EncodeSectorNumber(int sectorNumber)
    {
        if (sectorNumber is < QdMo5MfmFormat.MinimumSectorNumber or > QdMo5MfmFormat.MaximumSectorNumber) throw QdMo5MfmFormat.InvalidSectorNumber(sectorNumber);
        return [(byte)(sectorNumber >> QdMo5MfmFormat.SectorNumberHighByteShift), (byte)sectorNumber];
    }

    /// <summary>Écrit la synchronisation d'adresse, le numéro de secteur, la zone réservée et le premier gap.</summary>
    /// <param name="bits">Tampon recevant les cellules binaires.</param>
    /// <param name="address">Numéro de secteur grand-boutiste.</param>
    private static void WriteHeader(List<bool> bits, IReadOnlyList<byte> address)
    {
        bits.Raw(QdMo5MfmFormat.HeaderMark.ToArray());
        bits.Mfm(address.Concat(new byte[QdMo5MfmFormat.HeaderPaddingByteCount]));
        bits.Gap(QdMo5MfmFormat.HeaderGapBitCount);
    }

    /// <summary>Écrit la synchronisation de données, le préfixe, la charge utile, son checksum et le second gap.</summary>
    /// <param name="bits">Tampon recevant les cellules binaires.</param>
    /// <param name="prefix">Préfixe de données validé.</param>
    /// <param name="data">Données sectorielles validées.</param>
    private static void WriteData(List<bool> bits, byte prefix, IReadOnlyList<byte> data)
    {
        bits.Raw(QdMo5MfmFormat.Preamble.ToArray());
        bits.Mfm(new[] { prefix }.Concat(data).Append(QdMo5Checksum.Compute(prefix, data)));
        bits.Gap(QdMo5MfmFormat.DataGapBitCount);
    }
}
