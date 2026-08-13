namespace GWGUI.MediaEngine.Containers.Scp;

/// <summary>Construit les diagnostics de validation du writer SCP.</summary>
internal static class ScpWriterExceptions
{
    public static InvalidDataException InvalidRevolutionCount(int count) => new($"SCP requires between {ScpFormatConstants.MinimumRevolutionCount} and {ScpFormatConstants.MaximumRevolutionCount} revolutions; {count} was supplied.");
    public static InvalidDataException InvalidTrackRange(byte start, byte end) => new($"SCP track range {start}..{end} is invalid.");
    public static InvalidDataException DuplicateTrack(byte track) => new($"SCP track {track} is present more than once.");
    public static InvalidDataException TrackOutsideRange(byte track, byte start, byte end) => new($"SCP track {track} is outside declared range {start}..{end}.");
    public static InvalidDataException TrackAddressMismatch(byte track, int cylinder, int head) => new($"SCP track {track} does not match declared address cylinder {cylinder}, head {head}.");
    public static InvalidDataException RevolutionCountMismatch(byte track, int expected, int actual) => new($"SCP track {track} contains {actual} revolutions; {expected} are required.");
    public static InvalidDataException EmptyFluxInterval(byte track, int revolution) => new($"SCP track {track}, revolution {revolution} contains a zero-length flux interval.");
    public static NotSupportedException ExtendedMedia() => new("Writing extended SCP media is not supported.");
}
