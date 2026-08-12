using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Apple;
using GWGUI.MediaEngine.Encoding.BitPacking;
using GWGUI.MediaEngine.Primitives;
using GWGUI.MediaEngine.SectorImages;
using GWGUI.MediaEngine.SectorImages.Builders.Apple;

namespace GWGUI.MediaEngine.Containers.Apple.Nib;

/// <summary>Découpe un conteneur NIB Apple II en pistes fixes et transmet leurs bits au décodeur Apple partagé.</summary>
internal static class NibReader
{
    /// <summary>Valide la longueur du conteneur, décode ses pistes puis retient la famille Apple II appropriée.</summary>
    /// <param name="data">Octets consécutifs des pistes NIB.</param>
    /// <returns>Image sectorielle reconstruite à partir des pistes décodées.</returns>
    /// <exception cref="InvalidDataException">La charge utile est vide ou sa longueur n'est pas un multiple positif d'une piste NIB.</exception>
    public static SectorImage Read(ReadOnlySpan<byte> data)
    {
        if (data.Length == 0 || data.Length % NibLayout.TrackLengthBytes != 0) throw NibExceptions.InvalidLength(data.Length, NibLayout.TrackLengthBytes);
        var tracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        var rwtsTracks = new List<(int Track, IReadOnlyList<DecodedSector> Sectors)>();
        var selector = new AppleTrackDecodeSelector();
        for (var track = 0; track < data.Length / NibLayout.TrackLengthBytes; track++)
        {
            var bits = MsbFirstBitPacker.Unpack(data.Slice(track * NibLayout.TrackLengthBytes, NibLayout.TrackLengthBytes), NibLayout.TrackLengthBytes * BitPrimitives.BitsPerByte);
            var result = selector.Decode(bits, track);
            tracks.Add((track, result.StandardSectors));
            rwtsTracks.Add((track, result.Rwts18Sectors));
        }
        if (rwtsTracks.Count(item => item.Sectors.Count > 0) >= AppleTrackSelectionRules.MinimumCredibleRwts18TrackCount) return AppleRwts18SectorImageBuilder.Create(rwtsTracks);
        return AppleIISectorImageBuilder.Create(tracks);
    }
}
