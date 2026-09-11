namespace GWGUI.MediaEngine.Contracts;

/// <summary>Contains the validated fields of one MDS track block before its MDF range is resolved.</summary>
public sealed record AlcoholMdsTrackBlock(
    int SessionNumber,
    int TrackNumber,
    byte Mode,
    byte SubchannelMode,
    byte AdrControl,
    int SectorSize,
    long StartSector,
    long StartOffset,
    long FooterOffset,
    string DataPath);
