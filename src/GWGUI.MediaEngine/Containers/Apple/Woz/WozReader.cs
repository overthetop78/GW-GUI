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
    /// L’en-tête est invalide, un chunk est tronqué, ou les chunks TMAP et TRKS requis sont absents.
    /// </exception>
    /// <exception cref="NotSupportedException">Le conteneur ne décrit pas une disquette Apple II 5,25 pouces.</exception>
    /// <exception cref="OverflowException">Une longueur ou une position déclarée dépasse les limites des entiers utilisés.</exception>
    public static SectorImage Read(ReadOnlySpan<byte> data)
    {
        if (data.Length < 256 || !(data[..4].SequenceEqual("WOZ1"u8) || data[..4].SequenceEqual("WOZ2"u8)) ||
            !data.Slice(4, 4).SequenceEqual(new byte[] { 0xff, 0x0a, 0x0d, 0x0a }))
            throw new InvalidDataException("The WOZ header is invalid.");
        var version = data[3] - (byte)'0';
        var chunks = ReadChunks(data);
        if (!chunks.TryGetValue("INFO", out var info) || info.Length < 2 || info.Span[1] != 1)
            throw new NotSupportedException("Only Apple II 5.25-inch WOZ images are supported by this reader.");
        if (!chunks.TryGetValue("TMAP", out var tmap) || tmap.Length < 160 || !chunks.TryGetValue("TRKS", out var trks))
            throw new InvalidDataException("The WOZ track map or track data is missing.");

        var decoder = new AppleGcrDecoder();
        var rwtsDecoder = new AppleRwts18Decoder();
        var tracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        var rwtsTracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        for (var track = 0; track < 40; track++)
        {
            IReadOnlyList<DecodedSector>? best = null;
            IReadOnlyList<DecodedSector>? bestRwts = null;
            var bestScore = -1;
            var bestRwtsScore = -1;
            foreach (var descriptor in tmap.Span.Slice(track * 4, 4).ToArray().Where(value => value != 0xff).Distinct())
            {
                var bits = version == 1 ? ReadWoz1Track(trks.Span, descriptor) : ReadWoz2Track(data, trks.Span, descriptor);
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
        var offset = 12;
        while (offset <= data.Length - 8)
        {
            var id = System.Text.Encoding.ASCII.GetString(data.Slice(offset, 4));
            var length = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(offset + 4, 4)));
            offset += 8;
            if (length < 0 || offset > data.Length - length)
                throw new InvalidDataException($"The WOZ {id} chunk is truncated.");
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
    /// <param name="index">Index de l’entrée de piste référencée par TMAP.</param>
    /// <returns>Bits de la piste, ou un tableau vide lorsque l’entrée ou son nombre de bits est invalide.</returns>
    /// <exception cref="OverflowException">Le calcul de la position de l’entrée dépasse les limites d’un entier signé.</exception>
    private static bool[] ReadWoz1Track(ReadOnlySpan<byte> trks, int index)
    {
        const int entryLength = 6656;
        const int bitCountOffset = 6648;
        var offset = checked(index * entryLength);
        if (offset > trks.Length - entryLength) return [];
        var bitCount = BinaryPrimitives.ReadUInt16LittleEndian(trks.Slice(offset + bitCountOffset, 2));
        if (bitCount == 0 || bitCount > bitCountOffset * 8) return [];
        return NibTrackImageReader.ConvertToBits(trks.Slice(offset, (bitCount + 7) / 8), bitCount);
    }

    /// <summary>
    /// Lit un descripteur WOZ2 dans le chunk TRKS, puis extrait les blocs de 512 octets référencés dans le conteneur.
    /// Le descripteur fournit le premier bloc, le nombre de blocs réservés et le nombre exact de bits de la piste.
    /// </summary>
    /// <param name="file">Conteneur WOZ2 complet contenant les blocs de piste.</param>
    /// <param name="trks">Charge utile du chunk TRKS contenant les descripteurs WOZ2.</param>
    /// <param name="index">Index du descripteur de piste référencé par TMAP.</param>
    /// <returns>Bits de la piste, ou un tableau vide lorsque le descripteur ou sa plage est invalide.</returns>
    /// <exception cref="OverflowException">Une position ou un nombre de bits dépasse les limites des entiers utilisés.</exception>
    private static bool[] ReadWoz2Track(ReadOnlySpan<byte> file, ReadOnlySpan<byte> trks, int index)
    {
        var descriptorOffset = checked(index * 8);
        if (descriptorOffset > trks.Length - 8) return [];
        var startBlock = BinaryPrimitives.ReadUInt16LittleEndian(trks.Slice(descriptorOffset, 2));
        var blockCount = BinaryPrimitives.ReadUInt16LittleEndian(trks.Slice(descriptorOffset + 2, 2));
        var bitCount = BinaryPrimitives.ReadUInt32LittleEndian(trks.Slice(descriptorOffset + 4, 4));
        var offset = checked(startBlock * 512);
        var byteCount = checked((int)((bitCount + 7) / 8));
        if (startBlock == 0 || blockCount == 0 || bitCount == 0 || byteCount > blockCount * 512 ||
            offset > file.Length - byteCount) return [];
        return NibTrackImageReader.ConvertToBits(file.Slice(offset, byteCount), checked((int)bitCount));
    }
}
