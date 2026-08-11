using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Micral NFM.</summary>
public sealed class MicralNFmDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Sector Mark » utilisée par ce codec.</summary>
    private static readonly byte[] SectorMark = MicralNFmFormat.SectorMark.ToArray();
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.MicralNFm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.MicralNFm;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><returns>Résultat du décodage Micral N FM.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveFm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        var markBits = SectorMark.Length * 8; const int syncOffset = MicralNFmFormat.SyncZeroCount * 16; const int blockBytes = 1 + MicralNFmFormat.IdentityByteCount + MicralNFmFormat.SectorSize + MicralNFmFormat.ChecksumByteCount;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark)) continue;
            var blockStart = offset + syncOffset;
            var complete = blockStart + blockBytes * 16 <= stream.Bits.Length;
            if (complete)
            {
                var block = TryDecodeMfmBytes(stream, blockStart + 16, 131);
                if (block is null) continue;
                var number = block[0];
                var cylinder = block[1];
                var data = block.AsSpan(2, 128).ToArray();
                var storedChecksum = block[130];
                byte checksum = 0;
                foreach (var value in data) checksum = UpdateChecksum(checksum, value);
                var valid = checksum == storedChecksum;
                bytes.AddRange(data);
                sectors.Add(new(cylinder, 0, number, 0, 128, valid, offset, SectorIntegrityKind.Checksum));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, syncOffset + blockBytes * 16,
                    $"{FluxStructureDescriptions.Identity("Micral N", FluxStructureKind.FormatHeader, cylinder, 0, number, MicralNFmFormat.SectorSize, null, null)}, {FluxStructureDescriptions.Integrity("checksum", valid)}"));
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, FluxStructureDescriptions.Truncated("Micral N", FluxStructureKind.FormatHeader, null, "hard-sector block, checksum unavailable")));
            offset += markBits - 1;
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Exécute le traitement « Update Checksum » propre à ce format.</summary>
    /// <param name="checksum">Somme courante.</param><param name="data">Octet ajouté.</param><returns>Somme mise à jour.</returns>
    private static byte UpdateChecksum(byte checksum, byte data)
    {
            var carrySource = ((data ^ checksum) ^ MicralNFmFormat.ComplementMask) & ((data + checksum) ^ data);
            var carry = (carrySource & MicralNFmFormat.CarryMask) != 0 ? 1 : 0;
        return (byte)(checksum + data + carry);
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
