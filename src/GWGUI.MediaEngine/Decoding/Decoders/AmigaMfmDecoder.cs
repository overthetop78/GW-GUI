using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Amiga MFM.</summary>
public sealed class AmigaMfmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => AmigaMfmFormat.CodecId;

    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => AmigaMfmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP dont les intervalles sont décodés selon le format MFM Amiga.</param>
    /// <returns>Résultat contenant les structures, secteurs, octets décodés et la durée estimée d'une cellule.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        const int encodedBytes = AmigaMfmFormat.EncodedSectorByteCount;
        const int headerBytes = AmigaMfmFormat.EncodedHeaderByteCount;
        for (var offset = 0; offset + AmigaMfmFormat.SyncBitCount <= stream.Bits.Length; offset++)
        {
            if (!HasSynchronizationAt(stream, offset)) continue;
            var encoded = FluxBitReader.TryDecodeMfmBytes(stream, offset + AmigaMfmFormat.SyncBitCount, encodedBytes);
            var available = encoded ?? FluxBitReader.TryDecodeMfmBytes(stream, offset + AmigaMfmFormat.SyncBitCount, headerBytes);
            var header = DecodeAndValidateHeader(available, bytes);
            var data = DecodeAndValidateData(encoded, bytes);
            var length = data?.Length ?? header?.Length ?? AmigaMfmFormat.SyncBitCount;
            AddSectorAndStructure(offset, length, header, data, sectors, structures);
            offset += Math.Max(AmigaMfmFormat.SyncBitCount - 1, length - 1);
        }
        return new(Id, DisplayName, FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, AmigaMfmFormat.ConfidenceSectorWeight, AmigaMfmFormat.ConfidenceDivisor), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Vérifie la présence des deux mots de synchronisation Amiga à une position du flux.</summary>
    /// <param name="stream">Flux binaire à examiner.</param>
    /// <param name="offset">Position du premier bit à examiner.</param>
    /// <returns><see langword="true"/> lorsque les deux mots de synchronisation sont présents.</returns>
    private static bool HasSynchronizationAt(FluxBitstream stream, int offset) => FluxBitReader.Match(stream, offset, AmigaMfmFormat.SyncWord) && FluxBitReader.Match(stream, offset + AmigaMfmFormat.EncodedByteBitCount, AmigaMfmFormat.SyncWord);

    /// <summary>Décode l'en-tête odd/even et contrôle son octet de format et sa parité.</summary>
    /// <param name="available">Octets MFM disponibles après la synchronisation.</param>
    /// <param name="bytes">Collection recevant les octets d'en-tête décodés.</param>
    /// <returns>En-tête décodé, ou <see langword="null"/> lorsque les octets requis sont absents.</returns>
    private static AmigaHeaderDecodeResult? DecodeAndValidateHeader(byte[]? available, List<byte> bytes)
    {
        if (available is null) return null;
        var header = AmigaMfmCodec.DecodeOddEven(available.Take(AmigaMfmFormat.InfoByteCount).ToArray());
        var cylinder = (byte)(header[AmigaMfmFormat.TrackAndHeadOffset] >> AmigaMfmFormat.TrackCylinderShift);
        var head = (byte)(header[AmigaMfmFormat.TrackAndHeadOffset] & AmigaMfmFormat.TrackHeadMask);
        var number = header[AmigaMfmFormat.SectorNumberOffset];
        var parity = AmigaMfmCodec.CalculateParity(available, 0, AmigaMfmFormat.HeaderParitySourceByteCount);
        var valid = header[AmigaMfmFormat.FormatByteOffset] == AmigaMfmFormat.FormatByte && available[AmigaMfmFormat.HeaderParityHighOffset] == parity.High && available[AmigaMfmFormat.HeaderParityLowOffset] == parity.Low;
        bytes.AddRange(header);
        return new(cylinder, head, number, valid, AmigaMfmFormat.SyncBitCount + available.Length * AmigaMfmFormat.EncodedByteBitCount);
    }

    /// <summary>Décode les données odd/even d'un secteur complet et contrôle leur parité.</summary>
    /// <param name="encoded">Bloc encodé complet, ou <see langword="null"/> lorsqu'il est tronqué.</param>
    /// <param name="bytes">Collection recevant les octets de données décodés.</param>
    /// <returns>Données décodées, ou <see langword="null"/> lorsque le bloc complet est absent.</returns>
    private static AmigaDataDecodeResult? DecodeAndValidateData(byte[]? encoded, List<byte> bytes)
    {
        if (encoded is null) return null;
        var parity = AmigaMfmCodec.CalculateSplitParity(encoded, AmigaMfmFormat.EncodedDataOffset, AmigaMfmFormat.EncodedDataByteCount);
        var valid = encoded[AmigaMfmFormat.DataParityHighOffset] == parity.High && encoded[AmigaMfmFormat.DataParityLowOffset] == parity.Low;
        var payload = AmigaMfmCodec.DecodeOddEven(encoded.Skip(AmigaMfmFormat.EncodedDataOffset).Take(AmigaMfmFormat.EncodedDataByteCount).ToArray());
        bytes.AddRange(payload);
        return new(payload, valid, AmigaMfmFormat.SyncBitCount + AmigaMfmFormat.EncodedSectorByteCount * AmigaMfmFormat.EncodedByteBitCount);
    }

    /// <summary>Construit le secteur décodé et la structure de flux associés à une synchronisation.</summary>
    /// <param name="offset">Position de la synchronisation, en bits.</param>
    /// <param name="length">Longueur de la structure reconnue, en bits.</param>
    /// <param name="header">Résultat du décodage de l'en-tête.</param>
    /// <param name="data">Résultat du décodage des données.</param>
    /// <param name="sectors">Collection recevant le secteur.</param>
    /// <param name="structures">Collection recevant la structure de flux.</param>
    private static void AddSectorAndStructure(int offset, int length, AmigaHeaderDecodeResult? header, AmigaDataDecodeResult? data, List<DecodedSector> sectors, List<FluxStructure> structures)
    {
        var cylinder = header?.Cylinder ?? 0;
        var head = header?.Head ?? 0;
        var number = header?.Sector ?? 0;
        bool? integrity = header?.Valid == false || data?.Valid == false ? false : data is null ? null : true;
        sectors.Add(new(cylinder, head, number, SectorSizeCode.FromByteCount(AmigaMfmFormat.SectorByteCount), AmigaMfmFormat.SectorByteCount, integrity, offset, SectorIntegrityKind.Checksum, data?.Payload));
        structures.Add(new(FluxStructureKind.AmigaSync, offset, length, FluxStructureDescriptions.CompleteWithChecksums(AmigaMfmFormat.StructureDescriptionName, FluxStructureKind.AmigaSync, cylinder, head, number, AmigaMfmFormat.SectorByteCount, header?.Valid, data?.Valid)));
    }

    /// <summary>Regroupe l'identité, la validité et la longueur en bits d'un en-tête Amiga décodé.</summary>
    /// <param name="Cylinder">Numéro de cylindre.</param><param name="Head">Numéro de face.</param><param name="Sector">Numéro de secteur.</param><param name="Valid">Validité du format et de la parité d'en-tête.</param><param name="Length">Longueur reconnue, en bits.</param>
    private sealed record AmigaHeaderDecodeResult(byte Cylinder, byte Head, byte Sector, bool Valid, int Length);

    /// <summary>Regroupe les données, leur validité et la longueur en bits d'un secteur Amiga décodé.</summary>
    /// <param name="Payload">Données utiles décodées.</param><param name="Valid">Validité de la parité des données.</param><param name="Length">Longueur reconnue, en bits.</param>
    private sealed record AmigaDataDecodeResult(byte[] Payload, bool Valid, int Length);
}
