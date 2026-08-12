using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Aed6200p MFM.</summary>
public sealed class Aed6200pMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.Aed6200pMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.Aed6200pMfm;
    /// <summary>Encode les secteurs demandés sous forme de cellules MFM AED 6200P.</summary>
    /// <param name="request">Piste logique contenant le cylindre, la face et les secteurs à encoder.</param>
    /// <returns>Cellules binaires de la piste, dans leur ordre d'émission.</returns>
    /// <remarks>La taille de chaque charge utile est enregistrée sur deux octets dans l'en-tête ; les CRC couvrent respectivement l'identité et la marque suivie des données.</remarks>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            Validate(request.Cylinder, sector.Number, sector.Data.Count);
            WriteHeader(bits, BuildHeader(request.Cylinder, sector.Number, sector.Data.Count));
            WriteData(bits, SelectDataMark(sector.Deleted), sector.Data);
        }
        return bits;
    }

    /// <summary>Valide les champs AED stockés sur un ou deux octets.</summary>
    private static void Validate(int cylinder, int sector, int size)
    {
        if (cylinder is < 0 or > Aed6200pMfmFormat.MaximumCylinder) throw TrackEncodingExceptions.FormatValueOutOfRange("AED 6200P", nameof(cylinder), cylinder, Aed6200pMfmFormat.MaximumCylinder);
        if (sector is < 0 or > Aed6200pMfmFormat.MaximumSector) throw TrackEncodingExceptions.FormatValueOutOfRange("AED 6200P", nameof(sector), sector, Aed6200pMfmFormat.MaximumSector);
        if (size is < 0 or > Aed6200pMfmFormat.MaximumSectorByteCount) throw TrackEncodingExceptions.FormatValueOutOfRange("AED 6200P", nameof(size), size, Aed6200pMfmFormat.MaximumSectorByteCount);
    }

    /// <summary>Construit l'en-tête AED avec la taille en ordre faible puis fort.</summary>
    private static byte[] BuildHeader(int cylinder, int sector, int size) => [Aed6200pMfmFormat.HeaderAddressMark, (byte)cylinder, (byte)size, (byte)sector, (byte)(size >> BitPrimitives.BitsPerByte)];

    /// <summary>Sélectionne la marque de données normale ou supprimée.</summary>
    private static Aed6200pDataMarkDefinition SelectDataMark(bool deleted) => Aed6200pMfmFormat.DataMarks.Single(definition => definition.Deleted == deleted && (deleted || definition.Mark == Aed6200pMfmFormat.DataMark));

    /// <summary>Écrit l'en-tête, son CRC fort-faible et le gap de 64 cellules.</summary>
    private static void WriteHeader(List<bool> bits, IReadOnlyList<byte> header)
    {
        bits.Raw(Aed6200pMfmFormat.HeaderPattern.ToArray());
        bits.Mfm(Crc16Calculator.Append(header).Skip(1));
        bits.Gap(Aed6200pMfmFormat.FirstGapBitCount);
    }

    /// <summary>Écrit la marque, les données, leur CRC fort-faible et le gap de 128 cellules.</summary>
    private static void WriteData(List<bool> bits, Aed6200pDataMarkDefinition definition, IReadOnlyList<byte> data)
    {
        bits.Raw(definition.Pattern.ToArray());
        bits.Mfm(Crc16Calculator.Append(new[] { definition.Mark }.Concat(data)).Skip(1));
        bits.Gap(Aed6200pMfmFormat.SecondGapBitCount);
    }
}
