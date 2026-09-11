namespace GWGUI.MediaEngine.Representations.Flux;

/// <summary>Décrit le timing uniforme d'une plage de cellules binaires.</summary>
public sealed record TrackTimingSegment(int BitOffset, int BitLength, double BitCellNanoseconds);
