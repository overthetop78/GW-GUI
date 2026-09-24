namespace GWGUI.MediaEngine.Constants;

/// <summary>Invariant CloneCD descriptor names and sector dimensions used by the supported reader.</summary>
public static class CloneCdConstants
{
    public const string HeaderSection = "CloneCD";
    public const string DiscSection = "Disc";
    public const string SessionSectionPrefix = "Session ";
    public const string EntrySectionPrefix = "Entry ";
    public const string TrackSectionPrefix = "TRACK ";
    public const string VersionKey = "Version";
    public const string SessionsKey = "Sessions";
    public const string TocEntriesKey = "TocEntries";
    public const string SessionKey = "Session";
    public const string PointKey = "Point";
    public const string ControlKey = "Control";
    public const string AdrKey = "ADR";
    public const string TrackNumberKey = "TrackNo";
    public const string AbsoluteSectorKey = "PLBA";
    public const string AbsoluteMinuteKey = "AMin";
    public const string AbsoluteSecondKey = "ASec";
    public const string AbsoluteFrameKey = "AFrame";
    public const string AbsoluteLbaKey = "ALBA";
    public const string ZeroKey = "Zero";
    public const string PointMinuteKey = "PMin";
    public const string PointSecondKey = "PSec";
    public const string PointFrameKey = "PFrame";
    public const string ModeKey = "MODE";
    public const string IsrcKey = "ISRC";
    public const string FlagsKey = "FLAGS";
    public const string IndexKeyPrefix = "INDEX ";
    public const int MinimumVersion = 2;
    public const int MaximumVersion = 3;
    public const int RawSectorSize = OpticalSectorConstants.RawSectorSize;
    public const int SubchannelSize = OpticalSectorConstants.SubchannelSize;
}
