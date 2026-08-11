using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Micropolis MFM.</summary>
public sealed class MicropolisMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.MicropolisMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.MicropolisMfm;

    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != MicropolisMfmFormat.SectorSize) throw MicropolisMfmFormat.InvalidSectorSize(sector.Data.Count);
            var record = new List<byte> { MicropolisMfmFormat.AddressMark, (byte)request.Cylinder, (byte)sector.Number };
            record.AddRange(Enumerable.Repeat((byte)0, MicropolisMfmFormat.HeaderPaddingByteCount));
            record.AddRange(sector.Data);
            record.Add(Checksum(record.Skip(1)));
            record.AddRange(Enumerable.Repeat((byte)0, MicropolisMfmFormat.TrailerPaddingByteCount));
            bits.Mfm(new byte[MicropolisMfmFormat.PreambleByteCount]);
            bits.Mfm(record);
            bits.Gap(MicropolisMfmFormat.GapBitCount);
        }
        return bits;
    }

    /// <summary>Calcule la somme de contrôle du bloc fourni.</summary>
    private static byte Checksum(IEnumerable<byte> data)
    {
        var value = 0;
        foreach (var item in data) { if (value > MicropolisMfmFormat.ChecksumModulus) value -= MicropolisMfmFormat.ChecksumModulus; value += item; }
        return (byte)value;
    }
}
