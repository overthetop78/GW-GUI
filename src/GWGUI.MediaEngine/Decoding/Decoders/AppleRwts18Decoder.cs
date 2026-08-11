using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Decoding.Apple;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode le format de piste Brøderbund RWTS18 de Roland Gustafsson : six secteurs physiques de 768 octets, formés chacun de trois pages indépendantes de 256 octets.</summary>
public sealed class AppleRwts18Decoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => AppleRwts18Format.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => AppleRwts18Format.CodecDisplayName;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder en NRZI RWTS18.</param><returns>Résultat du décodage.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution) => DecodeCore(FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals));
    /// <summary>Décode directement les bits d'une piste RWTS18.</summary>
    /// <param name="bits">Bits de la piste.</param><returns>Résultat du décodage.</returns>
    internal FluxDecodeResult DecodeBits(bool[] bits) => DecodeCore(new FluxBitstream(bits, 1));

    /// <summary>Exécute le traitement « Decode Core » propre à ce format.</summary>
    /// <param name="source">Flux binaire source.</param><returns>Structures, secteurs et octets RWTS18 reconnus.</returns>
    private FluxDecodeResult DecodeCore(FluxBitstream source)
    {
        var trackBitLength = source.Bits.Length;
        var stream = source.WithCircularTail(AppleRwts18Format.CircularTailBitCount);
        var structures = new List<FluxStructure>();
        var decodedBytes = new List<byte>();
        var sectors = new List<DecodedSector>();

        for (var offset = 0; offset + AppleRwts18Format.AddressMarkBitCount <= trackBitLength; offset++)
        {
            if (!FluxBitReader.Match(stream, offset, AppleRwts18Format.EncodedAddressMark, AppleRwts18Format.AddressMarkBitCount)) continue;
            var address = TryReadAddress(stream.Bits, offset);
            if (address is null) continue;
            var data = TryReadData(stream.Bits, address.Cursor);
            AddSectorAndStructures(offset, address, data, structures, decodedBytes, sectors);
            offset = Math.Max(offset + AppleRwts18Format.AddressMarkAdvanceBitCount, (data?.EndOffset ?? address.Cursor) - 1);
        }

        var valid = sectors.Count(sector => sector.IntegrityValid == true);
        var confidence = FluxDecoderConfidence.CalculateByValidity(valid, sectors.Count, AppleRwts18Format.ConfidenceCompleteSectorDivisor, AppleRwts18Format.ConfidenceDetectedSectorDivisor);
        return new(Id, DisplayName, confidence, source.BitCellTicks, structures, decodedBytes, sectors);
    }

    /// <summary>Lit et valide l'adresse RWTS18 suivant une marque reconnue.</summary>
    /// <param name="bits">Bits de la piste.</param><param name="offset">Position de la marque d'adresse, en bits.</param><returns>Adresse validée, ou <see langword="null"/> si un champ est invalide.</returns>
    private static AppleRwts18Address? TryReadAddress(IReadOnlyList<bool> bits, int offset)
    {
        var cursor = offset + AppleRwts18Format.AddressMarkBitCount;
        var address = AppleBitLatch.TryReadBytes(bits, ref cursor, AppleRwts18Format.AddressByteCount);
        if (address is null || !AppleIIGcrFormat.InverseSixAndTwoTable.TryGetValue(address[0], out var track) || !AppleIIGcrFormat.InverseSixAndTwoTable.TryGetValue(address[1], out var sector) || !AppleIIGcrFormat.InverseSixAndTwoTable.TryGetValue(address[2], out var checksum) || address[3] != AppleRwts18Format.AddressTrailer || sector >= AppleRwts18Format.SectorCount || (byte)(track ^ sector) != checksum) return null;
        return new(track, sector, cursor);
    }

    /// <summary>Ajoute les structures d'adresse et de données ainsi que le secteur RWTS18.</summary>
    /// <param name="offset">Position de l'adresse, en bits.</param><param name="address">Adresse validée.</param><param name="data">Données éventuellement reconnues.</param><param name="structures">Collection recevant les structures.</param><param name="decodedBytes">Collection recevant les octets décodés.</param><param name="sectors">Collection recevant le secteur.</param>
    private static void AddSectorAndStructures(int offset, AppleRwts18Address address, (byte[] Data, bool Valid, int StartOffset, int EndOffset)? data, List<FluxStructure> structures, List<byte> decodedBytes, List<DecodedSector> sectors)
    {
        structures.Add(new(FluxStructureKind.AppleAddress, offset, address.Cursor - offset, FluxStructureDescriptions.WithIntegrity(AppleRwts18Format.StructureDescriptionName, FluxStructureKind.AppleAddress, address.Track, AppleRwts18Format.LogicalHead, address.Sector, AppleRwts18Format.SectorByteCount, null, null, AppleRwts18Format.AddressChecksumLabel, true)));
        if (data is not null)
        {
            structures.Add(new(FluxStructureKind.AppleData, data.Value.StartOffset, data.Value.EndOffset - data.Value.StartOffset, FluxStructureDescriptions.WithIntegrity(AppleRwts18Format.StructureDescriptionName, FluxStructureKind.AppleData, address.Track, AppleRwts18Format.LogicalHead, address.Sector, AppleRwts18Format.SectorByteCount, null, null, AppleRwts18Format.DataChecksumLabel, data.Value.Valid)));
            decodedBytes.AddRange(data.Value.Data);
        }
        sectors.Add(new(address.Track, AppleRwts18Format.LogicalHead, address.Sector, SectorSizeCode.FromByteCount(AppleRwts18Format.SectorByteCount), AppleRwts18Format.SectorByteCount, data?.Valid, offset, SectorIntegrityKind.Checksum, data?.Data));
    }

    /// <summary>Regroupe la piste, le secteur et la position suivant une adresse RWTS18.</summary>
    /// <param name="Track">Numéro de piste.</param><param name="Sector">Numéro de secteur physique.</param><param name="Cursor">Position suivant l'adresse, en bits.</param>
    private sealed record AppleRwts18Address(byte Track, byte Sector, int Cursor);

    /// <summary>Recherche et lit le bloc de données RWTS18 suivant une adresse.</summary>
    /// <param name="bits">Bits de la piste.</param><param name="offset">Position de recherche initiale, en bits.</param><returns>Données, validité et limites du bloc, ou <see langword="null"/> si aucun bloc n'est validé.</returns>
    private static (byte[] Data, bool Valid, int StartOffset, int EndOffset)? TryReadData(IReadOnlyList<bool> bits, int offset)
    {
        var cursor = offset;
        var stream = AppleBitLatch.TryReadBytes(bits, ref cursor, AppleRwts18Format.DataReadWindowByteCount);
        if (stream is null) return null;
        // Le premier octet est un identifiant Brøderbund modifiable. Il est repéré grâce
        // aux 1 025 symboles GCR valides consécutifs suivis de l'épilogue D4.
        for (var start = 0; start + AppleRwts18Format.DataRecordByteCount <= stream.Length; start++)
        {
            var decoded = TryDecodeDataRecord(stream, start);
            if (decoded is null) continue;
            var startOffset = offset + start * BitPrimitives.BitsPerByte;
            var endOffset = offset + (start + AppleRwts18Format.DataRecordByteCount) * BitPrimitives.BitsPerByte;
            return (decoded.Value.Data, decoded.Value.Valid, startOffset, endOffset);
        }
        return null;
    }

    /// <summary>Convertit les symboles d'un enregistrement et valide son épilogue.</summary>
    /// <param name="stream">Octets lus dans la fenêtre RWTS18.</param><param name="start">Position de l'identifiant modifiable.</param><returns>Données et validité du checksum, ou <see langword="null"/> si les symboles ou l'épilogue sont invalides.</returns>
    private static (byte[] Data, bool Valid)? TryDecodeDataRecord(IReadOnlyList<byte> stream, int start)
    {
        var values = new byte[AppleRwts18Format.PayloadWithChecksumSymbolCount];
        for (var index = 0; index < values.Length; index++)
            if (!AppleIIGcrFormat.InverseSixAndTwoTable.TryGetValue(stream[start + AppleRwts18Format.PayloadOffset + index], out values[index])) return null;
        if (stream[start + AppleRwts18Format.DataEpilogueOffset] != AppleRwts18Format.DataEpilogue) return null;
        return AppleRwts18Codec.DecodePayload(values);
    }

}
