using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format E-mu FM.</summary>
public sealed class EmuFmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => EmuFmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => EmuFmFormat.CodecDisplayName;

    /// <summary>Encode les secteurs E-mu avec leur adresse inversée et leurs CRC.</summary>
    /// <param name="request">Piste logique contenant cylindre, face et secteurs à encoder.</param>
    /// <returns>Cellules FM de la piste dans leur ordre d'émission.</returns>
    /// <exception cref="ArgumentException">La charge utile d'un secteur ne possède pas la taille E-mu attendue.</exception>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackBitEncoding.Bits();
        ValidateAddress(nameof(request.Cylinder), request.Cylinder, EmuFmFormat.MaximumCylinder);
        ValidateAddress(nameof(request.Head), request.Head, EmuFmFormat.MaximumHead);
        var rawTrack = ComposeRawTrack(request.Cylinder, request.Head);
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != EmuFmFormat.SectorSize) throw EmuFmFormat.InvalidSectorSize(sector.Data.Count);
            WriteHeader(bits, rawTrack);
            WriteData(bits, sector.Data);
        }
        return bits;
    }

    /// <summary>Compose le cylindre et la face puis inverse l'ordre de leurs bits pour le stockage E-mu.</summary>
    private static byte ComposeRawTrack(int cylinder, int head) => BitPrimitives.ReverseBits((byte)(cylinder << EmuFmFormat.TrackShift | head));

    /// <summary>Écrit la marque commune, l'adresse inversée, son CRC fort puis faible et le premier gap.</summary>
    private static void WriteHeader(List<bool> bits, byte rawTrack)
    {
        var crc = CalculateCrc([rawTrack]);
        bits.Raw(EmuFmFormat.SectorMark.ToArray());
        bits.DoubleFm([rawTrack, (byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]);
        bits.Gap(EmuFmFormat.GapBitCount, true);
    }

    /// <summary>Écrit la marque commune, les données, leur CRC fort puis faible et le second gap.</summary>
    private static void WriteData(List<bool> bits, IReadOnlyList<byte> data)
    {
        var crc = CalculateCrc(data);
        bits.Raw(EmuFmFormat.SectorMark.ToArray());
        bits.DoubleFm(data.Concat([(byte)(crc >> BitPrimitives.BitsPerByte), (byte)crc]));
        bits.Gap(EmuFmFormat.GapBitCount, true);
    }

    /// <summary>Calcule le CRC IBM commun aux champs d'adresse et de données.</summary>
    private static ushort CalculateCrc(IEnumerable<byte> values) => Crc16Calculator.Compute(values, EmuFmFormat.CrcPolynomial, EmuFmFormat.CrcInitialValue);

    /// <summary>Valide une composante de l'adresse avant son empaquetage.</summary>
    private static void ValidateAddress(string field, int value, int maximum)
    {
        if (value is < 0 || value > maximum) throw TrackEncodingExceptions.FormatValueOutOfRange("E-mu FM", field, value, maximum);
    }
}
