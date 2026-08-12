using GWGUI.MediaEngine.Encoding.BitPacking;

namespace GWGUI.MediaEngine.Containers.Apple.Nib;

/// <summary>Sérialise des pistes binaires Apple dans un conteneur NIB à pistes fixes.</summary>
internal static class NibWriter
{
    /// <summary>Valide, empaquette puis écrit toutes les pistes NIB.</summary>
    /// <param name="tracks">Pistes binaires à sérialiser.</param>
    /// <param name="path">Chemin du fichier de destination.</param>
    /// <param name="cancellationToken">Jeton d'annulation.</param>
    public static async Task WriteAsync(IReadOnlyList<IReadOnlyList<bool>> tracks, string path, CancellationToken cancellationToken = default)
    {
        if (tracks.Count == 0) throw NibExceptions.InvalidTrackCount(tracks.Count);
        for (var track = 0; track < tracks.Count; track++) if (tracks[track].Count > NibLayout.MaximumTrackBitCount) throw NibExceptions.TrackTooLong(track, tracks[track].Count, NibLayout.MaximumTrackBitCount);
        var output = new byte[checked(tracks.Count * NibLayout.TrackLengthBytes)];
        Array.Fill(output, NibLayout.TrackFillByte);
        for (var track = 0; track < tracks.Count; track++) MsbFirstBitPacker.Pack(tracks[track], output.AsSpan(track * NibLayout.TrackLengthBytes, NibLayout.TrackLengthBytes), true);
        await File.WriteAllBytesAsync(path, output, cancellationToken).ConfigureAwait(false);
    }
}
