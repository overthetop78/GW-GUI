using System.Buffers.Binary;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Apple;
using GWGUI.MediaEngine.Encoding.BitPacking;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Containers.Apple.Woz;

/// <summary>Lit les conteneurs WOZ1 et WOZ2 Apple II et sélectionne leurs meilleurs secteurs décodés.</summary>
internal static class WozReader
{
    /// <summary>Valide le conteneur, obtient ses chunks obligatoires puis décode ses pistes.</summary>
    /// <param name="data">Contenu complet du fichier WOZ.</param>
    /// <returns>Image sectorielle reconstruite.</returns>
    public static SectorImage Read(ReadOnlySpan<byte> data)
    {
        var version = ValidateHeader(data);
        var chunks = ReadRequiredChunks(data);
        return DecodeTracks(data, version, chunks.TrackMap.Span, chunks.Tracks.Span);
    }

    /// <summary>Valide la signature, la marque et le CRC puis retourne la version nommée du conteneur.</summary>
    /// <param name="data">Contenu complet du fichier WOZ.</param>
    /// <returns>Version WOZ déterminée depuis la signature.</returns>
    private static WozVersion ValidateHeader(ReadOnlySpan<byte> data)
    {
        if (data.Length < WozLayout.MinimumFileLength || !data.Slice(WozLayout.HeaderMarkerOffset, WozLayout.HeaderMarkerLength).SequenceEqual(WozFormat.HeaderMarker)) throw WozExceptions.InvalidHeader();
        var version = data[..WozLayout.SignatureLength].SequenceEqual(WozFormat.Version1Signature) ? WozVersion.Woz1 : data[..WozLayout.SignatureLength].SequenceEqual(WozFormat.Version2Signature) ? WozVersion.Woz2 : throw WozExceptions.InvalidHeader();
        var storedCrc = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(WozLayout.CrcOffset, WozLayout.CrcLength));
        var computedCrc = WozCrc32.Compute(data[WozLayout.ChunksOffset..]);
        if (storedCrc != computedCrc) throw WozExceptions.InvalidCrc(storedCrc, computedCrc);
        return version;
    }

    /// <summary>Obtient et valide les chunks INFO, TMAP et TRKS obligatoires.</summary>
    /// <param name="data">Contenu complet du fichier WOZ.</param>
    /// <returns>Table des pistes et données de pistes validées.</returns>
    private static (ReadOnlyMemory<byte> TrackMap, ReadOnlyMemory<byte> Tracks) ReadRequiredChunks(ReadOnlySpan<byte> data)
    {
        var chunks = ReadChunks(data);
        if (!chunks.TryGetValue(WozFormat.InfoChunkId, out var info) || info.Length < WozLayout.MinimumInfoLength) throw WozExceptions.MissingRequiredChunk(WozFormat.InfoChunkId);
        if (info.Span[WozLayout.InfoDiskTypeOffset] != WozFormat.AppleII525DiskType) throw WozExceptions.UnsupportedDiskType(info.Span[WozLayout.InfoDiskTypeOffset]);
        if (!chunks.TryGetValue(WozFormat.TrackMapChunkId, out var trackMap) || trackMap.Length < WozLayout.TrackMapLength) throw WozExceptions.MissingRequiredChunk(WozFormat.TrackMapChunkId);
        if (!chunks.TryGetValue(WozFormat.TracksChunkId, out var tracks)) throw WozExceptions.MissingRequiredChunk(WozFormat.TracksChunkId);
        return (trackMap, tracks);
    }

    /// <summary>Décode chaque descripteur de piste et conserve le meilleur candidat de chaque famille Apple.</summary>
    /// <param name="data">Contenu complet du fichier WOZ.</param>
    /// <param name="version">Version WOZ validée.</param>
    /// <param name="trackMap">Table associant les pistes Apple aux descripteurs.</param>
    /// <param name="trackData">Chunk contenant les descripteurs ou données de pistes.</param>
    /// <returns>Image sectorielle reconstruite.</returns>
    private static SectorImage DecodeTracks(ReadOnlySpan<byte> data, WozVersion version, ReadOnlySpan<byte> trackMap, ReadOnlySpan<byte> trackData)
    {
        var selector = new AppleTrackDecodeSelector();
        var standardTracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        var rwts18Tracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        for (var track = 0; track < WozLayout.AppleIITrackCount; track++)
        {
            IReadOnlyList<DecodedSector>? bestStandard = null;
            IReadOnlyList<DecodedSector>? bestRwts18 = null;
            var bestStandardScore = AppleTrackSelectionRules.InitialScore;
            var bestRwts18Score = AppleTrackSelectionRules.InitialScore;
            foreach (var descriptor in trackMap.Slice(track * WozLayout.TrackMapEntriesPerTrack, WozLayout.TrackMapEntriesPerTrack).ToArray().Where(value => value != WozLayout.MissingTrackDescriptor).Distinct())
            {
                var bits = version == WozVersion.Woz1 ? ReadWoz1Track(trackData, track, descriptor) : ReadWoz2Track(data, trackData, track, descriptor);
                if (bits.Length == 0) continue;
                var result = selector.Decode(bits, track);
                if (result.StandardScore > bestStandardScore) { bestStandard = result.StandardSectors; bestStandardScore = result.StandardScore; }
                if (result.Rwts18Score > bestRwts18Score) { bestRwts18 = result.Rwts18Sectors; bestRwts18Score = result.Rwts18Score; }
            }
            if (bestStandard is not null) standardTracks.Add((track, bestStandard));
            if (bestRwts18 is not null) rwts18Tracks.Add((track, bestRwts18));
        }
        return rwts18Tracks.Count(item => item.Sectors.Count > 0) >= AppleTrackSelectionRules.MinimumCredibleRwts18TrackCount ? AppleDiskImageReader.CreateRwts18FromDecodedTracks(rwts18Tracks) : AppleDiskImageReader.CreateAppleIIFromDecodedTracks(standardTracks);
    }

    /// <summary>Parcourt les chunks WOZ et retourne leurs charges utiles indexées par identifiant.</summary>
    /// <param name="data">Contenu complet du fichier WOZ.</param>
    /// <returns>Charges utiles indexées par identifiant de chunk.</returns>
    private static Dictionary<string, ReadOnlyMemory<byte>> ReadChunks(ReadOnlySpan<byte> data)
    {
        var chunks = new Dictionary<string, ReadOnlyMemory<byte>>(StringComparer.Ordinal);
        var offset = WozLayout.ChunksOffset;
        while (offset <= data.Length - WozLayout.ChunkHeaderLength)
        {
            var id = System.Text.Encoding.ASCII.GetString(data.Slice(offset + WozLayout.ChunkIdOffset, WozLayout.ChunkIdLength));
            var declaredLength = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset + WozLayout.ChunkLengthOffset, WozLayout.ChunkLengthSize));
            offset += WozLayout.ChunkHeaderLength;
            if (declaredLength > int.MaxValue || offset > data.Length - (int)declaredLength) throw WozExceptions.TruncatedChunk(id);
            var length = (int)declaredLength;
            chunks[id] = data.Slice(offset, length).ToArray();
            offset += length;
        }
        return chunks;
    }

    /// <summary>Extrait les bits d'une entrée de piste WOZ1.</summary>
    /// <param name="tracks">Charge utile du chunk TRKS.</param>
    /// <param name="track">Numéro de la piste Apple examinée.</param>
    /// <param name="index">Index du descripteur référencé.</param>
    /// <returns>Bits utiles de la piste.</returns>
    private static bool[] ReadWoz1Track(ReadOnlySpan<byte> tracks, int track, int index)
    {
        var offset = checked(index * WozLayout.Woz1TrackEntryLength);
        if (offset > tracks.Length - WozLayout.Woz1TrackEntryLength) throw WozExceptions.TrackReferenceOutOfBounds(track, index);
        var bitCount = BinaryPrimitives.ReadUInt16LittleEndian(tracks.Slice(offset + WozLayout.Woz1BitCountOffset, WozLayout.Woz1BitCountLength));
        if (bitCount == 0 || bitCount > WozLayout.Woz1BitCountOffset * BitPrimitives.BitsPerByte) return [];
        return MsbFirstBitPacker.Unpack(tracks.Slice(offset, MsbFirstBitPacker.RequiredByteCount(bitCount)), bitCount);
    }

    /// <summary>Extrait les bits des blocs référencés par un descripteur WOZ2.</summary>
    /// <param name="file">Contenu complet du fichier WOZ.</param>
    /// <param name="tracks">Charge utile du chunk TRKS.</param>
    /// <param name="track">Numéro de la piste Apple examinée.</param>
    /// <param name="index">Index du descripteur référencé.</param>
    /// <returns>Bits utiles de la piste.</returns>
    private static bool[] ReadWoz2Track(ReadOnlySpan<byte> file, ReadOnlySpan<byte> tracks, int track, int index)
    {
        var descriptorOffset = checked(index * WozLayout.Woz2TrackDescriptorLength);
        if (descriptorOffset > tracks.Length - WozLayout.Woz2TrackDescriptorLength) throw WozExceptions.TrackReferenceOutOfBounds(track, index);
        var startBlock = BinaryPrimitives.ReadUInt16LittleEndian(tracks.Slice(descriptorOffset + WozLayout.Woz2StartBlockOffset, WozLayout.Woz2BlockFieldLength));
        var blockCount = BinaryPrimitives.ReadUInt16LittleEndian(tracks.Slice(descriptorOffset + WozLayout.Woz2BlockCountOffset, WozLayout.Woz2BlockFieldLength));
        var bitCount = BinaryPrimitives.ReadUInt32LittleEndian(tracks.Slice(descriptorOffset + WozLayout.Woz2BitCountOffset, WozLayout.Woz2BitCountLength));
        if (bitCount > int.MaxValue) throw WozExceptions.TrackReferenceOutOfBounds(track, index);
        var offset = checked(startBlock * WozLayout.Woz2BlockLength);
        var byteCount = bitCount == 0 ? 0 : MsbFirstBitPacker.RequiredByteCount((int)bitCount);
        if (startBlock == 0 || blockCount == 0 || bitCount == 0 || byteCount > blockCount * WozLayout.Woz2BlockLength || offset > file.Length - byteCount) throw WozExceptions.TrackReferenceOutOfBounds(track, index);
        return MsbFirstBitPacker.Unpack(file.Slice(offset, byteCount), (int)bitCount);
    }
}
