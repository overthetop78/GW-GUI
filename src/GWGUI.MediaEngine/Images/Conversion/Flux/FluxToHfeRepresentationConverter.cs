using System.Globalization;
using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats;
using GWGUI.MediaEngine.Images.Formats.Floppy.Hfe;
using GWGUI.MediaEngine.Images.Models.Flux;
using GWGUI.MediaEngine.Images.Reading;
using GWGUI.MediaEngine.Interfaces.Conversion;

namespace GWGUI.MediaEngine.Images.Conversion.Flux;

internal sealed class FluxToHfeRepresentationConverter : IMediaRepresentationConverter
{
    private static readonly IReadOnlySet<MediaRepresentationKind> Kinds = new HashSet<MediaRepresentationKind> { MediaRepresentationKind.Flux };
    public string Id => "flux-to-hfe-cells";
    public IReadOnlySet<MediaRepresentationKind> SourceRepresentationKinds => Kinds;
    public IReadOnlySet<MediaRepresentationKind> TargetRepresentationKinds => Kinds;

    public bool CanConvert(MediaImageDocument source, string targetFormatId, MediaRepresentationKind targetRepresentationKind)
        => targetRepresentationKind == MediaRepresentationKind.Flux
           && source.Representation is FluxMediaImageRepresentation flux
           && flux.Tracks.Count > 0 && flux.Tracks.All(track => track.Revolutions.Count > 0)
           && (targetFormatId.Equals(DiskImageFormatIds.RawHfe, StringComparison.OrdinalIgnoreCase)
               || DiskDefsFormatReader.Definitions.TryGetValue(targetFormatId, out var definition)
               && definition.Tracks.All(track => track.Codec == "bitcell"));

    public Task<MediaRepresentationConversionResult> ConvertAsync(MediaImageDocument source, string targetFormatId,
        MediaRepresentationKind targetRepresentationKind, IReadOnlyDictionary<string, string> options,
        Action<MediaExplorationProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        if (!CanConvert(source, targetFormatId, targetRepresentationKind))
            throw new NotSupportedException($"The flux cannot be prepared for HFE target '{targetFormatId}'.");
        var flux = (FluxMediaImageRepresentation)source.Representation;
        double nanoseconds;
        if (DiskDefsFormatReader.Definitions.TryGetValue(targetFormatId, out var definition))
            nanoseconds = double.Parse(definition.Tracks[0].Parameters["clock"], CultureInfo.InvariantCulture) * 1000;
        else
        {
            var estimates = flux.Tracks.Select(track =>
                FluxTimingEstimator.EstimateNonFmBitCell(track.Revolutions[0].Flux.FluxIntervals)
                * track.Revolutions[0].ResolutionNanoseconds).Order().ToArray();
            var estimate = estimates[estimates.Length / 2];
            nanoseconds = new[] { 500d, 1000d, 2000d, 4000d }.MinBy(value => Math.Abs(value - estimate));
        }
        var decoded = new List<(ProtectedTrack Track, bool[] Bits)>();
        foreach (var track in flux.Tracks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var revolution = track.Revolutions[0];
            var cells = FluxTransitionDecoder.DecodePll(revolution.Flux.FluxIntervals,
                nanoseconds / revolution.ResolutionNanoseconds);
            var count = checked((int)Math.Round(revolution.Flux.IndexTimeTicks * (double)revolution.ResolutionNanoseconds / nanoseconds));
            count = checked((count + 7) / 8 * 8);
            var bits = new bool[count];
            cells.Bits.AsSpan(0, Math.Min(count, cells.Bits.Length)).CopyTo(bits);
            decoded.Add((track, bits));
        }
        var tracks = new List<ProtectedTrack>();
        foreach (var cylinder in decoded.GroupBy(item => item.Track.Cylinder))
        {
            var count = cylinder.Max(item => item.Bits.Length);
            foreach (var item in cylinder)
            {
                var bits = item.Bits;
                if (bits.Length != count) Array.Resize(ref bits, count);
                tracks.Add(new(item.Track.Cylinder, item.Track.Head, bits,
                    [new TrackTimingSegment(0, count, nanoseconds)], [], [], []));
            }
        }
        var metadata = new Dictionary<string, string>(source.Metadata, StringComparer.Ordinal);
        metadata.Remove(HfeMetadataKeys.BitRate);
        if (definition is not null)
            metadata[HfeMetadataKeys.Encoding] = (nanoseconds == 4000 ? HfeFormat.IsoFmEncoding : HfeFormat.IsoMfmEncoding).ToString(CultureInfo.InvariantCulture);
        var document = MediaImageDocumentFactory.CreateFloppyFlux(source.Source, targetFormatId,
            new ProtectedTrackImage(tracks, flux.Image.WriteProtected), metadata);
        return Task.FromResult(new MediaRepresentationConversionResult(document, [],
            ["HFE stores one revolution per track; flux timing is quantized to uniform bit cells."]));
    }
}
