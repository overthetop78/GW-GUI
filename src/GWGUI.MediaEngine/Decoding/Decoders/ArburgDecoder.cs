using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Arburg.</summary>
public sealed class ArburgDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Data Mark » utilisée par ce codec.</summary>
    private static readonly byte[] DataMark = ArburgFormat.DataMark.ToArray();
    /// <summary>Conserve la définition « System Mark » utilisée par ce codec.</summary>
    private static readonly byte[] SystemMark = ArburgFormat.SystemMark.ToArray();
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.Arburg;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.Arburg;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><returns>Résultat regroupant les blocs de données et système Arburg reconnus.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        ScanFmData(stream, structures, sectors, bytes);
        ScanSystemData(stream, structures, sectors, bytes);
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 8d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    /// <summary>Analyse les blocs de données FM.</summary>
    /// <param name="stream">Flux binaire FM source.</param><param name="structures">Structures auxquelles ajouter les blocs reconnus.</param><param name="sectors">Secteurs auxquels ajouter les blocs reconstruits.</param><param name="bytes">Octets auxquels ajouter les données décodées.</param>
    private static void ScanFmData(FluxBitstream stream, List<FluxStructure> structures, List<DecodedSector> sectors, List<byte> bytes)
    {
        var markBits = DataMark.Length * Primitives.BitPrimitives.BitsPerByte; const int blockSize = ArburgFormat.DataBlockSize, usefulSize = ArburgFormat.DataUsefulSize;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, DataMark)) continue;
            var complete = offset + markBits + blockSize * 32 <= stream.Bits.Length; bool? valid = null;
            if (complete)
            {
                var decoded = TryDecodeFmBytes(stream, offset + markBits, blockSize);
                if (decoded is null) continue;
                ushort checksum = 0; var data = new byte[usefulSize];
                for (var index = 0; index < usefulSize; index++) { var value = Primitives.BitPrimitives.ReverseBits(decoded[index]); data[index] = value; checksum += value; }
                var low = Primitives.BitPrimitives.ReverseBits(decoded[usefulSize]); var high = Primitives.BitPrimitives.ReverseBits(decoded[usefulSize + 1]);
                valid = low == (byte)checksum && high == (byte)(checksum >> Primitives.BitPrimitives.BitsPerByte); bytes.AddRange(data);
            }
            sectors.Add(new(0, 0, 1, 0, blockSize, valid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatData, offset, complete ? markBits + blockSize * 32 : markBits, $"{FluxStructureDescriptions.Identity("Arburg", FluxStructureKind.FormatData, 0, 0, 1, blockSize, null, "data block")}, {FluxStructureDescriptions.Integrity("checksum", valid)}"));
            offset += markBits - 1;
        }
    }

    /// <summary>Analyse les blocs de données système.</summary>
    /// <param name="stream">Flux binaire source.</param><param name="structures">Structures auxquelles ajouter les blocs reconnus.</param><param name="sectors">Secteurs auxquels ajouter les blocs reconstruits.</param><param name="bytes">Octets auxquels ajouter les données décodées.</param>
    private static void ScanSystemData(FluxBitstream stream, List<FluxStructure> structures, List<DecodedSector> sectors, List<byte> bytes)
    {
        var markBits = SystemMark.Length * Primitives.BitPrimitives.BitsPerByte; const int blockSize = ArburgFormat.SystemBlockSize, usefulSize = ArburgFormat.SystemUsefulSize;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SystemMark)) continue;
            var decoded = TryDecodeSystemBytes(stream, offset + markBits, blockSize); bool? valid = null;
            if (decoded is not null)
            {
                ushort checksum = 0; for (var index = 0; index < usefulSize; index++) checksum += decoded.Value.Bytes[index];
                valid = decoded.Value.Bytes[usefulSize] == (byte)checksum && decoded.Value.Bytes[usefulSize + 1] == (byte)(checksum >> Primitives.BitPrimitives.BitsPerByte); bytes.AddRange(decoded.Value.Bytes.Take(usefulSize));
            }
            sectors.Add(new(0, 0, 1, 0, blockSize, valid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, decoded is null ? markBits : decoded.Value.EndOffset - offset, $"{FluxStructureDescriptions.Identity("Arburg", FluxStructureKind.FormatHeader, 0, 0, 1, blockSize, null, "system block")}, {FluxStructureDescriptions.Integrity("checksum", valid)}"));
            offset += markBits - 1;
        }
    }

    /// <summary>Tente de décoder les octets d'un bloc système.</summary>
    private static (byte[] Bytes, int EndOffset)? TryDecodeSystemBytes(FluxBitstream stream, int start, int count)
    {
        var result = new byte[count]; var offset = start;
        for (var index = 0; index < count; index++)
        {
            byte value = 0;
            for (var bit = 0; bit < Primitives.BitPrimitives.BitsPerByte; bit++)
            {
                if (offset + 2 > stream.Bits.Length || stream.Bits[offset]) return null;
                if (stream.Bits[offset + 1]) offset += 2;
                else
                {
                    if (offset + 3 > stream.Bits.Length || !stream.Bits[offset + 2]) return null;
                    value |= (byte)(1 << bit); offset += 3;
                }
            }
            result[index] = value;
        }
        return (result, offset);
    }

    /// <summary>Tente de décoder une suite d'octets FM.</summary>
    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeFmByte32(stream, offset + index * 32, out result[index])) return null;
        return result;
    }
}
