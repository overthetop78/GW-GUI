using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les secteurs GCR zonés de 512 octets utilisés par le Commodore 900.</summary>
public sealed class Commodore900GcrDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => Commodore900GcrFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => Commodore900GcrFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder en GCR Commodore 900.</param>
    /// <returns>Résultat contenant les structures, secteurs et octets reconnus.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals);
        var collection = CollectBlocks(stream);
        var pairing = PairBlocks(collection);
        var structures = collection.Structures.ToList();
        structures.AddRange(pairing.Structures);
        var validCount = pairing.Sectors.Count(sector => sector.IntegrityValid == true);
        var confidence = FluxDecoderConfidence.Calculate(validCount, 0, 1, Commodore900GcrFormat.ExpectedSectorCount);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures.OrderBy(structure => structure.BitOffset).ToArray(), collection.DecodedBytes, pairing.Sectors);
    }

    /// <summary>Apparie chaque en-tête au premier bloc de données précédant l'en-tête suivant.</summary>
    /// <param name="collection">Blocs collectés pendant le balayage.</param>
    /// <returns>Secteurs et structures produits par l'appariement.</returns>
    private static Commodore900PairingResult PairBlocks(Commodore900BlockCollection collection)
    {
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var pairedData = new HashSet<Commodore900DataBlock>();
        for (var index = 0; index < collection.Headers.Count; index++)
        {
            var header = collection.Headers[index];
            var nextHeaderOffset = index + 1 < collection.Headers.Count ? collection.Headers[index + 1].Offset : int.MaxValue;
            var data = collection.DataBlocks.FirstOrDefault(candidate => candidate.Offset > header.EndOffset && candidate.Offset < nextHeaderOffset);
            if (data is not null) pairedData.Add(data);
            bool? dataValid = data?.ChecksumValid;
            bool? integrity = !header.ChecksumValid || dataValid == false ? false : dataValid is null ? null : true;
            var payload = data?.Bytes.Skip(Commodore900GcrFormat.DataPayloadOffset).Take(Commodore900GcrFormat.SectorByteCount).ToArray();
            sectors.Add(new(header.Cylinder, Commodore900GcrFormat.LogicalHead, header.Sector, Commodore900GcrFormat.SectorSizeCode, Commodore900GcrFormat.SectorByteCount, integrity, header.Offset, SectorIntegrityKind.Checksum, payload));
            structures.Add(new(FluxStructureKind.CommodoreHeader, header.Offset, Math.Max(Commodore900GcrFormat.MinimumSyncBitCount, header.EndOffset - header.Offset), Commodore900GcrDescriptions.Header(header.Cylinder, header.Sector, header.ChecksumValid)));
            if (data is not null) structures.Add(new(FluxStructureKind.FormatData, data.Offset, Math.Max(Commodore900GcrFormat.MinimumSyncBitCount, data.EndOffset - data.Offset), Commodore900GcrDescriptions.Data(header.Cylinder, header.Sector, data.ChecksumValid)));
        }
        foreach (var data in collection.DataBlocks.Where(data => !pairedData.Contains(data))) structures.Add(new(FluxStructureKind.FormatData, data.Offset, Math.Max(Commodore900GcrFormat.MinimumSyncBitCount, data.EndOffset - data.Offset), Commodore900GcrDescriptions.UnpairedData(data.ChecksumValid)));
        return new(sectors, structures);
    }

    /// <summary>Balaye les synchronisations et collecte les en-têtes et blocs de données complets.</summary>
    /// <param name="stream">Flux binaire GCR.</param>
    /// <returns>Blocs, synchronisations et octets décodés.</returns>
    private static Commodore900BlockCollection CollectBlocks(FluxBitstream stream)
    {
        var structures = new List<FluxStructure>();
        var decodedBytes = new List<byte>();
        var headers = new List<Commodore900HeaderBlock>();
        var dataBlocks = new List<Commodore900DataBlock>();
        for (var offset = 0; offset < stream.Bits.Length; offset++)
        {
            if (!stream.Bits[offset]) continue;
            var end = offset;
            while (end < stream.Bits.Length && stream.Bits[end]) end++;
            if (end - offset < Commodore900GcrFormat.MinimumSyncBitCount)
            {
                offset = end;
                continue;
            }
            structures.Add(new(FluxStructureKind.CommodoreSync, offset, end - offset, Commodore900GcrDescriptions.Sync()));
            var header = CommodoreGcrCodec.TryDecodeBytes(stream.Bits, end, Commodore900GcrFormat.HeaderByteCount);
            if (header is not null && header[Commodore900GcrFormat.HeaderMarkOffset] == Commodore900GcrFormat.HeaderMark)
            {
                headers.Add(new(offset, end + Commodore900GcrFormat.EncodedHeaderBitCount, header, header[Commodore900GcrFormat.HeaderCylinderOffset], header[Commodore900GcrFormat.HeaderSectorOffset], CommodoreGcrChecksum.IsValid(header)));
                decodedBytes.AddRange(header);
            }
            else
            {
                var data = CommodoreGcrCodec.TryDecodeBytes(stream.Bits, end, Commodore900GcrFormat.DataRecordByteCount);
                if (data is not null && data[Commodore900GcrFormat.DataMarkOffset] == Commodore900GcrFormat.DataMark)
                {
                    dataBlocks.Add(new(offset, end + Commodore900GcrFormat.EncodedDataRecordBitCount, data, CommodoreGcrChecksum.IsValid(data)));
                    decodedBytes.AddRange(data);
                }
            }
            offset = end;
        }
        return new(headers, dataBlocks, structures, decodedBytes);
    }

    /// <summary>Représente un en-tête Commodore 900 décodé.</summary>
    /// <param name="Offset">Position de la synchronisation.</param><param name="EndOffset">Position suivant l'en-tête.</param><param name="Bytes">Octets décodés.</param><param name="Cylinder">Cylindre.</param><param name="Sector">Secteur.</param><param name="ChecksumValid">Validité du checksum.</param>
    private sealed record Commodore900HeaderBlock(int Offset, int EndOffset, byte[] Bytes, byte Cylinder, byte Sector, bool ChecksumValid);
    /// <summary>Représente un bloc de données Commodore 900 décodé.</summary>
    /// <param name="Offset">Position de la synchronisation.</param><param name="EndOffset">Position suivant le bloc.</param><param name="Bytes">Octets décodés.</param><param name="ChecksumValid">Validité du checksum.</param>
    private sealed record Commodore900DataBlock(int Offset, int EndOffset, byte[] Bytes, bool ChecksumValid);
    /// <summary>Regroupe les éléments collectés pendant le balayage des synchronisations.</summary>
    /// <param name="Headers">En-têtes.</param><param name="DataBlocks">Blocs de données.</param><param name="Structures">Synchronisations reconnues.</param><param name="DecodedBytes">Octets décodés.</param>
    private sealed record Commodore900BlockCollection(IReadOnlyList<Commodore900HeaderBlock> Headers, IReadOnlyList<Commodore900DataBlock> DataBlocks, IReadOnlyList<FluxStructure> Structures, IReadOnlyList<byte> DecodedBytes);
    /// <summary>Regroupe les secteurs et structures produits par l'appariement.</summary>
    /// <param name="Sectors">Secteurs reconstruits.</param><param name="Structures">Structures d'en-tête et de données.</param>
    private sealed record Commodore900PairingResult(IReadOnlyList<DecodedSector> Sectors, IReadOnlyList<FluxStructure> Structures);
}
