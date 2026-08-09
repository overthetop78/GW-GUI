namespace GWGUI.Scp;

[Flags]
public enum ScpFlags : byte
{
    IndexAligned = 1,
    Tpi96 = 2,
    Rpm360 = 4,
    Normalized = 8,
    Writable = 16,
    Footer = 32,
    Extended = 64,
    ThirdPartyCreator = 128
}

public sealed record ScpHeader(
    byte Version,
    byte DiskType,
    byte Revolutions,
    byte StartTrack,
    byte EndTrack,
    ScpFlags Flags,
    byte BitCellEncoding,
    byte Heads,
    byte Resolution,
    uint Checksum)
{
    public int TrackCount => EndTrack - StartTrack + 1;
    public int ResolutionNanoseconds => 25 * (Resolution + 1);
    public string VersionText => $"{Version >> 4}.{Version & 0x0f}";
}

public sealed record ScpRevolution(uint IndexTimeTicks, uint DeclaredFluxCount, IReadOnlyList<uint> FluxIntervals)
{
    public double DurationMilliseconds(int resolutionNanoseconds) => IndexTimeTicks * resolutionNanoseconds / 1_000_000d;
    public double Rpm(int resolutionNanoseconds) => 60_000d / DurationMilliseconds(resolutionNanoseconds);
}

public sealed record ScpTrack(byte TrackNumber, int Cylinder, int Head, IReadOnlyList<ScpRevolution> Revolutions);
public sealed record ScpImage(ScpHeader Header, IReadOnlyList<ScpTrack> Tracks, bool ChecksumValid, long FileSize);
