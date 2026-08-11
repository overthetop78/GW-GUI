using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode le format sectoriel GCR commun aux contrôleurs Apple IWM Macintosh et Lisa FileWare.</summary>
public abstract class AppleIwmGcrDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique de la spécialisation.</summary>
    public abstract string Id { get; }
    /// <summary>Obtient le nom affiché de la spécialisation.</summary>
    public abstract string DisplayName { get; }

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder en NRZI Macintosh.</param><returns>Résultat du décodage.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution) => DecodeCore(FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals));

    /// <summary>Décode directement les bits d'une piste Macintosh.</summary>
    /// <param name="bits">Bits de la piste.</param><returns>Résultat du décodage.</returns>
    internal FluxDecodeResult DecodeBits(bool[] bits) => DecodeCore(new FluxBitstream(bits, 1));

    /// <summary>Exécute le traitement « Decode At Bit Cell » propre à ce format.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><param name="bitCellTicks">Durée imposée d'une cellule en ticks.</param><returns>Résultat du décodage.</returns>
    public FluxDecodeResult DecodeAtBitCell(FluxRevolution revolution, double bitCellTicks) => DecodeCore(FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals, bitCellTicks));

    /// <summary>Exécute le traitement « Decode Core » propre à ce format.</summary>
    /// <param name="stream">Flux binaire décodé.</param><returns>Structures, secteurs et octets reconnus.</returns>
    private FluxDecodeResult DecodeCore(FluxBitstream stream)
    {
        var trackBitLength = stream.Bits.Length;
        stream = stream.WithCircularTail(AppleIwmGcrFormat.CircularTailBitCount);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        var pairedData = new HashSet<int>();
        const int markBits = AppleIwmGcrFormat.MarkBitCount;
        const int headerSymbols = AppleIwmGcrFormat.HeaderSymbolCount;
        for (var offset = 0; offset < trackBitLength && offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, AppleIwmGcrFormat.AddressMark)) continue;
            var header = TryReadSymbols(stream, offset + markBits, headerSymbols);
            var address = DecodeAddress(header);
            var headerEnd = offset + markBits + (header is null ? 0 : headerSymbols * BitPrimitives.BitsPerByte);
            var data = FindAndDecodeData(stream, headerEnd, address, structures, bytes, pairedData);
            AddSectorAndAddress(offset, headerEnd, address, data, sectors, structures);
            offset = address?.Valid == true ? Math.Max(offset + AppleIwmGcrFormat.MarkAdvanceBitCount, data.StructureEnd - 1) : offset + AppleIwmGcrFormat.MarkAdvanceBitCount;
        }
        AddUnpairedDataStructures(stream, trackBitLength, pairedData, structures);
        return new(Id, DisplayName, FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, AppleIwmGcrFormat.ConfidenceSectorWeight, AppleIwmGcrFormat.ConfidenceDivisor), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    /// <summary>Décode les valeurs d'une adresse IWM et valide son checksum.</summary>
    /// <param name="header">Symboles GCR de l'adresse.</param><returns>Adresse décodée, ou <see langword="null"/> si les symboles sont absents ou inconnus.</returns>
    private static AppleIwmAddressDecodeResult? DecodeAddress(byte[]? header)
    {
        if (header is null || !header.All(AppleIIGcrFormat.InverseSixAndTwoTable.ContainsKey)) return null;
        var values = header.Select(value => AppleIIGcrFormat.InverseSixAndTwoTable[value]).ToArray();
        var cylinder = (byte)(((values[2] & AppleIwmGcrFormat.CylinderHighBitMask) << AppleIwmGcrFormat.CylinderHighBitShift) | (values[0] & AppleIwmGcrFormat.SixBitMask));
        var head = (byte)((values[2] >> AppleIwmGcrFormat.HeadBitShift) & AppleIwmGcrFormat.HeadBitMask);
        var valid = (byte)((values[0] ^ values[1] ^ values[2] ^ values[3]) & AppleIwmGcrFormat.SixBitMask) == values[4];
        return new(cylinder, head, values[1], values[3], valid);
    }

    /// <summary>Recherche le bloc de données apparié, lit ses tags et données puis valide son checksum.</summary>
    /// <param name="stream">Flux à parcourir.</param><param name="headerEnd">Position suivant l'adresse, en bits.</param><param name="address">Adresse décodée.</param><param name="structures">Collection recevant la structure.</param><param name="bytes">Collection recevant les données décodées.</param><param name="pairedData">Positions des marques déjà appariées.</param><returns>Résultat des données et position finale reconnue.</returns>
    private static AppleIwmDataDecodeResult FindAndDecodeData(FluxBitstream stream, int headerEnd, AppleIwmAddressDecodeResult? address, List<FluxStructure> structures, List<byte> bytes, HashSet<int> pairedData)
    {
        var dataOffset = address?.Valid == true ? FindMark(stream, headerEnd, Math.Min(stream.Bits.Length, headerEnd + AppleIwmGcrFormat.DataSearchBitCount), AppleIwmGcrFormat.DataMark) : -1;
        if (dataOffset < 0) return new(null, null, null, headerEnd);
        pairedData.Add(dataOffset);
        var encoded = TryReadSymbols(stream, dataOffset + AppleIwmGcrFormat.MarkBitCount, AppleIwmGcrFormat.DataSymbolCount);
        if (encoded is null || !encoded.All(AppleIIGcrFormat.InverseSixAndTwoTable.ContainsKey))
        {
            structures.Add(new(FluxStructureKind.AppleData, dataOffset, AppleIwmGcrFormat.MarkBitCount, FluxStructureDescriptions.Truncated(AppleIwmGcrFormat.StructureDescriptionName, FluxStructureKind.AppleData, null, AppleIwmGcrFormat.UnavailableChecksumVariant)));
            return new(null, null, null, headerEnd);
        }
        var values = encoded.Select(value => AppleIIGcrFormat.InverseSixAndTwoTable[value]).ToArray();
        var decoded = AppleIwmGcrCodec.Decode(values.AsSpan(1, AppleIwmGcrFormat.EncodedPayloadSymbolCount), out var checksum);
        var valid = checksum[3] == values[AppleIwmGcrFormat.PackedChecksumSymbolOffset] && checksum[2] == values[AppleIwmGcrFormat.ThirdChecksumSymbolOffset] && checksum[1] == values[AppleIwmGcrFormat.SecondChecksumSymbolOffset] && checksum[0] == values[AppleIwmGcrFormat.FirstChecksumSymbolOffset];
        var tag = decoded.Take(AppleIwmGcrFormat.TagByteCount).ToArray();
        var data = decoded.Skip(AppleIwmGcrFormat.TagByteCount).Take(AppleIwmGcrFormat.SectorByteCount).ToArray();
        var structureEnd = dataOffset + AppleIwmGcrFormat.MarkBitCount + AppleIwmGcrFormat.DataSymbolCount * BitPrimitives.BitsPerByte;
        bytes.AddRange(data);
        structures.Add(new(FluxStructureKind.AppleData, dataOffset, structureEnd - dataOffset, FluxStructureDescriptions.WithIntegrity(AppleIwmGcrFormat.StructureDescriptionName, FluxStructureKind.AppleData, address?.Cylinder ?? 0, address?.Head ?? 0, address?.Sector ?? 0, AppleIwmGcrFormat.SectorByteCount, null, null, AppleIwmGcrFormat.ChecksumLabel, valid)));
        return new(data, tag, valid, structureEnd);
    }

    /// <summary>Ajoute le secteur IWM et la structure décrivant son adresse.</summary>
    /// <param name="offset">Position de l'adresse, en bits.</param><param name="headerEnd">Position suivant l'adresse, en bits.</param><param name="address">Adresse décodée.</param><param name="data">Données décodées.</param><param name="sectors">Collection recevant le secteur.</param><param name="structures">Collection recevant la structure.</param>
    private static void AddSectorAndAddress(int offset, int headerEnd, AppleIwmAddressDecodeResult? address, AppleIwmDataDecodeResult data, List<DecodedSector> sectors, List<FluxStructure> structures)
    {
        bool? integrity = address?.Valid == false || data.Valid == false ? false : data.Valid is null ? null : true;
        sectors.Add(new(address?.Cylinder ?? 0, address?.Head ?? 0, address?.Sector ?? 0, SectorSizeCode.FromByteCount(AppleIwmGcrFormat.SectorByteCount), AppleIwmGcrFormat.SectorByteCount, integrity, offset, SectorIntegrityKind.Checksum, data.Data, data.Tag, address?.Format));
        structures.Add(new(FluxStructureKind.AppleAddress, offset, Math.Max(AppleIwmGcrFormat.MarkBitCount, headerEnd - offset), FluxStructureDescriptions.Complete(AppleIwmGcrFormat.StructureDescriptionName, FluxStructureKind.AppleAddress, address?.Cylinder ?? 0, address?.Head ?? 0, address?.Sector ?? 0, AppleIwmGcrFormat.SectorByteCount, null, null, address?.Valid, data.Valid, AppleIwmGcrFormat.AddressChecksumLabel, AppleIwmGcrFormat.DataChecksumLabel)));
    }

    /// <summary>Ajoute les marques de données qui n'ont été associées à aucune adresse.</summary>
    /// <param name="stream">Flux à parcourir.</param><param name="trackBitLength">Longueur logique de la piste, en bits.</param><param name="pairedData">Positions déjà appariées.</param><param name="structures">Collection recevant les structures.</param>
    private static void AddUnpairedDataStructures(FluxBitstream stream, int trackBitLength, HashSet<int> pairedData, List<FluxStructure> structures)
    {
        for (var offset = 0; offset < trackBitLength && offset + AppleIwmGcrFormat.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, AppleIwmGcrFormat.DataMark) || pairedData.Contains(offset)) continue;
            structures.Add(new(FluxStructureKind.AppleData, offset, AppleIwmGcrFormat.MarkBitCount, FluxStructureDescriptions.UnpairedData(AppleIwmGcrFormat.StructureDescriptionName, null, AppleIwmGcrFormat.UnpairedDataVariant)));
            offset += AppleIwmGcrFormat.MarkAdvanceBitCount;
        }
    }

    /// <summary>Regroupe l'identité, le format et la validité d'une adresse IWM.</summary>
    /// <param name="Cylinder">Numéro de cylindre.</param><param name="Head">Numéro de face.</param><param name="Sector">Numéro de secteur.</param><param name="Format">Octet de format.</param><param name="Valid">Validité du checksum d'adresse.</param>
    private sealed record AppleIwmAddressDecodeResult(byte Cylinder, byte Head, byte Sector, byte Format, bool Valid);
    /// <summary>Regroupe les données, les tags, la validité et la position finale d'un bloc IWM.</summary>
    /// <param name="Data">Données sectorielles.</param><param name="Tag">Tags sectoriels.</param><param name="Valid">Validité du checksum des données.</param><param name="StructureEnd">Position suivant la structure, en bits.</param>
    private sealed record AppleIwmDataDecodeResult(byte[]? Data, byte[]? Tag, bool? Valid, int StructureEnd);

    /// <summary>Exécute le traitement « Try Read Symbols » propre à ce format.</summary>
    /// <param name="stream">Flux binaire source.</param><param name="offset">Offset de départ en bits.</param><param name="count">Nombre de symboles à lire.</param><returns>Symboles lus, ou <see langword="null"/> si la plage est incomplète.</returns>
    private static byte[]? TryReadSymbols(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * BitPrimitives.BitsPerByte > stream.Bits.Length) return null;
        var result = new byte[count];
        for (var index = 0; index < count; index++)
            if (!FluxBitReader.TryDecodeByte(stream, offset + index * BitPrimitives.BitsPerByte, out result[index])) return null;
        return result;
    }
    /// <summary>Exécute le traitement « Find Mark » propre à ce format.</summary>
    /// <param name="stream">Flux à parcourir.</param><param name="start">Offset de départ inclus, en bits.</param><param name="end">Offset de fin exclu, en bits.</param><param name="mark">Marque recherchée.</param><returns>Offset de la marque en bits, ou <c>-1</c>.</returns>
    private static int FindMark(FluxBitstream stream, int start, int end, IReadOnlyList<byte> mark)
    {
        for (var offset = start; offset + mark.Count * BitPrimitives.BitsPerByte <= end; offset++)
            if (FluxBitReader.MatchBytes(stream, offset, mark)) return offset;
        return -1;
    }
}
