using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Images.Visualization;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.MediaEngine.Images;

public sealed class SectorImageFluxVisualizer(FluxEncoderRegistry? encoders = null)
{
    private readonly FluxEncoderRegistry _encoders = encoders ?? new FluxEncoderRegistry();
    private readonly SectorImageVisualizationPolicyRegistry _policies = new();

    public bool CanVisualize(SectorImage image) => _policies.Resolve(image) is not null;

    public ScpImage Create(SectorImage image, CancellationToken cancellationToken = default)
    {
        var policy = _policies.Resolve(image) ??
                     throw new NotSupportedException($"No track encoder is available for '{image.FormatId}'.");
        var tracks = new List<ScpTrack>();
        foreach (var group in image.AvailableBlocks
                     .Select(block => (Block: block, Address: policy.VisualAddress(image, block.Address)))
                     .GroupBy(item => (item.Address.Cylinder, item.Address.Head))
                     .OrderBy(group => group.Key.Cylinder).ThenBy(group => group.Key.Head))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sectors = policy.CreateTrackSectors(image,
                group.OrderBy(item => item.Address.Number).ToArray());
            if (sectors.Count == 0) continue;
            var encoded = _encoders.Encode(policy.EncoderId(image),
                new TrackEncodeRequest(group.Key.Cylinder, group.Key.Head, sectors,
                    policy.TrackAttributes(image, sectors.Count),
                    policy.BitCellTicks(image, group.Key.Cylinder)));
            var trackNumber = checked((byte)(group.Key.Cylinder * 2 + group.Key.Head));
            tracks.Add(new(trackNumber, group.Key.Cylinder, group.Key.Head, [encoded.Revolution]));
        }
        if (tracks.Count == 0)
            throw new InvalidDataException("The sector image contains no track that can be visualized.");
        var start = tracks.Min(track => track.TrackNumber);
        var end = tracks.Max(track => track.TrackNumber);
        var heads = tracks.Select(track => track.Head).Distinct().Count() == 1 ? tracks[0].Head == 0 ? ScpHeadSelection.Side0 : ScpHeadSelection.Side1 : ScpHeadSelection.Both;
        var header = new ScpHeader(0, 0, 1, start, end, ScpFlags.IndexAligned | ScpFlags.Writable, ScpBitCellEncoding.Default16Bit, heads, 0, 0);
        return new(header, tracks, true, image.Capacity);
    }

    internal static string? EncoderIdFor(SectorImage image) =>
        new SectorImageVisualizationPolicyRegistry().Resolve(image)?.EncoderId(image);
}
