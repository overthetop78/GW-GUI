using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Heathkit FM.</summary>
public sealed class HeathkitFmDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Sector Mark » utilisée par ce codec.</summary>
    private static readonly byte[] SectorMark = HeathkitFmFormat.SectorMark.ToArray();
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.HeathkitFm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.HeathkitFm;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><returns>Résultat du décodage Heathkit FM.</returns>
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveFm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        const int signatureBits = HeathkitFmFormat.HeaderByteCount * 16;
        const int headerTailBits = HeathkitFmFormat.HeaderByteCount * 16;
        for (var offset = 0; offset + signatureBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark)) continue;
            var complete = offset + signatureBits + headerTailBits <= stream.Bits.Length;
            if (complete)
            {
                var header = TryDecodeMfmBytes(stream, offset + signatureBits, 4);
                if (header is null) continue;
                var volume = Primitives.BitPrimitives.ReverseBits(header[0]);
                var cylinder = Primitives.BitPrimitives.ReverseBits(header[1]);
                var sectorNumber = Primitives.BitPrimitives.ReverseBits(header[2]);
                var stored = Primitives.BitPrimitives.ReverseBits(header[3]);
                byte checksum = 0;
                foreach (var value in new[] { volume, cylinder, sectorNumber }) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
                var headerValid = stored == checksum; var dataOffset = FindNextMark(stream, offset + signatureBits + headerTailBits, (88 + 16) * Primitives.BitPrimitives.BitsPerByte); bool? dataValid = null; var structureEnd = offset + signatureBits + headerTailBits;
                bytes.AddRange([volume, cylinder, sectorNumber]);
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset); var dataEnd = dataOffset + signatureBits + 257 * 16;
                    if (dataEnd <= stream.Bits.Length)
                    {
                        var decoded = TryDecodeMfmBytes(stream, dataOffset + signatureBits, 257);
                        if (decoded is null) continue;
                        var data = decoded.AsSpan(0, 256).ToArray();
                        for (var index = 0; index < data.Length; index++) data[index] = Primitives.BitPrimitives.ReverseBits(data[index]);
                        var dataStored = Primitives.BitPrimitives.ReverseBits(decoded[256]); byte dataChecksum = 0;
                        foreach (var value in data) { dataChecksum ^= value; dataChecksum = (byte)((dataChecksum >> 7) | (dataChecksum << 1)); }
                        dataValid = dataStored == dataChecksum; bytes.AddRange(data); structureEnd = dataEnd;
                    structures.Add(new(FluxStructureKind.FormatData, dataOffset, dataEnd - dataOffset, $"{FluxStructureDescriptions.Identity("Heathkit", FluxStructureKind.FormatData, cylinder, 0, sectorNumber, HeathkitFmFormat.SectorSize, null, null)}, {FluxStructureDescriptions.Integrity("checksum", dataValid)}"));
                    }
                else structures.Add(new(FluxStructureKind.FormatData, dataOffset, signatureBits, FluxStructureDescriptions.Truncated("Heathkit", FluxStructureKind.FormatData, null, "checksum unavailable")));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(cylinder, 0, sectorNumber, 1, 256, integrity, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, signatureBits + headerTailBits, FluxStructureDescriptions.Complete("Heathkit", FluxStructureKind.FormatHeader, cylinder, 0, sectorNumber, HeathkitFmFormat.SectorSize, null, $"volume {volume}", headerValid, dataValid, "header checksum", "data checksum")));
                offset = Math.Max(offset + signatureBits - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, signatureBits, FluxStructureDescriptions.Truncated("Heathkit", FluxStructureKind.FormatHeader, null, "hard-sector header")));
            if (!complete) offset += signatureBits - 1;
        }
        for (var offset = 0; offset + signatureBits <= stream.Bits.Length; offset++) if (FluxBitReader.MatchBytes(stream, offset, SectorMark) && !pairedData.Contains(offset) && structures.All(item => item.BitOffset != offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, signatureBits, FluxStructureDescriptions.UnpairedData("Heathkit", null, "data block"))); offset += signatureBits - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Recherche la prochaine marque du format.</summary>
    /// <param name="stream">Flux source.</param><param name="start">Offset initial en bits.</param><param name="maximumDistance">Distance maximale en bits.</param><returns>Offset trouvé, ou <c>-1</c>.</returns>
    private static int FindNextMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - SectorMark.Length * Primitives.BitPrimitives.BitsPerByte, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, SectorMark)) return offset;
        return -1;
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
