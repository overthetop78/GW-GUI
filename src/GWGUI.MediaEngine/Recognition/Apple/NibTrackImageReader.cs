using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Recognition.Apple;

/// <summary>
/// Découpe une image NIB Apple II en pistes de longueur fixe et transmet leurs bits aux décodeurs GCR Apple.
/// </summary>
internal static class NibTrackImageReader
{
    /// <summary>
    /// Valide la longueur de l’image NIB, décode chaque piste avec les codecs Apple II et RWTS18,
    /// puis conserve la famille ayant produit la structure sectorielle pertinente.
    /// </summary>
    /// <param name="data">Octets consécutifs des pistes NIB.</param>
    /// <returns>L’image sectorielle reconstruite à partir des pistes décodées.</returns>
    /// <exception cref="InvalidDataException">La charge utile est vide ou sa longueur n’est pas un multiple d’une piste NIB.</exception>
    public static SectorImage Read(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0 || data.Length % NibTrackFormat.TrackLength != 0)
            throw NibTrackExceptions.InvalidLength(data.Length, NibTrackFormat.TrackLength);
        var tracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        var rwtsTracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        var decoder = new AppleGcrDecoder();
        var rwtsDecoder = new AppleRwts18Decoder();
        for (var track = 0; track < data.Length / NibTrackFormat.TrackLength; track++)
        {
            var bits = ConvertToBits(data.Slice(track * NibTrackFormat.TrackLength, NibTrackFormat.TrackLength), NibTrackFormat.TrackLength * NibTrackFormat.BitsPerByte);
            tracks.Add((track, decoder.DecodeBits(bits).Sectors ?? []));
            rwtsTracks.Add((track, rwtsDecoder.DecodeBits(bits).Sectors ?? []));
        }
        if (rwtsTracks.Count(item => item.Sectors.Any(sector => sector.Data is { Count: 768 })) > 1)
            return AppleDiskImageReader.CreateRwts18FromDecodedTracks(rwtsTracks);
        return AppleDiskImageReader.CreateAppleIIFromDecodedTracks(tracks);
    }

    /// <summary>Convertit des octets en bits ordonnés du bit de poids fort vers le bit de poids faible.</summary>
    /// <param name="bytes">Octets contenant le flux de bits.</param>
    /// <param name="bitCount">Nombre de bits à produire depuis le début de la séquence.</param>
    /// <returns>Tableau contenant exactement le nombre de bits demandé.</returns>
    /// <exception cref="IndexOutOfRangeException">La séquence ne contient pas le nombre de bits demandé.</exception>
    internal static bool[] ConvertToBits(ReadOnlySpan<byte> bytes, int bitCount)
    {
        var bits = new bool[bitCount];
        for (var bit = 0; bit < bitCount; bit++)
            bits[bit] = (bytes[bit / NibTrackFormat.BitsPerByte] &
                         (1 << (NibTrackFormat.BitsPerByte - 1 - bit % NibTrackFormat.BitsPerByte))) != 0;
        return bits;
    }
}
