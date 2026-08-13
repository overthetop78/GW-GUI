namespace GWGUI.MediaEngine.TrackImages;

/// <summary>Décrit le timing uniforme d'une plage de cellules binaires.</summary>
public sealed record TrackTimingSegment(int BitOffset, int BitLength, double BitCellNanoseconds);
