using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Northstar MFM.</summary>
public sealed class NorthstarMfmDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Sector Mark » utilisée par ce codec.</summary>
    private static readonly byte[] SectorMark = NorthstarMfmFormat.SectorMark.ToArray();
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.NorthstarMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.NorthstarMfm;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><returns>Résultat du décodage NorthStar MFM.</returns>
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        var signatureBits = SectorMark.Length * Primitives.BitPrimitives.BitsPerByte;
        const int payloadBits = NorthstarMfmFormat.SectorSize * 16;
        for (var offset = 0; offset + signatureBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark)) continue;
            var hasIdentity = offset + signatureBits + 16 <= stream.Bits.Length; var fullBlock = offset + signatureBits + 16 + payloadBits + 16 <= stream.Bits.Length;
            byte info = 0;
            if (hasIdentity && !FluxBitReader.TryDecodeMfmByte(stream, offset + signatureBits, out info)) continue;
            var cylinder = (byte)(info >> NorthstarMfmFormat.CylinderShift); var sectorNumber = (byte)(info & NorthstarMfmFormat.SectorMask); bool? checksumValid = null;
            if (fullBlock)
            {
                byte checksum = 0; var data = TryDecodeMfmBytes(stream, offset + signatureBits + 16, NorthstarMfmFormat.SectorSize);
                if (data is null) continue;
                for (var index = 0; index < data.Length; index++)
                {
                    var value = data[index];
                    checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1));
                }
                if (!FluxBitReader.TryDecodeMfmByte(stream, offset + signatureBits + 16 + payloadBits, out var stored)) continue;
                checksumValid = stored == checksum; bytes.Add(info); bytes.AddRange(data);
            }
            if (hasIdentity) sectors.Add(new(cylinder, 0, sectorNumber, 2, NorthstarMfmFormat.SectorSize, checksumValid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, fullBlock ? signatureBits + 16 + payloadBits + 16 : signatureBits,
                fullBlock ? $"{FluxStructureDescriptions.Identity("NorthStar", FluxStructureKind.FormatHeader, cylinder, 0, sectorNumber, NorthstarMfmFormat.SectorSize, null, null)}, {FluxStructureDescriptions.Integrity("checksum", checksumValid)}" : hasIdentity ? $"{FluxStructureDescriptions.Identity("NorthStar", FluxStructureKind.FormatHeader, cylinder, 0, sectorNumber, NorthstarMfmFormat.SectorSize, null, null)}, {FluxStructureDescriptions.Integrity("checksum", null)}" : FluxStructureDescriptions.Truncated("NorthStar", FluxStructureKind.FormatHeader, null, "hard-sector block")));
            offset += signatureBits - 1;
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
