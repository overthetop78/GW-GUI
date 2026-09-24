namespace GWGUI.MediaEngine.Contracts;

/// <summary>Contains the understood numeric fields of one CloneCD table-of-contents entry.</summary>
public sealed record CloneCdTocEntry(
    int EntryNumber,
    int SessionNumber,
    int Point,
    int Adr,
    int Control,
    int TrackNumber,
    int AbsoluteMinute,
    int AbsoluteSecond,
    int AbsoluteFrame,
    long AbsoluteLba,
    int Zero,
    int PointMinute,
    int PointSecond,
    int PointFrame,
    long PointLba);
