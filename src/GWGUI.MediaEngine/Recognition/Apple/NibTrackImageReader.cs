using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Apple;
using GWGUI.MediaEngine.Containers.Apple.Nib;
using GWGUI.MediaEngine.Encoding.BitPacking;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Primitives;
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
            throw NibExceptions.InvalidLength(data.Length, NibTrackFormat.TrackLength);
        var tracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        var rwtsTracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        var selector = new AppleTrackDecodeSelector();
        for (var track = 0; track < data.Length / NibTrackFormat.TrackLength; track++)
        {
            var bits = MsbFirstBitPacker.Unpack(data.Slice(track * NibTrackFormat.TrackLength, NibTrackFormat.TrackLength), NibTrackFormat.TrackLength * BitPrimitives.BitsPerByte);
            var result = selector.Decode(bits, track);
            tracks.Add((track, result.StandardSectors));
            rwtsTracks.Add((track, result.Rwts18Sectors));
        }
        if (rwtsTracks.Count(item => item.Sectors.Count > 0) >= AppleTrackSelectionRules.MinimumCredibleRwts18TrackCount)
            return AppleDiskImageReader.CreateRwts18FromDecodedTracks(rwtsTracks);
        return AppleDiskImageReader.CreateAppleIIFromDecodedTracks(tracks);
    }

}
