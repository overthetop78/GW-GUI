using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Iso MFM.</summary>
public sealed class IsoMfmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.IsoMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.IsoMfm;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><returns>Meilleur résultat ISO MFM obtenu.</returns>
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var centre = FluxTimingEstimator.EstimateNonFmBitCell(revolution.FluxIntervals);
        var first = DecodeCore(FluxTransitionDecoder.DecodePll(revolution.FluxIntervals, centre));
        if (first.Sectors?.All(sector => sector.Data is not null && sector.IntegrityValid == true) == true) return first;

        var best = first;
        var bestScore = Score(first);
        foreach (var factor in new[] { .98, 1.02, .96, 1.04, .94, 1.06 })
        {
            var candidate = DecodeCore(FluxTransitionDecoder.DecodePll(revolution.FluxIntervals, centre * factor));
            var score = Score(candidate);
            if (score > bestScore) { best = candidate; bestScore = score; }
        }
        return best;
    }

    /// <summary>Exécute le traitement « Decode Core » propre à ce format.</summary>
    /// <param name="source">Flux binaire source.</param><returns>Résultat du décodage.</returns>
    private FluxDecodeResult DecodeCore(FluxBitstream source)
    {
        var originalLength = source.Bits.Length;
        var stream = source.WithCircularTail(20_000);
        var structures = new List<FluxStructure>(); var bytes = new List<byte>(); var sectors = new List<DecodedSector>();
        var headers = new List<(int Offset, byte Cylinder, byte Head, byte Number, byte SizeCode, int Size, bool? Valid)>(); var dataMarks = new List<(int Offset, byte Mark)>();
        for (var offset = 0; offset + IsoMfmFormat.SyncAndMarkBitCount <= originalLength; offset++)
        {
            if (!FluxBitReader.Match(stream, offset, IsoMfmFormat.EncodedSyncByte) || !FluxBitReader.Match(stream, offset + IsoMfmFormat.EncodedByteBitCount, IsoMfmFormat.EncodedSyncByte) || !FluxBitReader.Match(stream, offset + IsoMfmFormat.EncodedByteBitCount * 2, IsoMfmFormat.EncodedSyncByte)) continue;
            if (!FluxBitReader.TryDecodeMfmByte(stream, offset + IsoMfmFormat.SyncBitCount, out var mark)) continue;
            var kind = mark switch { IsoMfmFormat.IdAddressMark => FluxStructureKind.IdAddressMark, IsoMfmFormat.DataAddressMark => FluxStructureKind.DataAddressMark, IsoMfmFormat.DeletedDataAddressMark => FluxStructureKind.DeletedDataAddressMark, _ => FluxStructureKind.Sync };
            var description = FluxStructureDescriptions.UnclassifiedMark("ISO MFM", kind, mark, null);
            if (mark == IsoMfmFormat.IdAddressMark && offset + IsoMfmFormat.HeaderBitCount <= stream.Bits.Length)
            {
                var headerBytes = TryDecodeMfmBytes(stream, offset + IsoMfmFormat.SyncAndMarkBitCount, IsoMfmFormat.HeaderBytesAfterMark);
                if (headerBytes is null) continue;
                var cylinder = headerBytes[0]; var head = headerBytes[1]; var number = headerBytes[2]; var sizeCode = headerBytes[3];
                var storedCrc = (ushort)((headerBytes[IsoMfmFormat.HeaderFieldByteCount] << BitPrimitives.BitsPerByte) | headerBytes[IsoMfmFormat.HeaderFieldByteCount + 1]);
                var calculatedCrc = Primitives.Crc16Calculator.Compute([IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.IdAddressMark, cylinder, head, number, sizeCode], IsoMfmFormat.CrcPolynomial, IsoMfmFormat.CrcInitialValue); var valid = storedCrc == calculatedCrc;
                headers.Add((offset, cylinder, head, number, sizeCode, sizeCode <= IsoMfmFormat.MaximumSectorSizeCode ? IsoMfmFormat.BaseSectorSize << sizeCode : 0, valid));
                description = $"{FluxStructureDescriptions.Identity("ISO MFM", kind, cylinder, head, number, sizeCode <= IsoMfmFormat.MaximumSectorSizeCode ? IsoMfmFormat.BaseSectorSize << sizeCode : 0, mark, $"N{sizeCode}")}, {FluxStructureDescriptions.Integrity("CRC", valid)}";
            }
            else if (mark == IsoMfmFormat.IdAddressMark) headers.Add((offset, 0, 0, 0, 0, 0, null));
            else if (mark is IsoMfmFormat.DataAddressMark or IsoMfmFormat.DeletedDataAddressMark) dataMarks.Add((offset, mark));
            structures.Add(new(kind, offset, mark == IsoMfmFormat.IdAddressMark ? IsoMfmFormat.HeaderBitCount : IsoMfmFormat.SyncAndMarkBitCount, description)); bytes.Add(mark); offset += IsoMfmFormat.SyncBitCount - 1;
        }
        dataMarks.AddRange(dataMarks.Where(mark => mark.Offset < stream.Bits.Length - originalLength)
            .Select(mark => (mark.Offset + originalLength, mark.Mark)).ToArray());
        structures.RemoveAll(structure => structure.Kind == FluxStructureKind.IdAddressMark);
        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index]; var nextHeader = index + 1 < headers.Count ? headers[index + 1].Offset : int.MaxValue;
            (int Offset, byte Mark)? data = dataMarks.Where(candidate => candidate.Offset > header.Offset + IsoMfmFormat.HeaderBitCount - 1 && candidate.Offset < nextHeader).Select(candidate => ((int, byte)?)candidate).FirstOrDefault(); bool? dataValid = null; byte[]? payload = null;
            if (data is not null && header.Size > 0)
            {
                var end = data.Value.Offset + IsoMfmFormat.SyncAndMarkBitCount + (header.Size + IsoMfmFormat.CrcByteCount) * IsoMfmFormat.EncodedByteBitCount;
                if (end <= stream.Bits.Length)
                {
                    var dataBytes = TryDecodeMfmBytes(stream, data.Value.Offset + IsoMfmFormat.SyncAndMarkBitCount, header.Size + IsoMfmFormat.CrcByteCount);
                    if (dataBytes is null) continue;
                    payload = dataBytes.AsSpan(0, header.Size).ToArray();
                    var stored = (ushort)((dataBytes[header.Size] << BitPrimitives.BitsPerByte) | dataBytes[header.Size + 1]); dataValid = stored == Primitives.Crc16Calculator.Compute(new byte[] { IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, IsoMfmFormat.SyncByte, data.Value.Mark }.Concat(payload), IsoMfmFormat.CrcPolynomial, IsoMfmFormat.CrcInitialValue); bytes.AddRange(payload);
                    structures.RemoveAll(structure => structure.BitOffset == data.Value.Offset); structures.Add(new(data.Value.Mark == IsoMfmFormat.DataAddressMark ? FluxStructureKind.DataAddressMark : FluxStructureKind.DeletedDataAddressMark, data.Value.Offset, end - data.Value.Offset, $"{FluxStructureDescriptions.Identity("ISO MFM", data.Value.Mark == IsoMfmFormat.DataAddressMark ? FluxStructureKind.DataAddressMark : FluxStructureKind.DeletedDataAddressMark, header.Cylinder, header.Head, header.Number, header.Size, data.Value.Mark, null)}, {FluxStructureDescriptions.Integrity("CRC", dataValid)}"));
                }
            }
            bool? integrity = header.Valid == false || dataValid == false ? false : dataValid is null ? null : true; sectors.Add(new(header.Cylinder, header.Head, header.Number, header.SizeCode, header.Size, integrity, header.Offset, Data: payload));
            structures.Add(new(FluxStructureKind.IdAddressMark, header.Offset, header.Valid is null ? IsoMfmFormat.SyncAndMarkBitCount : IsoMfmFormat.HeaderBitCount, FluxStructureDescriptions.Complete("ISO MFM", FluxStructureKind.IdAddressMark, header.Cylinder, header.Head, header.Number, header.Size, IsoMfmFormat.IdAddressMark, $"N{header.SizeCode}", header.Valid, dataValid)));
        }
        var confidence = Math.Min(1, (sectors.Count * 2 + structures.Count(x => x.Kind is FluxStructureKind.DataAddressMark or FluxStructureKind.DeletedDataAddressMark)) / 12d);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Exécute le traitement « Score » propre à ce format.</summary>
    /// <param name="result">Résultat à évaluer.</param><returns>Score de sélection calculé.</returns>
    private static int Score(FluxDecodeResult result)
    {
        var sectors = result.Sectors ?? [];
        return sectors.Count(sector => sector.IntegrityValid == true) * 1000
            + sectors.Count(sector => sector.Data is not null) * 10
            + sectors.Count;
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * IsoMfmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }
}
