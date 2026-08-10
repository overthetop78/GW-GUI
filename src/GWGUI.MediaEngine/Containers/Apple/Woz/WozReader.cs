using System.Buffers.Binary;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Recognition.Apple;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Apple.Woz;

/// <summary>
/// Lit les conteneurs WOZ1 et WOZ2 Apple II, extrait leurs flux de bits par piste
/// et transmet chaque piste aux décodeurs GCR Apple II et RWTS18.
/// </summary>
internal static class WozReader
{
    /// <summary>
    /// Valide l’en-tête et les chunks structurants d’un conteneur WOZ1 ou WOZ2,
    /// sélectionne la meilleure lecture de chaque piste puis construit l’image sectorielle correspondante.
    /// </summary>
    /// <param name="data">Octets complets du conteneur WOZ.</param>
    /// <returns>L’image sectorielle Apple II reconstruite à partir des pistes décodées.</returns>
    /// <exception cref="InvalidDataException">
    /// L’en-tête ou le CRC32 est invalide, un chunk est tronqué, ou un chunk obligatoire est absent.
    /// </exception>
    /// <exception cref="NotSupportedException">Le conteneur ne décrit pas une disquette Apple II 5,25 pouces.</exception>
    /// <exception cref="OverflowException">Une longueur ou une position déclarée dépasse les limites des entiers utilisés.</exception>
    public static SectorImage Read(ReadOnlySpan<byte> data)
    {
        if (data.Length < WozLayout.MinimumFileLength ||
            !(data[..WozLayout.SignatureLength].SequenceEqual(WozFormat.Version1Signature) ||
              data[..WozLayout.SignatureLength].SequenceEqual(WozFormat.Version2Signature)) ||
            !data.Slice(WozLayout.HeaderMarkerOffset, WozLayout.HeaderMarkerLength).SequenceEqual(WozFormat.HeaderMarker))
            throw WozExceptions.InvalidHeader();
        var storedCrc = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(WozLayout.CrcOffset, WozLayout.CrcLength));
        var computedCrc = ComputeCrc32(data[WozLayout.ChunksOffset..]);
        if (storedCrc != computedCrc) throw WozExceptions.InvalidCrc(storedCrc, computedCrc);
        var version = data[..WozLayout.SignatureLength].SequenceEqual(WozFormat.Version1Signature) ? 1 : 2;
        var chunks = ReadChunks(data);
        if (!chunks.TryGetValue(WozFormat.InfoChunkId, out var info) || info.Length < WozLayout.MinimumInfoLength)
            throw WozExceptions.MissingRequiredChunk(WozFormat.InfoChunkId);
        if (info.Span[WozLayout.InfoDiskTypeOffset] != WozFormat.AppleII525DiskType)
            throw WozExceptions.UnsupportedDiskType(info.Span[WozLayout.InfoDiskTypeOffset]);
        if (!chunks.TryGetValue(WozFormat.TrackMapChunkId, out var tmap) || tmap.Length < WozLayout.TrackMapLength)
            throw WozExceptions.MissingRequiredChunk(WozFormat.TrackMapChunkId);
        if (!chunks.TryGetValue(WozFormat.TracksChunkId, out var trks))
            throw WozExceptions.MissingRequiredChunk(WozFormat.TracksChunkId);

        var decoder = new AppleGcrDecoder();
        var rwtsDecoder = new AppleRwts18Decoder();
        var tracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        var rwtsTracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        for (var track = 0; track < WozLayout.AppleIITrackCount; track++)
        {
            IReadOnlyList<DecodedSector>? best = null;
            IReadOnlyList<DecodedSector>? bestRwts = null;
            var bestScore = -1;
            var bestRwtsScore = -1;
            foreach (var descriptor in tmap.Span
                         .Slice(track * WozLayout.TrackMapEntriesPerTrack, WozLayout.TrackMapEntriesPerTrack)
                         .ToArray()
                         .Where(value => value != WozLayout.MissingTrackDescriptor)
                         .Distinct())
            {
                var bits = version == 1
                    ? ReadWoz1Track(trks.Span, track, descriptor)
                    : ReadWoz2Track(data, trks.Span, track, descriptor);
                if (bits.Length == 0) continue;
                var sectors = (decoder.DecodeBits(bits).Sectors ?? [])
                    .Where(sector => sector.Cylinder == track && sector.Number is >= 0 and < 16 && sector.Data is { Count: 256 })
                    .ToArray();
                var score = sectors.Select(sector => sector.Number).Distinct().Count() * 100
                    + sectors.Count(sector => sector.IntegrityValid == true) * 10 + sectors.Length;
                var rwts = (rwtsDecoder.DecodeBits(bits).Sectors ?? [])
                    .Where(sector => sector.Cylinder == track && sector.Number is >= 0 and < 6 && sector.Data is { Count: 768 })
                    .ToArray();
                var rwtsScore = rwts.Select(sector => sector.Number).Distinct().Count() * 100
                    + rwts.Count(sector => sector.IntegrityValid == true) * 10 + rwts.Length;
                if (score > bestScore)
                {
                    best = sectors;
                    bestScore = score;
                }
                if (rwtsScore > bestRwtsScore)
                {
                    bestRwts = rwts;
                    bestRwtsScore = rwtsScore;
                }
            }
            if (best is not null) tracks.Add((track, best));
            if (bestRwts is not null) rwtsTracks.Add((track, bestRwts));
        }
        if (rwtsTracks.Count(item => item.Sectors.Count > 0) > 1)
            return AppleDiskImageReader.CreateRwts18FromDecodedTracks(rwtsTracks);
        return AppleDiskImageReader.CreateAppleIIFromDecodedTracks(tracks);
    }

    /// <summary>
    /// Parcourt la table linéaire des chunks WOZ située après l’en-tête de douze octets.
    /// Chaque entrée contient un identifiant ASCII de quatre octets, une longueur little-endian de quatre octets,
    /// puis la charge utile annoncée.
    /// </summary>
    /// <param name="data">Conteneur WOZ complet.</param>
    /// <returns>Dictionnaire des charges utiles indexées par leur identifiant de chunk.</returns>
    /// <exception cref="InvalidDataException">La charge utile annoncée d’un chunk sort du conteneur.</exception>
    /// <exception cref="OverflowException">La longueur 32 bits d’un chunk ne peut pas être représentée.</exception>
    private static Dictionary<string, ReadOnlyMemory<byte>> ReadChunks(ReadOnlySpan<byte> data)
    {
        var chunks = new Dictionary<string, ReadOnlyMemory<byte>>(StringComparer.Ordinal);
        var offset = WozLayout.ChunksOffset;
        while (offset <= data.Length - WozLayout.ChunkHeaderLength)
        {
            var id = System.Text.Encoding.ASCII.GetString(data.Slice(offset + WozLayout.ChunkIdOffset, WozLayout.ChunkIdLength));
            var declaredLength = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset + WozLayout.ChunkLengthOffset, WozLayout.ChunkLengthSize));
            offset += WozLayout.ChunkHeaderLength;
            if (declaredLength > int.MaxValue || offset > data.Length - (int)declaredLength)
                throw WozExceptions.TruncatedChunk(id);
            var length = (int)declaredLength;
            chunks[id] = data.Slice(offset, length).ToArray();
            offset += length;
        }
        return chunks;
    }

    /// <summary>
    /// Lit une entrée WOZ1 de taille fixe dans le chunk TRKS.
    /// Les octets de piste occupent le début de l’entrée et le nombre exact de bits est stocké à son offset dédié.
    /// </summary>
    /// <param name="trks">Charge utile du chunk TRKS WOZ1.</param>
    /// <param name="track">Numéro de piste Apple II associé à la référence TMAP.</param>
    /// <param name="index">Index de l’entrée de piste référencée par TMAP.</param>
    /// <returns>Bits de la piste, ou un tableau vide lorsque son nombre de bits est nul ou invalide.</returns>
    /// <exception cref="InvalidDataException">La référence TMAP sort du chunk TRKS.</exception>
    /// <exception cref="OverflowException">Le calcul de la position de l’entrée dépasse les limites d’un entier signé.</exception>
    private static bool[] ReadWoz1Track(ReadOnlySpan<byte> trks, int track, int index)
    {
        var offset = checked(index * WozLayout.Woz1TrackEntryLength);
        if (offset > trks.Length - WozLayout.Woz1TrackEntryLength)
            throw WozExceptions.TrackReferenceOutOfBounds(track, index);
        var bitCount = BinaryPrimitives.ReadUInt16LittleEndian(trks.Slice(offset + WozLayout.Woz1BitCountOffset, WozLayout.Woz1BitCountLength));
        if (bitCount == 0 || bitCount > WozLayout.Woz1BitCountOffset * NibTrackFormat.BitsPerByte) return [];
        return NibTrackImageReader.ConvertToBits(trks.Slice(offset, (bitCount + NibTrackFormat.BitsPerByte - 1) / NibTrackFormat.BitsPerByte), bitCount);
    }

    /// <summary>
    /// Lit un descripteur WOZ2 dans le chunk TRKS, puis extrait les blocs de 512 octets référencés dans le conteneur.
    /// Le descripteur fournit le premier bloc, le nombre de blocs réservés et le nombre exact de bits de la piste.
    /// </summary>
    /// <param name="file">Conteneur WOZ2 complet contenant les blocs de piste.</param>
    /// <param name="trks">Charge utile du chunk TRKS contenant les descripteurs WOZ2.</param>
    /// <param name="track">Numéro de piste Apple II associé à la référence TMAP.</param>
    /// <param name="index">Index du descripteur de piste référencé par TMAP.</param>
    /// <returns>Bits de la piste référencée par le descripteur.</returns>
    /// <exception cref="InvalidDataException">Le descripteur ou les blocs qu’il référence sortent du conteneur.</exception>
    /// <exception cref="OverflowException">Une position ou un nombre de bits dépasse les limites des entiers utilisés.</exception>
    private static bool[] ReadWoz2Track(ReadOnlySpan<byte> file, ReadOnlySpan<byte> trks, int track, int index)
    {
        var descriptorOffset = checked(index * WozLayout.Woz2TrackDescriptorLength);
        if (descriptorOffset > trks.Length - WozLayout.Woz2TrackDescriptorLength)
            throw WozExceptions.TrackReferenceOutOfBounds(track, index);
        var startBlock = BinaryPrimitives.ReadUInt16LittleEndian(trks.Slice(descriptorOffset + WozLayout.Woz2StartBlockOffset, WozLayout.Woz2BlockFieldLength));
        var blockCount = BinaryPrimitives.ReadUInt16LittleEndian(trks.Slice(descriptorOffset + WozLayout.Woz2BlockCountOffset, WozLayout.Woz2BlockFieldLength));
        var bitCount = BinaryPrimitives.ReadUInt32LittleEndian(trks.Slice(descriptorOffset + WozLayout.Woz2BitCountOffset, WozLayout.Woz2BitCountLength));
        var offset = checked(startBlock * WozLayout.Woz2BlockLength);
        var byteCount = checked((int)((bitCount + NibTrackFormat.BitsPerByte - 1) / NibTrackFormat.BitsPerByte));
        if (startBlock == 0 || blockCount == 0 || bitCount == 0 ||
            byteCount > blockCount * WozLayout.Woz2BlockLength || offset > file.Length - byteCount)
            throw WozExceptions.TrackReferenceOutOfBounds(track, index);
        return NibTrackImageReader.ConvertToBits(file.Slice(offset, byteCount), checked((int)bitCount));
    }

    /// <summary>Calcule le CRC32 WOZ des octets fournis.</summary>
    /// <param name="data">Octets couverts par le CRC.</param>
    /// <returns>CRC32 calculé avec le polynôme du format WOZ.</returns>
    private static uint ComputeCrc32(ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;
        foreach (var value in data)
        {
            crc ^= value;
            for (var bit = 0; bit < NibTrackFormat.BitsPerByte; bit++)
                crc = (crc >> 1) ^ (WozFormat.Crc32Polynomial & (uint)-(int)(crc & 1));
        }
        return ~crc;
    }
}
