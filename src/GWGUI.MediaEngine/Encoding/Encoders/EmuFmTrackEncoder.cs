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
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != EmuFmFormat.SectorSize) throw EmuFmFormat.InvalidSectorSize(sector.Data.Count);
            var rawTrack = BitPrimitives.ReverseBits((byte)(request.Cylinder << EmuFmFormat.TrackShift | request.Head));
            var headerCrc = Crc16Calculator.Compute([rawTrack], EmuFmFormat.CrcPolynomial, EmuFmFormat.CrcInitialValue);
            bits.Raw(EmuFmFormat.SectorMark.ToArray());
            bits.DoubleFm([rawTrack, (byte)(headerCrc >> BitPrimitives.BitsPerByte), (byte)headerCrc]);
            bits.Gap(EmuFmFormat.GapBitCount, true);
            var dataCrc = Crc16Calculator.Compute(sector.Data, EmuFmFormat.CrcPolynomial, EmuFmFormat.CrcInitialValue);
            bits.Raw(EmuFmFormat.SectorMark.ToArray());
            bits.DoubleFm(sector.Data.Concat([(byte)(dataCrc >> BitPrimitives.BitsPerByte), (byte)dataCrc]));
            bits.Gap(EmuFmFormat.GapBitCount, true);
        }
        return bits;
    }
}
