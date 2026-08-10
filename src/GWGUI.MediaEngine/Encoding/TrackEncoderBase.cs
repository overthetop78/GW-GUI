namespace GWGUI.MediaEngine.Encoding;

public abstract class TrackEncoderBase : ITrackEncoder
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }

    public EncodedTrack Encode(TrackEncodeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Cylinder is < 0 or > 255) throw new ArgumentOutOfRangeException(nameof(request.Cylinder));
        if (request.Head is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(request.Head));
        if (request.Sectors.Count == 0) throw new ArgumentException("At least one sector is required.", nameof(request));
        var bits = EncodeBits(request);
        if (bits.Count == 0) throw new InvalidOperationException($"Encoder {Id} produced an empty track.");
        return new(Id, bits, TrackEncoding.ToRevolution(bits, request.BitCellTicks, request.IndexTimeTicks));
    }

    protected abstract IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request);

    protected static int Attribute(TrackEncodeRequest request, string key, int fallback) =>
        request.Attributes is not null && request.Attributes.TryGetValue(key, out var value) ? value : fallback;

    protected static int Attribute(TrackSector sector, string key, int fallback) =>
        sector.Attributes is not null && sector.Attributes.TryGetValue(key, out var value) ? value : fallback;
}
