using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Conversion.Scp;

/// <summary>Reconstruit un conteneur SCP synthétique depuis les pistes logiques d'une image sectorielle.</summary>
public sealed class SectorImageScpConversionService(SectorImageTrackEncoder encoder, ScpEncodedTrackFluxService fluxService, ScpWriter writer)
{
    public bool CanCreate(SectorImage image) => encoder.CanEncode(image);

    public async Task ConvertAsync(SectorImage image, string outputPath, CancellationToken cancellationToken = default)
    {
        var scp = Create(image, cancellationToken);
        await writer.WriteAsync(outputPath, scp, cancellationToken).ConfigureAwait(false);
    }

    public ScpImage Create(SectorImage image, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        var encoded = encoder.Encode(image, cancellationToken);
        if (encoded.Count == 0) throw SectorImageScpConversionExceptions.MissingTrack(image.FormatId);
        var tracks = encoded.Select(track => CreateTrack(track)).ToArray();
        var startTrack = tracks.Min(track => track.TrackNumber);
        var endTrack = tracks.Max(track => track.TrackNumber);
        var flags = CreateFlags(encoded);
        var heads = CreateHeadSelection(tracks);
        var header = new ScpHeader(ScpWriterDefaults.Version, (byte)ScpDiskTypeCatalog.Resolve(image), ScpWriterDefaults.RevolutionCount, startTrack, endTrack, flags, ScpBitCellEncoding.Default16Bit, heads, ScpWriterDefaults.Resolution, ScpFormatConstants.MissingChecksum);
        return new ScpImage(header, tracks, true, ScpWriterDefaults.InitialFileSize);
    }

    private ScpTrack CreateTrack(EncodedDiskTrack encoded)
    {
        var trackNumber = ScpFormatConstants.ToTrackNumber(encoded.Cylinder, encoded.Head);
        var revolution = fluxService.Create(encoded.Track, ScpWriterDefaults.Resolution);
        return new ScpTrack(trackNumber, encoded.Cylinder, encoded.Head, [revolution]);
    }

    private static ScpFlags CreateFlags(IReadOnlyList<EncodedDiskTrack> tracks)
    {
        var flags = ScpFlags.IndexAligned | ScpFlags.Normalized | ScpFlags.ThirdPartyCreator;
        if (tracks.Max(track => track.Cylinder) > 41) flags |= ScpFlags.Tpi96;
        if (tracks.Any(track => track.Track.Revolution.IndexTimeTicks == TrackEncodingTimings.Rpm360IndexTimeTicks)) flags |= ScpFlags.Rpm360;
        return flags;
    }

    private static ScpHeadSelection CreateHeadSelection(IReadOnlyList<ScpTrack> tracks)
    {
        var heads = tracks.Select(track => track.Head).Distinct().Order().ToArray();
        if (heads.SequenceEqual([0])) return ScpHeadSelection.Side0;
        if (heads.SequenceEqual([1])) return ScpHeadSelection.Side1;
        return ScpHeadSelection.Both;
    }
}
