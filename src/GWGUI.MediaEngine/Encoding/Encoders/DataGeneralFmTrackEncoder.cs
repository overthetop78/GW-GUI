using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Data General 2F.</summary>
public sealed class DataGeneralFmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => DataGeneralFmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => DataGeneralFmFormat.CodecDisplayName;

    /// <summary>Encode les secteurs Data General 2F avec leur identité et leur somme de contrôle.</summary>
    /// <param name="request">Piste logique contenant cylindre, face et secteurs à encoder.</param>
    /// <returns>Cellules FM de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille Data General attendue.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        ValidateValue(nameof(request.Cylinder), request.Cylinder, DataGeneralFmFormat.MaximumCylinder);
        ValidateValue(nameof(request.Head), request.Head, DataGeneralFmFormat.MaximumHead);
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != DataGeneralFmFormat.SectorSize) throw DataGeneralFmFormat.InvalidSectorSize(sector.Data.Count);
            ValidateValue(nameof(sector.Number), sector.Number, DataGeneralFmFormat.MaximumSectorNumber);
            WriteAddress(bits, request.Cylinder, request.Head, sector.Number);
            WriteData(bits, sector.Data);
        }
        return bits;
    }

    /// <summary>Écrit la synchronisation, l'adresse composée et le gap d'en-tête.</summary>
    private static void WriteAddress(List<bool> bits, int cylinder, int head, int sector)
    {
        bits.Raw(DataGeneralFmFormat.Sync.ToArray());
        bits.Fm([(byte)(cylinder | head << DataGeneralFmFormat.HeadShift), (byte)(sector << DataGeneralFmFormat.SectorShift)]);
        bits.Gap(DataGeneralFmFormat.HeaderGapBitCount);
    }

    /// <summary>Écrit la synchronisation, les données, le checksum fort puis faible et le gap final.</summary>
    private static void WriteData(List<bool> bits, IReadOnlyList<byte> data)
    {
        bits.Raw(DataGeneralFmFormat.Sync.ToArray());
        var checksum = DataGeneralChecksum.Calculate(data);
        bits.Fm(data.Concat([(byte)(checksum >> BitPrimitives.BitsPerByte), (byte)checksum]));
        bits.Gap(DataGeneralFmFormat.DataGapBitCount);
    }

    /// <summary>Valide une valeur avant sa composition dans l'adresse Data General.</summary>
    private static void ValidateValue(string field, int value, int maximum)
    {
        if (value is < 0 || value > maximum) throw TrackEncodingExceptions.FormatValueOutOfRange("Data General 2F", field, value, maximum);
    }
}
