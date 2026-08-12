using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les secteurs GCR zonés de 512 octets employés par le Commodore 900.</summary>
public sealed class Commodore900GcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => Commodore900GcrFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => Commodore900GcrFormat.CodecDisplayName;

    /// <summary>Encode les secteurs Commodore 900 avec leurs marques et sommes de contrôle.</summary>
    /// <param name="request">Piste logique contenant le cylindre et les secteurs de 512 octets.</param>
    /// <returns>Cellules GCR de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne contient pas 512 octets.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        ValidateAddress(nameof(request.Cylinder), request.Cylinder);
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != Commodore900GcrFormat.SectorByteCount) throw Commodore900GcrFormat.InvalidSectorSize(sector.Data.Count);
            ValidateAddress(nameof(sector.Number), sector.Number);

            bits.Gap(Commodore900GcrFormat.SyncGapBitCount, true);
            bits.AddRange(CommodoreGcrCodec.Encode(BuildHeader((byte)request.Cylinder, (byte)sector.Number)));
            bits.Gap(Commodore900GcrFormat.RecordGapBitCount);
            bits.Gap(Commodore900GcrFormat.SyncGapBitCount, true);
            bits.AddRange(CommodoreGcrCodec.Encode(BuildDataRecord(sector.Data)));
            bits.Gap(Commodore900GcrFormat.RecordGapBitCount);
        }
        return bits;
    }

    /// <summary>Construit l'en-tête avec son checksum XOR.</summary>
    private static byte[] BuildHeader(byte cylinder, byte sector)
    {
        byte[] values = [Commodore900GcrFormat.HeaderMark, cylinder, sector];
        return values.Append(CommodoreGcrChecksum.Calculate(values)).ToArray();
    }

    /// <summary>Construit le champ de données de 512 octets avec sa marque et son checksum XOR.</summary>
    private static byte[] BuildDataRecord(IReadOnlyList<byte> payload)
    {
        var values = new byte[] { Commodore900GcrFormat.DataMark }.Concat(payload).ToArray();
        return values.Append(CommodoreGcrChecksum.Calculate(values)).ToArray();
    }

    /// <summary>Valide un champ d'adresse avant sa conversion en octet.</summary>
    private static void ValidateAddress(string field, int value)
    {
        if (value is < 0 || value > Commodore900GcrFormat.MaximumAddressValue) throw TrackEncodingExceptions.FormatValueOutOfRange("Commodore 900 GCR", field, value, Commodore900GcrFormat.MaximumAddressValue);
    }
}
