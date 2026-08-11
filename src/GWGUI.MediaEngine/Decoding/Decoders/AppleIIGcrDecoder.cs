using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Decoding.Apple;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Apple IIGCR.</summary>
public sealed class AppleIIGcrDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.AppleIIGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.AppleIIGcr;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP dont les intervalles sont décodés en NRZI Apple II.</param>
    /// <returns>Résultat du décodage de la piste.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution) => DecodeCore(FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals));

    /// <summary>Décode directement les bits d'une piste Apple II.</summary>
    /// <param name="bits">Bits de la piste dans leur ordre logique.</param>
    /// <returns>Résultat du décodage de la piste.</returns>
    internal FluxDecodeResult DecodeBits(bool[] bits) => DecodeCore(new FluxBitstream(bits, 1));

    /// <summary>Exécute le traitement « Decode Core » propre à ce format.</summary>
    /// <param name="stream">Flux binaire à analyser.</param>
    /// <returns>Structures, secteurs et octets reconnus dans le flux.</returns>
    private FluxDecodeResult DecodeCore(FluxBitstream stream)
    {
        var trackBitLength = stream.Bits.Length;
        stream = stream.WithCircularTail(AppleIIGcrFormat.CircularTailBitCount);
        var structures = new List<FluxStructure>(); var bytes = new List<byte>(); var sectors = new List<DecodedSector>(); var pairedData = new HashSet<int>();
        for (var offset = 0; offset < trackBitLength && offset + AppleIIGcrFormat.PrologueBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.Match(stream, offset, AppleIIGcrFormat.SixAndTwoAddressPrologue, AppleIIGcrFormat.PrologueBitCount)) continue;
            var address = DecodeAddress(stream.Bits, offset, bytes, true);
            var headerEnd = offset + AppleIIGcrFormat.PrologueBitCount + (address is null ? 0 : AppleIIGcrFormat.EncodedAddressBitCount);
            var data = FindAndDecodeSixAndTwoData(stream, headerEnd, address, structures, bytes, pairedData);
            AddSectorAndAddress(offset, headerEnd, address, data, null, sectors, structures);
            offset = address?.Valid == true ? Math.Max(offset + AppleIIGcrFormat.PrologueAdvanceBitCount, data.StructureEnd - 1) : offset + AppleIIGcrFormat.PrologueAdvanceBitCount;
        }
        DecodeFiveAndThree(stream, trackBitLength, structures, bytes, sectors, pairedData);
        for (var offset = 0; offset < trackBitLength && offset + AppleIIGcrFormat.PrologueBitCount <= stream.Bits.Length; offset++) if (FluxBitReader.Match(stream, offset, AppleIIGcrFormat.DataPrologue, AppleIIGcrFormat.PrologueBitCount) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.AppleData, offset, AppleIIGcrFormat.PrologueBitCount, FluxStructureDescriptions.UnpairedData(AppleIIGcrFormat.StructureDescriptionName, null, AppleIIGcrFormat.UnpairedDataVariant))); offset += AppleIIGcrFormat.PrologueAdvanceBitCount; }
        return new(Id, DisplayName, FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, AppleIIGcrFormat.ConfidenceSectorWeight, AppleIIGcrFormat.ConfidenceDivisor), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Exécute le traitement « Decode Five And Three » propre à ce format.</summary>
    /// <param name="stream">Flux binaire à analyser.</param><param name="trackBitLength">Longueur logique de la piste en bits.</param><param name="structures">Structures auxquelles ajouter les blocs reconnus.</param><param name="bytes">Octets auxquels ajouter les données décodées.</param><param name="sectors">Secteurs auxquels ajouter les secteurs reconnus.</param><param name="pairedData">Offsets en bits des données déjà associées à une adresse.</param>
    private static void DecodeFiveAndThree(FluxBitstream stream, int trackBitLength, List<FluxStructure> structures, List<byte> bytes, List<DecodedSector> sectors, HashSet<int> pairedData)
    {
        for (var offset = 0; offset < trackBitLength && offset + AppleIIGcrFormat.PrologueBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.Match(stream, offset, AppleIIGcrFormat.FiveAndThreeAddressPrologue, AppleIIGcrFormat.PrologueBitCount)) continue;
            var address = DecodeAddress(stream.Bits, offset, bytes, false);
            var headerEnd = offset + AppleIIGcrFormat.AddressBlockBitCount;
            var data = FindAndDecodeFiveAndThreeData(stream, headerEnd, address, structures, bytes, pairedData);
            AddSectorAndAddress(offset, headerEnd, address, data, AppleIIGcrFormat.ThirteenSectorVariant, sectors, structures);
            offset = address?.Valid == true ? Math.Max(offset + AppleIIGcrFormat.PrologueAdvanceBitCount, data.StructureEnd - 1) : offset + AppleIIGcrFormat.PrologueAdvanceBitCount;
        }
    }

    private static AppleAddressDecodeResult? DecodeAddress(IReadOnlyList<bool> bits, int offset, List<byte> bytes, bool appendDecodedBytes)
    {
        var encoded = TryReadBytes(bits, offset + AppleIIGcrFormat.PrologueBitCount, AppleIIGcrFormat.EncodedAddressByteCount);
        if (encoded is null) return null;
        var volume = AppleIIGcrCodec.DecodeFourAndFour(encoded[0], encoded[1]);
        var cylinder = AppleIIGcrCodec.DecodeFourAndFour(encoded[2], encoded[3]);
        var sector = AppleIIGcrCodec.DecodeFourAndFour(encoded[4], encoded[5]);
        var checksum = AppleIIGcrCodec.DecodeFourAndFour(encoded[6], encoded[7]);
        if (appendDecodedBytes) bytes.AddRange([volume, cylinder, sector, checksum]);
        return new(volume, cylinder, sector, (byte)(volume ^ cylinder ^ sector) == checksum);
    }

    private static AppleDataDecodeResult FindAndDecodeSixAndTwoData(FluxBitstream stream, int headerEnd, AppleAddressDecodeResult? address, List<FluxStructure> structures, List<byte> bytes, HashSet<int> pairedData)
    {
        var dataOffset = Find(stream, headerEnd, Math.Min(stream.Bits.Length, headerEnd + AppleIIGcrFormat.DataSearchBitCount), AppleIIGcrFormat.DataPrologue);
        if (dataOffset < 0) return new(null, null, headerEnd);
        pairedData.Add(dataOffset);
        var decoded = AppleIIGcrCodec.TryDecodeSixAndTwo(stream.Bits, dataOffset + AppleIIGcrFormat.PrologueBitCount);
        if (decoded is null)
        {
            structures.Add(new(FluxStructureKind.AppleData, dataOffset, AppleIIGcrFormat.PrologueBitCount, FluxStructureDescriptions.Truncated(AppleIIGcrFormat.StructureDescriptionName, FluxStructureKind.AppleData, null, AppleIIGcrFormat.UnavailableChecksumVariant)));
            return new(null, null, headerEnd);
        }
        bytes.AddRange(decoded.Value.Data);
        structures.Add(new(FluxStructureKind.AppleData, dataOffset, decoded.Value.EndOffset - dataOffset, FluxStructureDescriptions.WithIntegrity(AppleIIGcrFormat.StructureDescriptionName, FluxStructureKind.AppleData, address?.Cylinder ?? 0, AppleIIGcrFormat.LogicalHead, address?.Sector ?? 0, AppleIIGcrFormat.SectorSize, null, null, AppleIIGcrFormat.ChecksumLabel, decoded.Value.Valid)));
        return new(decoded.Value.Data, decoded.Value.Valid, decoded.Value.EndOffset);
    }

    private static AppleDataDecodeResult FindAndDecodeFiveAndThreeData(FluxBitstream stream, int headerEnd, AppleAddressDecodeResult? address, List<FluxStructure> structures, List<byte> bytes, HashSet<int> pairedData)
    {
        var dataOffset = Find(stream, headerEnd, Math.Min(stream.Bits.Length, headerEnd + AppleIIGcrFormat.DataSearchBitCount), AppleIIGcrFormat.DataPrologue);
        if (dataOffset < 0) return new(null, null, headerEnd);
        pairedData.Add(dataOffset);
        var decoded = AppleIIGcrCodec.TryDecodeFiveAndThree(stream.Bits, dataOffset + AppleIIGcrFormat.PrologueBitCount);
        if (decoded is null) return new(null, null, headerEnd);
        bytes.AddRange(decoded.Value.Data);
        structures.Add(new(FluxStructureKind.AppleData, dataOffset, decoded.Value.EndOffset - dataOffset, FluxStructureDescriptions.WithIntegrity(AppleIIGcrFormat.StructureDescriptionName, FluxStructureKind.AppleData, address?.Cylinder ?? 0, AppleIIGcrFormat.LogicalHead, address?.Sector ?? 0, AppleIIGcrFormat.SectorSize, null, AppleIIGcrFormat.ThirteenSectorVariant, AppleIIGcrFormat.ChecksumLabel, decoded.Value.Valid)));
        return new(decoded.Value.Data, decoded.Value.Valid, decoded.Value.EndOffset);
    }

    private static void AddSectorAndAddress(int offset, int headerEnd, AppleAddressDecodeResult? address, AppleDataDecodeResult data, string? variant, List<DecodedSector> sectors, List<FluxStructure> structures)
    {
        var volume = address?.Volume ?? 0;
        var cylinder = address?.Cylinder ?? 0;
        var sector = address?.Sector ?? 0;
        bool? integrity = address?.Valid == false || data.Valid == false ? false : data.Valid is null ? null : true;
        sectors.Add(new(cylinder, AppleIIGcrFormat.LogicalHead, sector, SectorSizeCode.FromByteCount(AppleIIGcrFormat.SectorSize), AppleIIGcrFormat.SectorSize, integrity, offset, SectorIntegrityKind.Checksum, data.Data));
        var description = variant is null
            ? FluxStructureDescriptions.Complete(AppleIIGcrFormat.StructureDescriptionName, FluxStructureKind.AppleAddress, cylinder, AppleIIGcrFormat.LogicalHead, sector, AppleIIGcrFormat.SectorSize, null, $"V{volume}", address?.Valid, data.Valid, AppleIIGcrFormat.AddressChecksumLabel, AppleIIGcrFormat.DataChecksumLabel)
            : FluxStructureDescriptions.WithIntegrity(AppleIIGcrFormat.StructureDescriptionName, FluxStructureKind.AppleAddress, cylinder, AppleIIGcrFormat.LogicalHead, sector, AppleIIGcrFormat.SectorSize, null, $"{variant} V{volume}", AppleIIGcrFormat.AddressChecksumLabel, address?.Valid);
        structures.Add(new(FluxStructureKind.AppleAddress, offset, variant is null ? Math.Max(AppleIIGcrFormat.PrologueBitCount, headerEnd - offset) : AppleIIGcrFormat.AddressBlockBitCount, description));
    }

    private sealed record AppleAddressDecodeResult(byte Volume, byte Cylinder, byte Sector, bool Valid);
    private sealed record AppleDataDecodeResult(byte[]? Data, bool? Valid, int StructureEnd);

    /// <summary>Exécute le traitement « Try Read Bytes » propre à ce format.</summary>
    /// <param name="bits">Bits source.</param><param name="offset">Offset de départ en bits.</param><param name="count">Nombre d'octets à lire.</param><returns>Octets lus, ou <see langword="null"/> si la plage est incomplète.</returns>
    private static byte[]? TryReadBytes(IReadOnlyList<bool> bits, int offset, int count)
    {
        if (offset + count * BitPrimitives.BitsPerByte > bits.Count) return null; var result = new byte[count];
        for (var index = 0; index < count; index++) for (var bit = 0; bit < BitPrimitives.BitsPerByte; bit++) if (bits[offset + index * BitPrimitives.BitsPerByte + bit]) result[index] |= (byte)(1 << (BitPrimitives.BitsPerByte - 1 - bit));
        return result;
    }
    /// <summary>Recherche le prochain motif dans la plage indiquée.</summary>
    /// <param name="stream">Flux binaire à parcourir.</param><param name="start">Offset de départ inclus, en bits.</param><param name="end">Offset de fin exclu, en bits.</param><param name="mark">Motif à rechercher.</param><returns>Offset du motif en bits, ou <c>-1</c> s'il est absent.</returns>
    private static int Find(FluxBitstream stream, int start, int end, uint mark)
    {
        for (var offset = Math.Max(0, start); offset + AppleIIGcrFormat.PrologueBitCount <= end; offset++) if (FluxBitReader.Match(stream, offset, mark, AppleIIGcrFormat.PrologueBitCount)) return offset;
        return -1;
    }
}
