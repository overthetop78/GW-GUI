using GWGUI.MediaEngine.Formats.Floppy.Scp;
using GWGUI.MediaEngine.Representations.Flux;

namespace GWGUI.MediaEngine.Formats.Floppy.I86f;

/// <summary>Exposes 86F bit cells through the common flux representation without decoding sectors.</summary>
public static class I86fProtectedTrackImageAdapter
{
    public static ProtectedTrackImage Create(I86fImage image)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (image.Tracks.Count == 0) throw new InvalidDataException("The 86F image does not contain any track data.");
        var bitCellNanoseconds = I86fLayout.TicksPerBitCell * (double)ScpFormatConstants.ResolutionStepNanoseconds;
        var tracks = image.Tracks.Select(track =>
        {
            var cylinder = track.LogicalIndex % I86fLayout.TrackTableEntriesPerSide;
            var head = track.LogicalIndex / I86fLayout.TrackTableEntriesPerSide;
            var indexTime = checked((uint)(track.Bits.Count * (long)I86fLayout.TicksPerBitCell));
            var revolution = FluxRevolutionFactory.Create(track.Bits, I86fLayout.TicksPerBitCell, indexTime);
            return new ProtectedTrack(
                cylinder,
                head,
                track.Bits,
                [new TrackTimingSegment(0, track.Bits.Count, bitCellNanoseconds)],
                [],
                [],
                [new TrackFluxRevolution(ScpFormatConstants.ResolutionStepNanoseconds, revolution)]);
        }).ToArray();
        return new ProtectedTrackImage(tracks, false);
    }
}
