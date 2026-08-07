namespace GWGUI.Scp.Encoding;

public sealed record TrackSector(
    int Number,
    IReadOnlyList<byte> Data,
    bool Deleted = false,
    byte? SizeCode = null,
    IReadOnlyDictionary<string, int>? Attributes = null);

public sealed record TrackEncodeRequest(
    int Cylinder,
    int Head,
    IReadOnlyList<TrackSector> Sectors,
    IReadOnlyDictionary<string, int>? Attributes = null,
    uint BitCellTicks = 40,
    uint IndexTimeTicks = 8_000_000);

public sealed record EncodedTrack(
    string EncoderId,
    IReadOnlyList<bool> Bits,
    ScpRevolution Revolution);

public interface ITrackEncoder
{
    string Id { get; }
    string DisplayName { get; }
    EncodedTrack Encode(TrackEncodeRequest request);
}
