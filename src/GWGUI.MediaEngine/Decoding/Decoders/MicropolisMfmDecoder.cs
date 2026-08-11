using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Encoding.Definitions;

using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Micropolis MFM.</summary>
public sealed class MicropolisMfmDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Sync » utilisée par ce codec.</summary>
    private static readonly byte[] Sync = MicropolisMfmFormat.Sync.ToArray();

    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.MicropolisMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.MicropolisMfm;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><returns>Résultat du décodage Micropolis MFM.</returns>
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        const int recordBytes = MicropolisMfmFormat.RecordByteCount;

        for (var offset = 0; offset + Sync.Length * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, Sync)) continue;
            var recordStart = offset + 3 * 16;
            if (recordStart + recordBytes * 16 > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, Sync.Length * BitPrimitives.BitsPerByte, FluxStructureDescriptions.Truncated("Micropolis", FluxStructureKind.FormatHeader, null, "sector")));
                offset += Sync.Length * BitPrimitives.BitsPerByte - 1;
                continue;
            }

            var record = TryDecodeMfmBytes(stream, recordStart, recordBytes);
            if (record is null) continue;
            var cylinder = record[1];
            var sectorNumber = record[2];
            var valid = Checksum(record.AsSpan(1, recordBytes - 7)) == record[recordBytes - 6];
            var payload = record.AsSpan(MicropolisMfmFormat.RecordIdentityByteCount + MicropolisMfmFormat.HeaderPaddingByteCount, MicropolisMfmFormat.SectorSize).ToArray();
            bytes.AddRange(payload);
            sectors.Add(new(cylinder, 0, sectorNumber, 1, MicropolisMfmFormat.SectorSize, valid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, (3 + recordBytes) * 16,
                $"{FluxStructureDescriptions.Identity("Micropolis", FluxStructureKind.FormatHeader, cylinder, 0, sectorNumber, MicropolisMfmFormat.SectorSize, null, null)}, {FluxStructureDescriptions.Integrity("checksum", valid)}"));
            offset += Sync.Length * BitPrimitives.BitsPerByte - 1;
        }

        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 24d), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Calcule la somme de contrôle du bloc fourni.</summary>
    /// <param name="data">Octets du bloc.</param><returns>Somme de contrôle calculée.</returns>
    private static byte Checksum(ReadOnlySpan<byte> data)
    {
        var value = 0;
        foreach (var item in data)
        {
            if (value > MicropolisMfmFormat.ChecksumModulus) value -= MicropolisMfmFormat.ChecksumModulus;
            value += item;
        }
        return (byte)value;
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
