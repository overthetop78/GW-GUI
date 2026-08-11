using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Membrain MFM.</summary>
public sealed class MembrainMfmDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Sector Header » utilisée par ce codec.</summary>
    private static readonly byte[] SectorHeader = MembrainMfmFormat.SectorHeader.ToArray();
    /// <summary>Conserve la définition « Sector Data » utilisée par ce codec.</summary>
    private static readonly byte[] SectorData = MembrainMfmFormat.SectorData.ToArray();
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.MembrainMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.MembrainMfm;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><returns>Résultat du décodage Membrain MFM.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int headerBits = 6 * 16; const int sectorBytes = MembrainMfmFormat.SectorSize; const int dataBlockBytes = 2 + sectorBytes + MembrainMfmFormat.CrcByteCount;
        var pairedData = new HashSet<int>();
        for (var offset = 0; offset + SectorHeader.Length * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorHeader)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                var header = TryDecodeMfmBytes(stream, offset, 6);
                if (header is null) continue;
                var headerValid = header[1] == MembrainMfmFormat.HeaderAddressMark && Primitives.Crc16Calculator.Compute(header, MembrainMfmFormat.CrcPolynomial, MembrainMfmFormat.CrcInitialValue) == 0;
                var cylinder = (byte)(((header[2] & MembrainMfmFormat.CylinderHighMask) << MembrainMfmFormat.CylinderLowBitCount) | ((header[3] & MembrainMfmFormat.CylinderLowMask) >> MembrainMfmFormat.CylinderLowShift));
                var head = (byte)((header[3] >> MembrainMfmFormat.HeadShift) & MembrainMfmFormat.HeadMask); var number = (byte)(header[3] & MembrainMfmFormat.SectorMask);
                bytes.AddRange(header);
                var dataOffset = FindMark(stream, offset + 1, Math.Min(stream.Bits.Length, offset + 104 * BitPrimitives.BitsPerByte), SectorData);
                bool? dataValid = null; var structureEnd = offset + headerBits;
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset);
                    var dataEnd = dataOffset + dataBlockBytes * 16;
                    if (dataEnd <= stream.Bits.Length)
                    {
                        var data = TryDecodeMfmBytes(stream, dataOffset, dataBlockBytes);
                        if (data is null) continue;
                        dataValid = data[1] is >= MembrainMfmFormat.DataAddressMark and <= MembrainMfmFormat.LastDataAddressMark && Primitives.Crc16Calculator.Compute(data, MembrainMfmFormat.CrcPolynomial, MembrainMfmFormat.CrcInitialValue) == 0;
                        bytes.AddRange(data.Skip(2).Take(sectorBytes)); structureEnd = dataEnd;
                        structures.Add(new(FluxStructureKind.FormatData, dataOffset, dataEnd - dataOffset, $"{FluxStructureDescriptions.Identity("Membrain", FluxStructureKind.FormatData, cylinder, head, number, MembrainMfmFormat.SectorSize, null, "data block")}, {FluxStructureDescriptions.Integrity("CRC", dataValid)}"));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, SectorData.Length * BitPrimitives.BitsPerByte, FluxStructureDescriptions.Truncated("Membrain", FluxStructureKind.FormatData, null, "CRC unavailable")));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(cylinder, head, number, 2, sectorBytes, integrity, offset));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, FluxStructureDescriptions.Complete("Membrain", FluxStructureKind.FormatHeader, cylinder, head, number, MembrainMfmFormat.SectorSize, null, null, headerValid, dataValid)));
                offset = Math.Max(offset + SectorHeader.Length * BitPrimitives.BitsPerByte - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, SectorHeader.Length * BitPrimitives.BitsPerByte, FluxStructureDescriptions.Truncated("Membrain", FluxStructureKind.FormatHeader, null, "sector header")));
            if (!complete) offset += SectorHeader.Length * BitPrimitives.BitsPerByte - 1;
        }
        for (var offset = 0; offset + SectorData.Length * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++) if (FluxBitReader.MatchBytes(stream, offset, SectorData) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, SectorData.Length * BitPrimitives.BitsPerByte, FluxStructureDescriptions.UnpairedData("Membrain", null, "data block"))); offset += SectorData.Length * BitPrimitives.BitsPerByte - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Exécute le traitement « Find Mark » propre à ce format.</summary>
    /// <param name="stream">Flux source.</param><param name="start">Début inclus en bits.</param><param name="end">Fin exclue en bits.</param><param name="mark">Marque recherchée.</param><returns>Offset trouvé, ou <c>-1</c>.</returns>
    private static int FindMark(FluxBitstream stream, int start, int end, IReadOnlyList<byte> mark)
    {
        for (var offset = Math.Max(0, start); offset + mark.Count * BitPrimitives.BitsPerByte <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, mark)) return offset;
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
